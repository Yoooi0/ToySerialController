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
        private float _lastCollisionSmoothingT;
        private float _lastNoCollisionSmoothingStartTime, _lastNoCollisionSmoothingDuration;
        private bool _isLoading;

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
            var command = $"{axisName}{Mathf.RoundToInt(Mathf.Clamp01(cmd) * 9999):0000}I{Mathf.RoundToInt(Time.deltaTime * 1000)}";
            stringBuilder.Append(command).Append(" ");
            return command;
        }

        public bool Update(IMotionSource motionSource, IOutputTarget outputTarget)
        {
            if (_isLoading)
            {
                for (var i = 0; i < 9; i++)
                    ETarget[i] = Mathf.Lerp(ETarget[i], 0f, 0.05f);

                for (var i = 0; i < 3; i++)
                {
                    XTarget[i] = Mathf.Lerp(XTarget[i], 0.5f, 0.05f);
                    RTarget[i] = Mathf.Lerp(RTarget[i], 0f, 0.05f);
                }
            }
            else if(motionSource != null)
            {
                UpdateMotion(motionSource);

                DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMinL0Slider.val * motionSource.ReferenceLength, motionSource.TargetUp, motionSource.TargetRight, Color.white, 0.05f);
                DebugDraw.DrawCircle(motionSource.TargetPosition + motionSource.TargetUp * RangeMaxL0Slider.val * motionSource.ReferenceLength, motionSource.TargetUp, motionSource.TargetRight, Color.white, 0.05f);
            }

            UpdateValues(outputTarget);

            return true;
        }

        public void UpdateValues(IOutputTarget outputTarget)
        {
            if (_lastNoCollisionSmoothingEnabled)
            {
                _lastCollisionSmoothingT = Mathf.Pow(2, 10 * ((Time.time - _lastNoCollisionSmoothingStartTime) / _lastNoCollisionSmoothingDuration - 1));
                if (_lastCollisionSmoothingT > 1.0f)
                {
                    _lastNoCollisionSmoothingEnabled = false;
                    _lastCollisionSmoothingT = 0;
                }
            }

            UpdateL0(); UpdateL1(); UpdateL2();
            UpdateR0(); UpdateR1(); UpdateR2();
            UpdateV0();
            UpdateA0(); UpdateA1(); UpdateA2();

            _stringBuilder.Length = 0;
            var l0 = AppendIfChanged(_stringBuilder, "L0", XCmd[0], ref LastXCmd[0]);
            var l1 = AppendIfChanged(_stringBuilder, "L1", XCmd[1], ref LastXCmd[1]);
            var l2 = AppendIfChanged(_stringBuilder, "L2", XCmd[2], ref LastXCmd[2]);
            var r0 = AppendIfChanged(_stringBuilder, "R0", RCmd[0], ref LastRCmd[0]);
            var r1 = AppendIfChanged(_stringBuilder, "R1", RCmd[1], ref LastRCmd[1]);
            var r2 = AppendIfChanged(_stringBuilder, "R2", RCmd[2], ref LastRCmd[2]);
            var v0 = AppendIfChanged(_stringBuilder, "V0", ECmd[0], ref LastECmd[0]);
            var a0 = AppendIfChanged(_stringBuilder, "A0", ECmd[1], ref LastECmd[1]);
            var a1 = AppendIfChanged(_stringBuilder, "A1", ECmd[2], ref LastECmd[2]);
            var a2 = AppendIfChanged(_stringBuilder, "A2", ECmd[3], ref LastECmd[3]);

            var data = $"{_stringBuilder}\n";
            if (!string.IsNullOrEmpty(data.Trim()))
                outputTarget?.Write(data);

            _stringBuilder.Length = 0;
            _stringBuilder.Append("    Target    Cmd    Output\n");
            _stringBuilder.Append("L0\t").AppendFormat("{0,5:0.00}", XTarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[0]).Append(",\t").AppendLine(l0);
            _stringBuilder.Append("L1\t").AppendFormat("{0,5:0.00}", XTarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[1]).Append(",\t").AppendLine(l1);
            _stringBuilder.Append("L2\t").AppendFormat("{0,5:0.00}", XTarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", XCmd[2]).Append(",\t").AppendLine(l2);
            _stringBuilder.Append("R0\t").AppendFormat("{0,5:0.00}", RTarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[0]).Append(",\t").AppendLine(r0);
            _stringBuilder.Append("R1\t").AppendFormat("{0,5:0.00}", RTarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[1]).Append(",\t").AppendLine(r1);
            _stringBuilder.Append("R2\t").AppendFormat("{0,5:0.00}", RTarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", RCmd[2]).Append(",\t").AppendLine(r2);
            _stringBuilder.Append("V0\t").AppendFormat("{0,5:0.00}", ETarget[0]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[0]).Append(",\t").AppendLine(v0);
            _stringBuilder.Append("A0\t").AppendFormat("{0,5:0.00}", ETarget[1]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[1]).Append(",\t").AppendLine(a0);
            _stringBuilder.Append("A1\t").AppendFormat("{0,5:0.00}", ETarget[2]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[2]).Append(",\t").AppendLine(a1);
            _stringBuilder.Append("A2\t").AppendFormat("{0,5:0.00}", ETarget[3]).Append(",\t").AppendFormat("{0,5:0.00}", ECmd[3]).Append(",\t").Append(a2);
            DeviceReport = _stringBuilder.ToString();
        }

        public void UpdateL0()
        {
            var t = Mathf.Clamp01((XTarget[0] - RangeMinL0Slider.val) / (RangeMaxL0Slider.val - RangeMinL0Slider.val));
            var output = Mathf.Clamp01(Mathf.Lerp(OutputMinL0Slider.val, OutputMaxL0Slider.val, t));

            if (InvertL0Toggle.val) output = 1f - output;
            if (EnableOverrideL0Toggle.val) output = OverrideL0Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(XCmd[0], output, _lastCollisionSmoothingT);

            XCmd[0] = Mathf.Lerp(XCmd[0], output, 1 - SmoothingSlider.val);
        }

        public void UpdateL1()
        {
            var t = Mathf.Clamp01((XTarget[1] + RangeMaxL1Slider.val) / (2 * RangeMaxL1Slider.val));
            var output = Mathf.Clamp01(OffsetL1Slider.val + 0.5f + Mathf.Lerp(-OutputMaxL1Slider.val, OutputMaxL1Slider.val, t));

            if (InvertL1Toggle.val) output = 1f - output;
            if (EnableOverrideL1Toggle.val) output = OverrideL1Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(XCmd[1], output, _lastCollisionSmoothingT);

            XCmd[1] = Mathf.Lerp(XCmd[1], output, 1 - SmoothingSlider.val);
        }

        public void UpdateL2()
        {
            var t = Mathf.Clamp01((XTarget[2] + RangeMaxL2Slider.val) / (2 * RangeMaxL2Slider.val));
            var output = Mathf.Clamp01(OffsetL2Slider.val + 0.5f + Mathf.Lerp(-OutputMaxL2Slider.val, OutputMaxL2Slider.val, t));

            if (InvertL2Toggle.val) output = 1f - output;
            if (EnableOverrideL2Toggle.val) output = OverrideL2Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(XCmd[2], output, _lastCollisionSmoothingT);

            XCmd[2] = Mathf.Lerp(XCmd[2], output, 1 - SmoothingSlider.val);
        }

        public void UpdateR0()
        {
            var t = Mathf.Clamp01(0.5f + (RTarget[0] / 2) / (RangeMaxR0Slider.val / 180));
            var output = Mathf.Clamp01(OffsetR0Slider.val + 0.5f + Mathf.Lerp(-OutputMaxR0Slider.val, OutputMaxR0Slider.val, t));

            if (InvertR0Toggle.val) output = 1f - output;
            if (EnableOverrideR0Toggle.val) output = OverrideR0Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(RCmd[0], output, _lastCollisionSmoothingT);

            RCmd[0] = Mathf.Lerp(RCmd[0], output, 1 - SmoothingSlider.val);
        }

        public void UpdateR1()
        {
            var t = Mathf.Clamp01(0.5f + (RTarget[1] / 2) / (RangeMaxR1Slider.val / 90));
            var output = Mathf.Clamp01(OffsetR1Slider.val + 0.5f + Mathf.Lerp(-OutputMaxR1Slider.val, OutputMaxR1Slider.val, t));

            if (InvertR1Toggle.val) output = 1f - output;
            if (EnableOverrideR1Toggle.val) output = OverrideR1Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(RCmd[1], output, _lastCollisionSmoothingT);

            RCmd[1] = Mathf.Lerp(RCmd[1], output, 1 - SmoothingSlider.val);
        }

        public void UpdateR2()
        {
            var t = Mathf.Clamp01(0.5f + (RTarget[2] / 2) / (RangeMaxR2Slider.val / 90));
            var output = Mathf.Clamp01(OffsetR2Slider.val + 0.5f + Mathf.Lerp(-OutputMaxR2Slider.val, OutputMaxR2Slider.val, t));

            if (InvertR2Toggle.val) output = 1f - output;
            if (EnableOverrideR2Toggle.val) output = OverrideR2Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(RCmd[2], output, _lastCollisionSmoothingT);

            RCmd[2] = Mathf.Lerp(RCmd[2], output, 1 - SmoothingSlider.val);
        }

        public void UpdateV0()
        {
            var output = Mathf.Clamp01(ETarget[0]);

            if (EnableOverrideV0Toggle.val) output = OverrideV0Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(ECmd[0], output, _lastCollisionSmoothingT);

            ECmd[0] = Mathf.Lerp(ECmd[0], output, 1 - SmoothingSlider.val);
        }

        public void UpdateA0()
        {
            var output = Mathf.Clamp01(ETarget[1]);

            if (EnableOverrideA0Toggle.val) output = OverrideA0Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(ECmd[1], output, _lastCollisionSmoothingT);

            ECmd[1] = Mathf.Lerp(ECmd[1], output, 1 - SmoothingSlider.val);
        }

        public void UpdateA1()
        {
            var output = Mathf.Clamp01(ETarget[2]);

            if (EnableOverrideA1Toggle.val) output = OverrideA1Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(ECmd[2], output, _lastCollisionSmoothingT);

            ECmd[2] = Mathf.Lerp(ECmd[2], output, 1 - SmoothingSlider.val);
        }

        public void UpdateA2()
        {
            var output = Mathf.Clamp01(ETarget[3]);

            if (EnableOverrideA2Toggle.val) output = OverrideA2Slider.val;
            if (_lastNoCollisionSmoothingEnabled)
                output = Mathf.Lerp(ECmd[3], output, _lastCollisionSmoothingT);

            ECmd[3] = Mathf.Lerp(ECmd[3], output, 1 - SmoothingSlider.val);
        }

        public bool UpdateMotion(IMotionSource motionSource)
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
                    var rightOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceRight);
                    var forwardOffset = Vector3.Project(diffOnPlane, motionSource.ReferenceForward);
                    XTarget[1] = forwardOffset.magnitude * Mathf.Sign(Vector3.Dot(forwardOffset, motionSource.ReferenceForward));
                    XTarget[2] = rightOffset.magnitude * Mathf.Sign(Vector3.Dot(rightOffset, motionSource.ReferenceRight));
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
                ETarget[1] = OutputA0CurveEditorSettings.Evaluate(XTarget, RTarget);
                ETarget[2] = OutputA1CurveEditorSettings.Evaluate(XTarget, RTarget);
                ETarget[3] = OutputA2CurveEditorSettings.Evaluate(XTarget, RTarget);

                if (_lastNoCollisionTime != null)
                {
                    _lastNoCollisionSmoothingEnabled = true;
                    _lastNoCollisionSmoothingStartTime = Time.time;
                    _lastNoCollisionSmoothingDuration = Mathf.Clamp(Time.time - _lastNoCollisionTime.Value, 0.5f, 2);
                    _lastNoCollisionTime = null;
                }

                return true;
            }
            else
            {
                if (_lastNoCollisionTime == null)
                    _lastNoCollisionTime = Time.time;

                return false;
            }
        }

        public void Dispose() => Dispose(true);
        protected virtual void Dispose(bool disposing) { }

        public virtual void OnSceneChanging() => _isLoading = true;
        public virtual void OnSceneChanged()
        {
            _lastNoCollisionSmoothingEnabled = true;
            _lastNoCollisionSmoothingStartTime = Time.time;
            _lastNoCollisionSmoothingDuration = 2;
            _lastNoCollisionTime = null;

            _isLoading = false;
        }
    }
}
