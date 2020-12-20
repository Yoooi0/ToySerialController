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

        public bool Update(IMotionSource motionSource)
        {
            var length = motionSource.ReferenceLength * ReferenceLengthScaleSlider.val;
            var radius = motionSource.ReferenceRadius * ReferenceRadiusScaleSlider.val;
            var referenceEnding = motionSource.ReferencePosition + motionSource.ReferenceUp * length;
            var diffPosition = motionSource.TargetPosition - motionSource.ReferencePosition;
            var diffEnding = motionSource.TargetPosition - referenceEnding;
            var aboveTarget = (Vector3.Dot(diffPosition, motionSource.TargetUp) < 0 && Vector3.Dot(diffEnding, motionSource.TargetUp) < 0)
                                || Vector3.Dot(diffPosition, motionSource.ReferenceUp) < 0;

            for (var i = 0; i < 5; i++)
                DebugDraw.DrawCircle(Vector3.Lerp(motionSource.ReferencePosition, referenceEnding, i / 4.0f), motionSource.ReferenceUp, Color.grey, radius);

            if (diffPosition.magnitude > 0.00001f)
            {
                var t = Mathf.Clamp(Vector3.Dot(motionSource.TargetPosition - motionSource.ReferencePosition, motionSource.ReferenceUp), 0f, length);
                var closestPoint = motionSource.ReferencePosition + motionSource.ReferenceUp * t;

                _xTarget.x = 1 - Mathf.Clamp01((closestPoint - motionSource.ReferencePosition).magnitude / length);
                if (aboveTarget)
                    _xTarget.x = _xTarget.x > 0 ? 1 : 0;
                else if (Vector3.Magnitude(closestPoint - motionSource.TargetPosition) > radius)
                    return false;

                var diffOnPlane = Vector3.ProjectOnPlane(diffPosition, motionSource.ReferencePlaneNormal);
                var yOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceRight);
                var zOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceForward);
                _xTarget.y = yOffset.magnitude * Mathf.Sign(Vector3.Dot(yOffset, motionSource.ReferenceRight));
                _xTarget.z = zOffset.magnitude * Mathf.Sign(Vector3.Dot(zOffset, motionSource.ReferenceForward));
            }
            else
            {
                _xTarget = new Vector3(1, 0, 0);
            }

            var correctedRight = Vector3.ProjectOnPlane(motionSource.TargetRight, motionSource.ReferenceUp);
            if (Vector3.Dot(correctedRight, motionSource.ReferenceRight) < 0)
                correctedRight -= 2 * Vector3.Project(correctedRight, motionSource.ReferenceRight);

            _rTarget.x = Vector3.SignedAngle(motionSource.ReferenceRight, correctedRight, motionSource.ReferenceUp) / 180;
            _rTarget.y = Vector3.SignedAngle(motionSource.ReferenceUp, Vector3.ProjectOnPlane(motionSource.TargetUp, motionSource.ReferenceForward), motionSource.ReferenceForward) / 90;
            _rTarget.z = Vector3.SignedAngle(motionSource.ReferenceUp, Vector3.ProjectOnPlane(motionSource.TargetUp, motionSource.ReferenceRight), motionSource.ReferenceRight) / 90;

            for (var i = 0; i < 3; i++)
            {
                _xTargetMax[i] = Math.Max(_xTarget[i], _xTargetMax[i]);
                _xTargetMin[i] = Math.Min(_xTarget[i], _xTargetMin[i]);
                _rTargetMax[i] = Math.Max(_rTarget[i], _rTargetMax[i]);
                _rTargetMin[i] = Math.Min(_rTarget[i], _rTargetMin[i]);
            }

            var l0t = (_xTarget.x - RangeMinL0Slider.val) / (RangeMaxL0Slider.val - RangeMinL0Slider.val);
            var l1t = (_xTarget.y + RangeMaxL1Slider.val) / (2 * RangeMaxL1Slider.val);
            var l2t = (_xTarget.z + RangeMaxL2Slider.val) / (2 * RangeMaxL2Slider.val);
            var r0t = 0.5f + (_rTarget.x / 2) / (RangeMaxR0Slider.val / 180);
            var r1t = 0.5f + (_rTarget.y / 2) / (RangeMaxR1Slider.val / 90);
            var r2t = 0.5f + (_rTarget.z / 2) / (RangeMaxR2Slider.val / 90);
            var v0t = OutputV0CurveEditorSettings.Evaluate(_xTarget, _rTarget);
            var v1t = OutputV1CurveEditorSettings.Evaluate(_xTarget, _rTarget);

            l0t = Mathf.Clamp01(l0t);
            l1t = Mathf.Clamp01(l1t);
            l2t = Mathf.Clamp01(l2t);
            r0t = Mathf.Clamp01(r0t);
            r1t = Mathf.Clamp01(r1t);
            r2t = Mathf.Clamp01(r2t);
            v0t = Mathf.Clamp01(v0t);
            v1t = Mathf.Clamp01(v1t);

            var l0CmdRaw = Mathf.Lerp(OutputMinL0Slider.val, OutputMaxL0Slider.val, l0t);
            var l1CmdRaw = OffsetL1Slider.val + 0.5f + Mathf.Lerp(-OutputMaxL1Slider.val, OutputMaxL1Slider.val, l1t);
            var l2CmdRaw = OffsetL2Slider.val + 0.5f + Mathf.Lerp(-OutputMaxL2Slider.val, OutputMaxL2Slider.val, l2t);
            var r0CmdRaw = OffsetR0Slider.val + 0.5f + Mathf.Lerp(-OutputMaxR0Slider.val, OutputMaxR0Slider.val, r0t);
            var r1CmdRaw = OffsetR1Slider.val + 0.5f + Mathf.Lerp(-OutputMaxR1Slider.val, OutputMaxR1Slider.val, r1t);
            var r2CmdRaw = OffsetR2Slider.val + 0.5f + Mathf.Lerp(-OutputMaxR2Slider.val, OutputMaxR2Slider.val, r2t);
            var v0CmdRaw = v0t;
            var v1CmdRaw = v1t;

            l0CmdRaw = Mathf.Clamp01(l0CmdRaw);
            l1CmdRaw = Mathf.Clamp01(l1CmdRaw);
            l2CmdRaw = Mathf.Clamp01(l2CmdRaw);
            r0CmdRaw = Mathf.Clamp01(r0CmdRaw);
            r1CmdRaw = Mathf.Clamp01(r1CmdRaw);
            r2CmdRaw = Mathf.Clamp01(r2CmdRaw);
            v0CmdRaw = Mathf.Clamp01(v0CmdRaw);
            v1CmdRaw = Mathf.Clamp01(v1CmdRaw);

            if (InvertL0Toggle.val) l0CmdRaw = 1f - l0CmdRaw;
            if (InvertL1Toggle.val) l1CmdRaw = 1f - l1CmdRaw;
            if (InvertL2Toggle.val) l2CmdRaw = 1f - l2CmdRaw;
            if (InvertR0Toggle.val) r0CmdRaw = 1f - r0CmdRaw;
            if (InvertR1Toggle.val) r1CmdRaw = 1f - r1CmdRaw;
            if (InvertR2Toggle.val) r2CmdRaw = 1f - r2CmdRaw;

            if (EnableOverrideL0Toggle.val) l0CmdRaw = OverrideL0Slider.val;
            if (EnableOverrideL1Toggle.val) l1CmdRaw = OverrideL1Slider.val;
            if (EnableOverrideL2Toggle.val) l2CmdRaw = OverrideL2Slider.val;
            if (EnableOverrideR0Toggle.val) r0CmdRaw = OverrideR0Slider.val;
            if (EnableOverrideR1Toggle.val) r1CmdRaw = OverrideR1Slider.val;
            if (EnableOverrideR2Toggle.val) r2CmdRaw = OverrideR2Slider.val;
            if (EnableOverrideV0Toggle.val) v0CmdRaw = OverrideV0Slider.val;
            if (EnableOverrideV1Toggle.val) v1CmdRaw = OverrideV1Slider.val;

            _xCmd.x = Mathf.Lerp(_xCmd.x, l0CmdRaw, 1 - SmoothingSlider.val);
            _xCmd.y = Mathf.Lerp(_xCmd.y, l1CmdRaw, 1 - SmoothingSlider.val);
            _xCmd.z = Mathf.Lerp(_xCmd.z, l2CmdRaw, 1 - SmoothingSlider.val);
            _rCmd.x = Mathf.Lerp(_rCmd.x, r0CmdRaw, 1 - SmoothingSlider.val);
            _rCmd.y = Mathf.Lerp(_rCmd.y, r1CmdRaw, 1 - SmoothingSlider.val);
            _rCmd.z = Mathf.Lerp(_rCmd.z, r2CmdRaw, 1 - SmoothingSlider.val);
            _vCmd[0] = Mathf.Lerp(_vCmd[0], v0CmdRaw, 1 - SmoothingSlider.val);
            _vCmd[1] = Mathf.Lerp(_vCmd[1], v1CmdRaw, 1 - SmoothingSlider.val);

            var s = "          Min      Cur      Max     Cmd\n";
            s += string.Format("L0\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _xTargetMin.x, _xTarget.x, _xTargetMax.x, _xCmd.x);
            s += string.Format("L1\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _xTargetMin.y, _xTarget.y, _xTargetMax.y, _xCmd.y);
            s += string.Format("L2\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _xTargetMin.z, _xTarget.z, _xTargetMax.z, _xCmd.z);
            s += string.Format("R0\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _rTargetMin.x, _rTarget.x, _rTargetMax.x, _rCmd.x);
            s += string.Format("R1\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _rTargetMin.y, _rTarget.y, _rTargetMax.y, _rCmd.y);
            s += string.Format("R2\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", _rTargetMin.z, _rTarget.z, _rTargetMax.z, _rCmd.z);
            s += string.Format("V0\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}\n", 0, 0, 0, _vCmd[0]);
            s += string.Format("V1\t{0,5:0.00},\t{1,5:0.00},\t{2,5:0.00},\t{3,5:0.00}", 0, 0, 0, _vCmd[1]);
            DeviceReport = s;

            DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMinL0Slider.val * motionSource.ReferenceLength, motionSource.TargetUp, Color.white, 0.05f);
            DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMaxL0Slider.val * motionSource.ReferenceLength, motionSource.TargetUp, Color.white, 0.05f);

            return true;
        }

        public abstract void Write(SerialPort serial, Vector3 xCmd, Vector3 rCmd, float[] _vCmd);

        public void Write(SerialPort serial)
            => Write(serial, _xCmd, _rCmd, _vCmd);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing) { }
    }
}
