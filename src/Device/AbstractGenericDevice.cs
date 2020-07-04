using DebugUtils;
using System;
using System.IO.Ports;
using ToySerialController.MotionSource;
using UnityEngine;

namespace ToySerialController
{
    public abstract partial class AbstractGenericDevice : IDevice
    {
        private Vector3 _xTarget, _xTargetMax, _xTargetMin, _xCmd;
        private Vector3 _rTarget, _rTargetMin, _rTargetMax, _rCmd;
        private float[] _vCmd;

        protected string DeviceReport { get; set; }
        protected string SerialReport { get; set; }

        public string GetDeviceReport() => DeviceReport;
        public string GetSerialReport() => SerialReport;

        protected AbstractGenericDevice()
        {
            _xTargetMax = _rTargetMax = Vector3.one * float.MinValue;
            _xTargetMin = _rTargetMin = Vector3.one * float.MaxValue;
            _vCmd = new float[9];
        }

        //TODO: detect true insertion
        public bool Update(IMotionSource motionSource)
        {
            var diff = motionSource.TargetPosition - motionSource.ReferencePosition;

            Vector3 projectionNormal;
            if (ProjectXChooser.val == "Difference") projectionNormal = diff.normalized;
            else if (ProjectXChooser.val == "Reference Up") projectionNormal = motionSource.ReferenceUp;
            else if (ProjectXChooser.val == "Target Up") projectionNormal = motionSource.TargetUp;
            else return false;

            if (diff.magnitude > 0.00001f)
            {
                var diffOnNormal = Vector3.Project(diff, projectionNormal);
                var diffOnPlane = Vector3.ProjectOnPlane(diff, motionSource.ReferencePlaneNormal);
                var yOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceRight);
                var zOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceForward);

                if(ActivationDistanceSlider.val > 0)
                    if (diffOnNormal.magnitude / motionSource.ReferenceLength > ActivationDistanceSlider.val)
                        return false;

                _xTarget.x = 1 - Mathf.Clamp01(diffOnNormal.magnitude / motionSource.ReferenceLength);
                if (Vector3.Dot(diff, motionSource.ReferenceUp) < 0)
                    _xTarget.x = _xTarget.x > 0 ? 1 : 0;

                _xTarget.y = yOffset.magnitude * Mathf.Sign(Vector3.Dot(yOffset, motionSource.ReferenceRight));
                _xTarget.z = zOffset.magnitude * Mathf.Sign(Vector3.Dot(zOffset, motionSource.ReferenceForward));

                DebugDraw.DrawRay(motionSource.ReferencePosition, diffOnNormal, 1f, new Color(0.3f, 0.3f, 0.3f));
            }
            else
            {
                _xTarget = new Vector3(1, 0, 0);
            }

            var twistAngle = Vector3.SignedAngle(motionSource.ReferenceRight, Vector3.ProjectOnPlane(motionSource.TargetRight, motionSource.ReferenceUp), motionSource.ReferenceUp);
            _rTarget.x = (twistAngle < 0 ? twistAngle + 360 : twistAngle) / 360;
            _rTarget.y = Vector3.Dot(motionSource.ReferenceRight, motionSource.TargetUp);
            _rTarget.z = Vector3.Dot(motionSource.ReferenceForward, motionSource.TargetUp);

            for (var i = 0; i < 3; i++)
            {
                _xTargetMax[i] = Math.Max(_xTarget[i], _xTargetMax[i]);
                _xTargetMin[i] = Math.Min(_xTarget[i], _xTargetMin[i]);
                _rTargetMax[i] = Math.Max(_rTarget[i], _rTargetMax[i]);
                _rTargetMin[i] = Math.Min(_rTarget[i], _rTargetMin[i]);
            }

            var xt = (_xTarget.x - RangeMinXSlider.val) / (RangeMaxXSlider.val - RangeMinXSlider.val);
            var yt = (_xTarget.y + RangeMaxYSlider.val) / (2 * RangeMaxYSlider.val);
            var zt = (_xTarget.z + RangeMaxZSlider.val) / (2 * RangeMaxZSlider.val);
            var rxt = 0.5f + OutputRXCurveEditorSettings.Evaluate(_xTarget, _rTarget);
            var ryt = (_rTarget.y + RangeMaxRYSlider.val) / (2 * RangeMaxRYSlider.val);
            var rzt = (_rTarget.z + RangeMaxRZSlider.val) / (2 * RangeMaxRZSlider.val);
            var v0t = OutputV0CurveEditorSettings.Evaluate(_xTarget, _rTarget);
            var v1t = OutputV1CurveEditorSettings.Evaluate(_xTarget, _rTarget);

            xt = Mathf.Clamp01(xt);
            yt = Mathf.Clamp01(yt);
            zt = Mathf.Clamp01(zt);
            rxt = Mathf.Clamp01(rxt);
            ryt = Mathf.Clamp01(ryt);
            rzt = Mathf.Clamp01(rzt);
            v0t = Mathf.Clamp01(v0t);
            v1t = Mathf.Clamp01(v1t);

            var xCmdRaw = Mathf.Lerp(OutputMinXSlider.val, OutputMaxXSlider.val, xt);
            var yCmdRaw = AdjustYSlider.val + 0.5f + Mathf.Lerp(-OutputMaxYSlider.val, OutputMaxYSlider.val, yt);
            var zCmdRaw = AdjustZSlider.val + 0.5f + Mathf.Lerp(-OutputMaxZSlider.val, OutputMaxZSlider.val, zt);
            var rxCmdRaw = 0.5f + Mathf.Lerp(-OutputMaxRXSlider.val, OutputMaxRXSlider.val, rxt);
            var ryCmdRaw = AdjustRYSlider.val + 0.5f + Mathf.Lerp(-OutputMaxRYSlider.val, OutputMaxRYSlider.val, ryt);
            var rzCmdRaw = AdjustRZSlider.val + 0.5f + Mathf.Lerp(-OutputMaxRZSlider.val, OutputMaxRZSlider.val, rzt);
            var v0CmdRaw = v0t;
            var v1CmdRaw = v1t;

            xCmdRaw = Mathf.Clamp01(xCmdRaw);
            yCmdRaw = Mathf.Clamp01(yCmdRaw);
            zCmdRaw = Mathf.Clamp01(zCmdRaw);
            rxCmdRaw = Mathf.Clamp01(rxCmdRaw);
            ryCmdRaw = Mathf.Clamp01(ryCmdRaw);
            rzCmdRaw = Mathf.Clamp01(rzCmdRaw);
            v0CmdRaw = Mathf.Clamp01(v0CmdRaw);
            v1CmdRaw = Mathf.Clamp01(v1CmdRaw);

            if (InvertXToggle.val) xCmdRaw = 1f - xCmdRaw;
            if (InvertYToggle.val) yCmdRaw = 1f - yCmdRaw;
            if (InvertZToggle.val) zCmdRaw = 1f - zCmdRaw;
            if (InvertRXToggle.val) rxCmdRaw = 1f - rxCmdRaw;
            if (InvertRYToggle.val) ryCmdRaw = 1f - ryCmdRaw;
            if (InvertRZToggle.val) rzCmdRaw = 1f - rzCmdRaw;

            if (EnableOverrideXToggle.val) xCmdRaw = OverrideXSlider.val;
            if (EnableOverrideYToggle.val) yCmdRaw = OverrideYSlider.val;
            if (EnableOverrideZToggle.val) zCmdRaw = OverrideZSlider.val;
            if (EnableOverrideRXToggle.val) rxCmdRaw = OverrideRXSlider.val;
            if (EnableOverrideRYToggle.val) ryCmdRaw = OverrideRYSlider.val;
            if (EnableOverrideRZToggle.val) rzCmdRaw = OverrideRZSlider.val;
            if (EnableOverrideV0Toggle.val) v0CmdRaw = OverrideV0Slider.val;
            if (EnableOverrideV1Toggle.val) v1CmdRaw = OverrideV1Slider.val;

            _xCmd.x = Mathf.Lerp(_xCmd.x, xCmdRaw, 1 - SmoothingSlider.val);
            _xCmd.y = Mathf.Lerp(_xCmd.y, yCmdRaw, 1 - SmoothingSlider.val);
            _xCmd.z = Mathf.Lerp(_xCmd.z, zCmdRaw, 1 - SmoothingSlider.val);
            _rCmd.x = Mathf.Lerp(_rCmd.x, rxCmdRaw, 1 - SmoothingSlider.val);
            _rCmd.y = Mathf.Lerp(_rCmd.y, ryCmdRaw, 1 - SmoothingSlider.val);
            _rCmd.z = Mathf.Lerp(_rCmd.z, rzCmdRaw, 1 - SmoothingSlider.val);
            _vCmd[0] = Mathf.Lerp(_vCmd[0], v0CmdRaw, 1 - SmoothingSlider.val);
            _vCmd[1] = Mathf.Lerp(_vCmd[1], v1CmdRaw, 1 - SmoothingSlider.val);

            var s = "          Min      Cur      Max     Cmd\n";
            s += string.Format("   X\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _xTargetMin.x, _xTarget.x, _xTargetMax.x, _xCmd.x);
            s += string.Format("   Y\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _xTargetMin.y, _xTarget.y, _xTargetMax.y, _xCmd.y);
            s += string.Format("   Z\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _xTargetMin.z, _xTarget.z, _xTargetMax.z, _xCmd.z);
            s += string.Format("RX\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _rTargetMin.x, _rTarget.x, _rTargetMax.x, _rCmd.x);
            s += string.Format("RY\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _rTargetMin.y, _rTarget.y, _rTargetMax.y, _rCmd.y);
            s += string.Format("RZ\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _rTargetMin.z, _rTarget.z, _rTargetMax.z, _rCmd.z);
            s += string.Format("V0\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", 0, 0, 0, _vCmd[0]);
            s += string.Format("V1\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}", 0, 0, 0, _vCmd[1]);
            DeviceReport = s;

            DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMinXSlider.val * motionSource.ReferenceLength, motionSource.TargetUp, Color.white, 0.05f);
            DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMaxXSlider.val * motionSource.ReferenceLength, motionSource.TargetUp, Color.white, 0.05f);

            return true;
        }

        public abstract void Write(SerialPort serial, Vector3 xCmd, Vector3 rCmd, float[] _vCmd);

        public void Write(SerialPort serial)
            => Write(serial, _xCmd, _rCmd, _vCmd);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing) { }
    }
}
