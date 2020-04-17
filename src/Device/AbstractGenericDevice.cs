using CurveEditor;
using CurveEditor.UI;
using DebugUtils;
using Leap;
using System;
using System.IO.Ports;
using ToySerialController.MotionSource;
using ToySerialController.UI;
using ToySerialController.Utils;
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
            var diff = motionSource.TargetPosition - motionSource.ReferencePosition;

            Vector3 projectionNormal;
            if (ProjectXChooser.val == "Reference Up") projectionNormal = motionSource.ReferenceUp;
            else if (ProjectXChooser.val == "Target Up") projectionNormal = motionSource.TargetNormal;
            else projectionNormal = diff.normalized;

            var diffOnNormal = Vector3.Project(diff, projectionNormal);
            var diffOnPlane = Vector3.ProjectOnPlane(diff, motionSource.ReferencePlaneNormal);
            var yOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceRight);
            var zOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceForward);

            DebugDraw.DrawRay(motionSource.ReferencePosition, diffOnNormal, 1f, new Color(0.3f, 0.3f, 0.3f));

            _xTarget.x = 1 - Mathf.Clamp(diffOnNormal.magnitude / motionSource.ReferenceLength, 0, 1);
            _xTarget.y = yOffset.magnitude * Mathf.Sign(Vector3.Dot(yOffset, motionSource.ReferenceRight));
            _xTarget.z = zOffset.magnitude * Mathf.Sign(Vector3.Dot(zOffset, motionSource.ReferenceForward));

            // TODO: true target twist 
            _rTarget.y = Vector3.Dot(motionSource.ReferenceRight, motionSource.TargetNormal);
            _rTarget.z = Vector3.Dot(motionSource.ReferenceForward, motionSource.TargetNormal);

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
            var rxt = EvaluateCurve(OutputRXCurveXAxisChooser.val, OutputRXCurveEditor, OutputRXCurve);
            var ryt = (_rTarget.y + RangeMaxRYSlider.val) / (2 * RangeMaxRYSlider.val);
            var rzt = (_rTarget.z + RangeMaxRZSlider.val) / (2 * RangeMaxRZSlider.val);
            var v0t = EvaluateCurve(OutputV0CurveXAxisChooser.val, OutputV0CurveEditor, OutputV0Curve);
            var v1t = EvaluateCurve(OutputV1CurveXAxisChooser.val, OutputV1CurveEditor, OutputV1Curve);

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

            _xCmd.x = Mathf.Lerp(xCmdRaw, _xCmd.x, SmoothingSlider.val);
            _xCmd.y = Mathf.Lerp(yCmdRaw, _xCmd.y, SmoothingSlider.val);
            _xCmd.z = Mathf.Lerp(zCmdRaw, _xCmd.z, SmoothingSlider.val);
            _rCmd.x = Mathf.Lerp(rxCmdRaw, _rCmd.x, SmoothingSlider.val);
            _rCmd.y = Mathf.Lerp(ryCmdRaw, _rCmd.z, SmoothingSlider.val);
            _rCmd.z = Mathf.Lerp(rzCmdRaw, _rCmd.z, SmoothingSlider.val);
            _vCmd[0] = Mathf.Lerp(v0CmdRaw, _vCmd[0], SmoothingSlider.val);
            _vCmd[1] = Mathf.Lerp(v1CmdRaw, _vCmd[1], SmoothingSlider.val);

            if (InvertXToggle.val) _xCmd.x = 1f - _xCmd.x;
            if (InvertYToggle.val) _xCmd.y = 1f - _xCmd.y;
            if (InvertZToggle.val) _xCmd.z = 1f - _xCmd.z;
            if (InvertRXToggle.val) _rCmd.x = 1f - _rCmd.x;
            if (InvertRYToggle.val) _rCmd.y = 1f - _rCmd.y;
            if (InvertRZToggle.val) _rCmd.z = 1f - _rCmd.z;

            if (EnableOverrideXToggle.val) _xCmd.x = OverrideXSlider.val;
            if (EnableOverrideYToggle.val) _xCmd.y = OverrideYSlider.val;
            if (EnableOverrideZToggle.val) _xCmd.z = OverrideZSlider.val;
            if (EnableOverrideRXToggle.val) _rCmd.x = OverrideRXSlider.val;
            if (EnableOverrideRYToggle.val) _rCmd.y = OverrideRYSlider.val;
            if (EnableOverrideRZToggle.val) _rCmd.z = OverrideRZSlider.val;
            if (EnableOverrideV0Toggle.val) _vCmd[0] = OverrideV0Slider.val;
            if (EnableOverrideV1Toggle.val) _vCmd[1] = OverrideV1Slider.val;

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

            return true;
        }

        private float EvaluateCurve(string xAxisType, UICurveEditor editor, IStorableAnimationCurve storable)
        {
            var t = _xTarget.x;
            if (xAxisType == "X") t = _xTarget.x;
            else if (xAxisType == "RY") t = Mathf.Abs(_rTarget.y);
            else if (xAxisType == "RZ") t = Mathf.Abs(_rTarget.z);
            else if (xAxisType == "RY+RZ") t = Mathf.Sqrt(_rTarget.y * _rTarget.y + _rTarget.z * _rTarget.z);
            else if (xAxisType == "X+RY+RZ") t = Mathf.Sqrt(_xTarget.x * _xTarget.x + _rTarget.y * _rTarget.y + _rTarget.z * _rTarget.z);

            t = Mathf.Clamp01(t);
            editor.SetScrubber(storable, t);
            return storable.val.Evaluate(t);
        }

        public abstract void Write(SerialPort serial, Vector3 xCmd, Vector3 rCmd, float[] _vCmd);

        public void Write(SerialPort serial)
            => Write(serial, _xCmd, _rCmd, _vCmd);

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing) { }
    }
}
