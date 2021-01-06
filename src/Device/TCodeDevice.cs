using DebugUtils;
using System;
using System.IO.Ports;
using System.Text;
using ToySerialController.MotionSource;
using UnityEngine;

namespace ToySerialController
{
    public partial class TCodeDevice : IDevice
    {
        protected readonly float[] XTarget, RTarget, ETarget;
        protected readonly float[] XCmd, RCmd, ECmd;

        private float? _lastCollisionTime;
        private bool _lastCollisionSmoothingEnabled;
        private float _lastCollisionSmoothingStartTime, _lastCollisionSmoothingDuration;

        protected string DeviceReport { get; set; }
        public string GetDeviceReport() => DeviceReport;

        public TCodeDevice()
        {
            XTarget = new float[3];
            RTarget = new float[3];
            ETarget = new float[9];

            XCmd = new float[] { 0.5f, 0.5f, 0.5f };
            RCmd = new float[] { 0.5f, 0.5f, 0.5f };
            ECmd = new float[9];
        }

        public void Write(SerialPort serial)
        {
            var ms = Mathf.RoundToInt(TCodeIntervalSlider.val);
            var interval = ms > 0 ? $"I{ms}" : string.Empty;

            var l0 = $"L0{TCodeNumber(XCmd[0])}{interval}";
            var l1 = $"L1{TCodeNumber(XCmd[1])}{interval}";
            var l2 = $"L2{TCodeNumber(XCmd[2])}{interval}";
            var r0 = $"R0{TCodeNumber(RCmd[0])}{interval}";
            var r1 = $"R1{TCodeNumber(RCmd[1])}{interval}";
            var r2 = $"R2{TCodeNumber(RCmd[2])}{interval}";
            var v0 = $"V0{TCodeNumber(ECmd[0])}{interval}";
            var v1 = $"V1{TCodeNumber(ECmd[1])}{interval}";
            var l3 = $"L3{TCodeNumber(ECmd[2])}{interval}";

            var data = $"{l0} {l1} {l2} {r0} {r1} {r2} {v0} {v1} {l3}\n";
            if (serial?.IsOpen == true)
                serial.Write(data);

            var sb = new StringBuilder();
            sb.Append("        Target   Cmd      Serial\n");
            sb.Append("L0\t").AppendFormat("{0,5:0.00}", XTarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[0]).Append(",\t").Append(l0).AppendLine();
            sb.Append("L1\t").AppendFormat("{0,5:0.00}", XTarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[1]).Append(",\t").Append(l1).AppendLine();
            sb.Append("L2\t").AppendFormat("{0,5:0.00}", XTarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[2]).Append(",\t").Append(l2).AppendLine();
            sb.Append("R0\t").AppendFormat("{0,5:0.00}", RTarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[0]).Append(",\t").Append(r0).AppendLine();
            sb.Append("R1\t").AppendFormat("{0,5:0.00}", RTarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[1]).Append(",\t").Append(r1).AppendLine();
            sb.Append("R2\t").AppendFormat("{0,5:0.00}", RTarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[2]).Append(",\t").Append(r2).AppendLine();
            sb.Append("V0\t").AppendFormat("{0,5:0.00}", ETarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[0]).Append(",\t").Append(v0).AppendLine();
            sb.Append("V1\t").AppendFormat("{0,5:0.00}", ETarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[1]).Append(",\t").Append(v1).AppendLine();
            sb.Append("L3\t").AppendFormat("{0,5:0.00}", ETarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[2]).Append(",\t").Append(l3);
            DeviceReport = sb.ToString();
        }

        private string TCodeNumber(float input)
        {
            input = Mathf.Clamp(input, 0, 1) * 1000;

            if (input >= 999f) return "999";
            else if (input >= 1f) return input.ToString("000");
            return "000";
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

                if (Vector3.Magnitude(closestPoint - motionSource.TargetPosition) > radius)
                {
                    if (_lastCollisionTime == null)
                        _lastCollisionTime = Time.time;
                    return false;
                }

                XTarget[0] = 1 - Mathf.Clamp01((closestPoint - motionSource.ReferencePosition).magnitude / length);
                if (aboveTarget)
                    XTarget[0] = XTarget[0] > 0 ? 1 : 0;

                var diffOnPlane = Vector3.ProjectOnPlane(diffPosition, motionSource.ReferencePlaneNormal);
                var yOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceRight);
                var zOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceForward);
                XTarget[1] = yOffset.magnitude * Mathf.Sign(Vector3.Dot(yOffset, motionSource.ReferenceRight));
                XTarget[2] = zOffset.magnitude * Mathf.Sign(Vector3.Dot(zOffset, motionSource.ReferenceForward));
            }
            else
            {
                SuperController.singleton.Message("diff");
                XTarget[0] = 1;
                XTarget[1] = 0;
                XTarget[2] = 0;
            }

            var correctedRight = Vector3.ProjectOnPlane(motionSource.TargetRight, motionSource.ReferenceUp);
            if (Vector3.Dot(correctedRight, motionSource.ReferenceRight) < 0)
                correctedRight -= 2 * Vector3.Project(correctedRight, motionSource.ReferenceRight);

            RTarget[0] = Vector3.SignedAngle(motionSource.ReferenceRight, correctedRight, motionSource.ReferenceUp) / 180;
                RTarget[1] = -Vector3.SignedAngle(motionSource.ReferenceUp, Vector3.ProjectOnPlane(motionSource.TargetUp, motionSource.ReferenceForward), motionSource.ReferenceForward) / 90;
            RTarget[2] = Vector3.SignedAngle(motionSource.ReferenceUp, Vector3.ProjectOnPlane(motionSource.TargetUp, motionSource.ReferenceRight), motionSource.ReferenceRight) / 90;

            ETarget[0] = OutputV0CurveEditorSettings.Evaluate(XTarget, RTarget);
            ETarget[1] = OutputV1CurveEditorSettings.Evaluate(XTarget, RTarget);
            ETarget[2] = OutputL3CurveEditorSettings.Evaluate(XTarget, RTarget);

            var l0t = (XTarget[0] - RangeMinL0Slider.val) / (RangeMaxL0Slider.val - RangeMinL0Slider.val);
            var l1t = (XTarget[1] + RangeMaxL1Slider.val) / (2 * RangeMaxL1Slider.val);
            var l2t = (XTarget[2] + RangeMaxL2Slider.val) / (2 * RangeMaxL2Slider.val);
            var r0t = 0.5f + (RTarget[0] / 2) / (RangeMaxR0Slider.val / 180);
            var r1t = 0.5f + (RTarget[1] / 2) / (RangeMaxR1Slider.val / 90);
            var r2t = 0.5f + (RTarget[2] / 2) / (RangeMaxR2Slider.val / 90);
            var v0t = ETarget[0];
            var v1t = ETarget[1];
            var l3t = ETarget[2];

            l0t = Mathf.Clamp01(l0t);
            l1t = Mathf.Clamp01(l1t);
            l2t = Mathf.Clamp01(l2t);
            r0t = Mathf.Clamp01(r0t);
            r1t = Mathf.Clamp01(r1t);
            r2t = Mathf.Clamp01(r2t);
            v0t = Mathf.Clamp01(v0t);
            v1t = Mathf.Clamp01(v1t);
            l3t = Mathf.Clamp01(l3t);

            var l0CmdRaw = Mathf.Lerp(OutputMinL0Slider.val, OutputMaxL0Slider.val, l0t);
            var l1CmdRaw = OffsetL1Slider.val + 0.5f + Mathf.Lerp(-OutputMaxL1Slider.val, OutputMaxL1Slider.val, l1t);
            var l2CmdRaw = OffsetL2Slider.val + 0.5f + Mathf.Lerp(-OutputMaxL2Slider.val, OutputMaxL2Slider.val, l2t);
            var r0CmdRaw = OffsetR0Slider.val + 0.5f + Mathf.Lerp(-OutputMaxR0Slider.val, OutputMaxR0Slider.val, r0t);
            var r1CmdRaw = OffsetR1Slider.val + 0.5f + Mathf.Lerp(-OutputMaxR1Slider.val, OutputMaxR1Slider.val, r1t);
            var r2CmdRaw = OffsetR2Slider.val + 0.5f + Mathf.Lerp(-OutputMaxR2Slider.val, OutputMaxR2Slider.val, r2t);
            var v0CmdRaw = v0t;
            var v1CmdRaw = v1t;
            var l3CmdRaw = l3t;

            l0CmdRaw = Mathf.Clamp01(l0CmdRaw);
            l1CmdRaw = Mathf.Clamp01(l1CmdRaw);
            l2CmdRaw = Mathf.Clamp01(l2CmdRaw);
            r0CmdRaw = Mathf.Clamp01(r0CmdRaw);
            r1CmdRaw = Mathf.Clamp01(r1CmdRaw);
            r2CmdRaw = Mathf.Clamp01(r2CmdRaw);
            v0CmdRaw = Mathf.Clamp01(v0CmdRaw);
            v1CmdRaw = Mathf.Clamp01(v1CmdRaw);
            l3CmdRaw = Mathf.Clamp01(l3CmdRaw);

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
            if (EnableOverrideL3Toggle.val) l3CmdRaw = OverrideL3Slider.val;

            if (_lastCollisionTime != null)
            {
                var noCollisionDuration = Time.time - _lastCollisionTime.Value;
                _lastCollisionSmoothingDuration = Mathf.Clamp(noCollisionDuration, 0.5f, 2);
                _lastCollisionSmoothingStartTime = Time.time;
                _lastCollisionSmoothingEnabled = true;
                _lastCollisionTime = null;
            }

            if (_lastCollisionSmoothingEnabled)
            {
                var lastCollisionSmoothingT = Mathf.Pow(2, 10 * ((Time.time - _lastCollisionSmoothingStartTime) / _lastCollisionSmoothingDuration - 1));
                if (lastCollisionSmoothingT >= 1.0f)
                {
                    _lastCollisionSmoothingEnabled = false;
                }
                else
                {
                    l0CmdRaw = Mathf.Lerp(XCmd[0], l0CmdRaw, lastCollisionSmoothingT);
                    l1CmdRaw = Mathf.Lerp(XCmd[1], l1CmdRaw, lastCollisionSmoothingT);
                    l2CmdRaw = Mathf.Lerp(XCmd[2], l2CmdRaw, lastCollisionSmoothingT);
                    r0CmdRaw = Mathf.Lerp(RCmd[0], r0CmdRaw, lastCollisionSmoothingT);
                    r1CmdRaw = Mathf.Lerp(RCmd[1], r1CmdRaw, lastCollisionSmoothingT);
                    r2CmdRaw = Mathf.Lerp(RCmd[2], r2CmdRaw, lastCollisionSmoothingT);
                    v0CmdRaw = Mathf.Lerp(ECmd[0], v0CmdRaw, lastCollisionSmoothingT);
                    v1CmdRaw = Mathf.Lerp(ECmd[1], v1CmdRaw, lastCollisionSmoothingT);
                    l3CmdRaw = Mathf.Lerp(ECmd[2], l3CmdRaw, lastCollisionSmoothingT);
                }
            }

            XCmd[0] = Mathf.Lerp(XCmd[0], l0CmdRaw, 1 - SmoothingSlider.val);
            XCmd[1] = Mathf.Lerp(XCmd[1], l1CmdRaw, 1 - SmoothingSlider.val);
            XCmd[2] = Mathf.Lerp(XCmd[2], l2CmdRaw, 1 - SmoothingSlider.val);
            RCmd[0] = Mathf.Lerp(RCmd[0], r0CmdRaw, 1 - SmoothingSlider.val);
            RCmd[1] = Mathf.Lerp(RCmd[1], r1CmdRaw, 1 - SmoothingSlider.val);
            RCmd[2] = Mathf.Lerp(RCmd[2], r2CmdRaw, 1 - SmoothingSlider.val);
            ECmd[0] = Mathf.Lerp(ECmd[0], v0CmdRaw, 1 - SmoothingSlider.val);
            ECmd[1] = Mathf.Lerp(ECmd[1], v1CmdRaw, 1 - SmoothingSlider.val);
            ECmd[2] = Mathf.Lerp(ECmd[2], l3CmdRaw, 1 - SmoothingSlider.val);

            DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMinL0Slider.val * motionSource.ReferenceLength, motionSource.TargetUp, Color.white, 0.05f);
            DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMaxL0Slider.val * motionSource.ReferenceLength, motionSource.TargetUp, Color.white, 0.05f);

            return true;
        }

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing) { }
    }
}
