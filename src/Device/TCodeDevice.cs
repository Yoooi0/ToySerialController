using DebugUtils;
using System.Linq;
using System.Text;
using ToySerialController.MotionSource;
using ToySerialController.Device.OutputTarget;
using UnityEngine;

namespace ToySerialController
{
    public partial class TCodeDevice : IDevice
    {
        protected readonly float[] XTarget, RTarget, ETarget;
        protected readonly float[] XCmd, RCmd, ECmd;
        protected readonly float[] LastXCmd, LastRCmd, LastECmd;
        private readonly StringBuilder _stringBuilder;

        private float? _lastNoCollisionTime;
        private bool _lastNoCollisionSmoothingEnabled;
        private float _lastNoCollisionSmoothingStartTime, _lastNoCollisionSmoothingDuration;

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

            LastXCmd = new float[] { float.NaN, float.NaN, float.NaN };
            LastRCmd = new float[] { float.NaN, float.NaN, float.NaN };
            LastECmd = Enumerable.Range(0, 9).Select(_ => float.NaN).ToArray();

            _lastNoCollisionTime = Time.time;
            _stringBuilder = new StringBuilder();
        }

        private string AppendIfChanged(StringBuilder stringBuilder, string axisName, float cmd, ref float lastCmd)
        {
            if (!float.IsNaN(lastCmd) && Mathf.Abs(lastCmd - cmd) * 999 < 1)
                return string.Empty;

            lastCmd = cmd;
            var command = $"{axisName}{Mathf.RoundToInt(Mathf.Clamp01(cmd) * 999):000}I{Mathf.RoundToInt(Time.deltaTime * 1000)}";
            stringBuilder.Append(command).Append(" ");
            return command;
        }

        public bool Update(IMotionSource motionSource, IOutputTarget outputTarget)
        {
            var length = motionSource.ReferenceLength * ReferenceLengthScaleSlider.val;
            var radius = motionSource.ReferenceRadius * ReferenceRadiusScaleSlider.val;
            var referenceEnding = motionSource.ReferencePosition + motionSource.ReferenceUp * length;
            var diffPosition = motionSource.TargetPosition - motionSource.ReferencePosition;
            var diffEnding = motionSource.TargetPosition - referenceEnding;
            var aboveTarget = (Vector3.Dot(diffPosition, motionSource.TargetUp) < 0 && Vector3.Dot(diffEnding, motionSource.TargetUp) < 0)
                                || Vector3.Dot(diffPosition, motionSource.ReferenceUp) < 0;

            for (var i = 0; i < 5; i++)
                DebugDraw.DrawCircle(Vector3.Lerp(motionSource.ReferencePosition, referenceEnding, i / 4.0f), motionSource.ReferenceUp, motionSource.ReferenceRight, Color.grey, radius);

            var t = Mathf.Clamp(Vector3.Dot(motionSource.TargetPosition - motionSource.ReferencePosition, motionSource.ReferenceUp), 0f, length);
            var closestPoint = motionSource.ReferencePosition + motionSource.ReferenceUp * t;

            if (Vector3.Magnitude(closestPoint - motionSource.TargetPosition) <= radius)
            {
                if (diffPosition.magnitude > 0.0001f)
                {
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

                if (_lastNoCollisionTime != null)
                {
                    _lastNoCollisionSmoothingEnabled = true;
                    _lastNoCollisionSmoothingStartTime = Time.time;
                    _lastNoCollisionSmoothingDuration = Mathf.Clamp(Time.time - _lastNoCollisionTime.Value, 0.5f, 2);
                    _lastNoCollisionTime = null;
                }
            }
            else
            {
                if (_lastNoCollisionTime == null)
                    _lastNoCollisionTime = Time.time;

                return false;
            }

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

            if (_lastNoCollisionSmoothingEnabled)
            {
                var lastCollisionSmoothingT = Mathf.Pow(2, 10 * ((Time.time - _lastNoCollisionSmoothingStartTime) / _lastNoCollisionSmoothingDuration - 1));
                if (lastCollisionSmoothingT >= 1.0f)
                {
                    _lastNoCollisionSmoothingEnabled = false;
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

            DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMinL0Slider.val * motionSource.ReferenceLength, motionSource.TargetUp, motionSource.TargetRight, Color.white, 0.05f);
            DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMaxL0Slider.val * motionSource.ReferenceLength, motionSource.TargetUp, motionSource.TargetRight, Color.white, 0.05f);

            _stringBuilder.Length = 0;
            var l0 = AppendIfChanged(_stringBuilder, "L0", XCmd[0], ref LastXCmd[0]);
            var l1 = AppendIfChanged(_stringBuilder, "L1", XCmd[1], ref LastXCmd[1]);
            var l2 = AppendIfChanged(_stringBuilder, "L2", XCmd[2], ref LastXCmd[2]);
            var r0 = AppendIfChanged(_stringBuilder, "R0", RCmd[0], ref LastRCmd[0]);
            var r1 = AppendIfChanged(_stringBuilder, "R1", RCmd[1], ref LastRCmd[1]);
            var r2 = AppendIfChanged(_stringBuilder, "R2", RCmd[2], ref LastRCmd[2]);
            var v0 = AppendIfChanged(_stringBuilder, "V0", ECmd[0], ref LastECmd[0]);
            var v1 = AppendIfChanged(_stringBuilder, "V1", ECmd[1], ref LastECmd[1]);
            var l3 = AppendIfChanged(_stringBuilder, "L3", ECmd[2], ref LastECmd[2]);

            var data = $"{_stringBuilder}\n";
            if (!string.IsNullOrEmpty(data.Trim()))
                outputTarget?.Write(data);

            _stringBuilder.Length = 0;
            _stringBuilder.Append("        Target   Cmd      Output\n");
            _stringBuilder.Append("L0\t").AppendFormat("{0,5:0.00}", XTarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[0]).Append(",\t").AppendLine(l0);
            _stringBuilder.Append("L1\t").AppendFormat("{0,5:0.00}", XTarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[1]).Append(",\t").AppendLine(l1);
            _stringBuilder.Append("L2\t").AppendFormat("{0,5:0.00}", XTarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[2]).Append(",\t").AppendLine(l2);
            _stringBuilder.Append("R0\t").AppendFormat("{0,5:0.00}", RTarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[0]).Append(",\t").AppendLine(r0);
            _stringBuilder.Append("R1\t").AppendFormat("{0,5:0.00}", RTarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[1]).Append(",\t").AppendLine(r1);
            _stringBuilder.Append("R2\t").AppendFormat("{0,5:0.00}", RTarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[2]).Append(",\t").AppendLine(r2);
            _stringBuilder.Append("V0\t").AppendFormat("{0,5:0.00}", ETarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[0]).Append(",\t").AppendLine(v0);
            _stringBuilder.Append("V1\t").AppendFormat("{0,5:0.00}", ETarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[1]).Append(",\t").AppendLine(v1);
            _stringBuilder.Append("L3\t").AppendFormat("{0,5:0.00}", ETarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[2]).Append(",\t").Append(l3);
            DeviceReport = _stringBuilder.ToString();
            return true;
        }

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing) { }
    }
}
