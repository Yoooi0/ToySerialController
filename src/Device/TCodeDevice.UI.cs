using CurveEditor;
using CurveEditor.UI;
using SimpleJSON;
using System.Collections.Generic;
using ToySerialController.Config;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController
{
    public partial class TCodeDevice : IDevice
    {
        private UIDynamicButton MainTitle;
        private JSONStorableFloat SmoothingSlider;
        private JSONStorableFloat ReferenceLengthScaleSlider;
        private JSONStorableFloat ReferenceRadiusScaleSlider;
        private JSONStorableStringChooser RTargetCalculationMode;

        private UIDynamicButton L0AxisTitle;
        private JSONStorableBool InvertL0Toggle;
        private JSONStorableFloat OutputMaxL0Slider;
        private JSONStorableFloat OutputMinL0Slider;
        private JSONStorableBool EnableOverrideL0Toggle;
        private JSONStorableFloat OverrideL0Slider;
        private JSONStorableFloat RangeMaxL0Slider;
        private JSONStorableFloat RangeMinL0Slider;

        private UIDynamicButton L1AxisTitle;
        private JSONStorableBool InvertL1Toggle;
        private JSONStorableBool EnableOverrideL1Toggle;
        private JSONStorableFloat OutputMaxL1Slider;
        private JSONStorableFloat OffsetL1Slider;
        private JSONStorableFloat OverrideL1Slider;
        private JSONStorableFloat RangeMaxL1Slider;

        private UIDynamicButton L2AxisTitle;
        private JSONStorableBool InvertL2Toggle;
        private JSONStorableBool EnableOverrideL2Toggle;
        private JSONStorableFloat OutputMaxL2Slider;
        private JSONStorableFloat OffsetL2Slider;
        private JSONStorableFloat OverrideL2Slider;
        private JSONStorableFloat RangeMaxL2Slider;

        private UIDynamicButton R0AxisTitle;
        private JSONStorableBool InvertR0Toggle;
        private JSONStorableFloat OutputMaxR0Slider;
        private JSONStorableBool EnableOverrideR0Toggle;
        private JSONStorableFloat OffsetR0Slider;
        private JSONStorableFloat OverrideR0Slider;
        private JSONStorableFloat RangeMaxR0Slider;

        private UIDynamicButton R1AxisTitle;
        private JSONStorableBool InvertR1Toggle;
        private JSONStorableFloat OutputMaxR1Slider;
        private JSONStorableBool EnableOverrideR1Toggle;
        private JSONStorableFloat OffsetR1Slider;
        private JSONStorableFloat OverrideR1Slider;
        private JSONStorableFloat RangeMaxR1Slider;

        private UIDynamicButton R2AxisTitle;
        private JSONStorableBool InvertR2Toggle;
        private JSONStorableFloat OutputMaxR2Slider;
        private JSONStorableFloat OffsetR2Slider;
        private JSONStorableFloat OverrideR2Slider;
        private JSONStorableFloat RangeMaxR2Slider;
        private JSONStorableBool EnableOverrideR2Toggle;

        private UIDynamicButton V0AxisTitle;
        private UICurveEditor OutputV0CurveEditor;
        private JSONStorableAnimationCurve OutputV0Curve;
        private JSONStorableFloat OverrideV0Slider;
        private JSONStorableBool EnableOverrideV0Toggle;
        private DeviceCurveEditorSettings OutputV0CurveEditorSettings;

        private UIDynamicButton A0AxisTitle;
        private UICurveEditor OutputA0CurveEditor;
        private JSONStorableAnimationCurve OutputA0Curve;
        private JSONStorableFloat OverrideA0Slider;
        private JSONStorableBool EnableOverrideA0Toggle;
        private DeviceCurveEditorSettings OutputA0CurveEditorSettings;

        private UIDynamicButton A1AxisTitle;
        private UICurveEditor OutputA1CurveEditor;
        private JSONStorableAnimationCurve OutputA1Curve;
        private JSONStorableFloat OverrideA1Slider;
        private JSONStorableBool EnableOverrideA1Toggle;
        private DeviceCurveEditorSettings OutputA1CurveEditorSettings;

        private UIDynamicButton A2AxisTitle;
        private UICurveEditor OutputA2CurveEditor;
        private JSONStorableAnimationCurve OutputA2Curve;
        private JSONStorableFloat OverrideA2Slider;
        private JSONStorableBool EnableOverrideA2Toggle;
        private DeviceCurveEditorSettings OutputA2CurveEditorSettings;

        private UIGroup _group;

        public virtual void CreateUI(IUIBuilder builder)
        {
            _group = new UIGroup(builder);

            var visible = false;
            var group = new UIGroup(_group);

            MainTitle = _group.CreateButton("Main", () => group.SetVisible(visible = !visible), new Color(0.3f, 0.3f, 0.3f), Color.white, true);

            SmoothingSlider = group.CreateSlider("Plugin:Smoothing", "Smoothing (%)", 0.1f, 0.0f, 0.99f, true, true, true, "P0");
            ReferenceLengthScaleSlider = group.CreateSlider("Device:ReferenceLengthScale", "Reference Length (%)", 1.0f, 0, 3, true, true, true, "P0");
            ReferenceRadiusScaleSlider = group.CreateSlider("Device:ReferenceRadiusScale", "Reference Radius (%)", 3.0f, 0, 5, true, true, true, "P0");
            RTargetCalculationMode = group.CreatePopup("Device:RTargetCalculationMode", "Rotation Calculation Mode", new List<string>() { "Target-Reference", "Target-Plane" }, "Target-Reference", null, true);

            group.SetVisible(false);

            CreateL0AxisUI(_group);
            CreateL1AxisUI(_group);
            CreateL2AxisUI(_group);
            CreateR0AxisUI(_group);
            CreateR1AxisUI(_group);
            CreateR2AxisUI(_group);
            CreateV0AxisUI(_group);
            CreateA0AxisUI(_group);
            CreateA1AxisUI(_group);
            CreateA2AxisUI(_group);
        }

        public virtual void DestroyUI(IUIBuilder builder) => _group.Destroy();
        public virtual void StoreConfig(JSONNode config)
        {
            _group.StoreConfig(config);

            OutputV0CurveEditorSettings?.StoreConfig(config);
            OutputA0CurveEditorSettings?.StoreConfig(config);
            OutputA1CurveEditorSettings?.StoreConfig(config);
            OutputA2CurveEditorSettings?.StoreConfig(config);
        }

        public virtual void RestoreConfig(JSONNode config)
        {
            _group.RestoreConfig(config);

            OutputV0CurveEditorSettings?.RestoreConfig(config);
            OutputA0CurveEditorSettings?.RestoreConfig(config);
            OutputA1CurveEditorSettings?.RestoreConfig(config);
            OutputA2CurveEditorSettings?.RestoreConfig(config);
        }

        private void CreateL0AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            L0AxisTitle = builder.CreateButton("Up/Down | L0", () => group.SetVisible(visible = !visible), Color.red * 0.8f, Color.white, true);
            RangeMaxL0Slider = group.CreateSlider("Device:L0:RangeMax", "Range Max (%)", 1f, 0.01f, 1f, v => RangeMinL0Slider.val = Mathf.Clamp(RangeMinL0Slider.val, 0f, v - 0.01f), true, true, true, "P0");
            RangeMinL0Slider = group.CreateSlider("Device:L0:RangeMin", "Range Min (%)", 0f, 0f, 0.99f, v => RangeMaxL0Slider.val = Mathf.Clamp(RangeMaxL0Slider.val, v + 0.01f, 1f), true, true, true, "P0");
            OutputMaxL0Slider = group.CreateSlider("Device:L0:OutputMax", "Output Max (%)", 1f, 0.01f, 1f, v => OutputMinL0Slider.val = Mathf.Clamp(OutputMinL0Slider.val, 0f, v - 0.01f), true, true, true, "P0");
            OutputMinL0Slider = group.CreateSlider("Device:L0:OutputMin", "Output Min (%)", 0f, 0f, 0.99f, v => OutputMaxL0Slider.val = Mathf.Clamp(OutputMaxL0Slider.val, v + 0.01f, 1f), true, true, true, "P0");
            InvertL0Toggle = group.CreateToggle("Device:L0:Invert", "Invert", true, true);
            EnableOverrideL0Toggle = group.CreateToggle("Device:L0:EnableOverride", "Enable Override", false, true);
            OverrideL0Slider = group.CreateSlider("Device:L0:Override", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }

        private void CreateL1AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            L1AxisTitle = builder.CreateButton("Forward/Backward | L1", () => group.SetVisible(visible = !visible), Color.green * 0.8f, Color.white, true);
            RangeMaxL1Slider = group.CreateSlider("Device:L1:RangeMax", "Range Max (+/- cm)", 0.15f, 0.01f, 1f, true, true, true, "P2");
            OutputMaxL1Slider = group.CreateSlider("Device:L1:OutputMax", "Output Max (+/- %)", 0.5f, 0f, 0.5f, true, true, true, "P0");
            OffsetL1Slider = group.CreateSlider("Device:L1:Offset", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertL1Toggle = group.CreateToggle("Device:L1:Invert", "Invert", false, true);
            EnableOverrideL1Toggle = group.CreateToggle("Device:L1:EnableOverride", "Enable Override", false, true);
            OverrideL1Slider = group.CreateSlider("Device:L1:Override", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }

        private void CreateL2AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            L2AxisTitle = builder.CreateButton("Left/Right | L2", () => group.SetVisible(visible = !visible), Color.blue * 0.8f, Color.white, true);
            RangeMaxL2Slider = group.CreateSlider("Device:L2:RangeMax", "Range Max (+/- cm)", 0.15f, 0.01f, 1f, true, true, true, "P2");
            OutputMaxL2Slider = group.CreateSlider("Device:L2:OutputMax", "Output Max (+/- %)", 0.5f, 0f, 0.5f, true, true, true, "P0");
            OffsetL2Slider = group.CreateSlider("Device:L2:Offset", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertL2Toggle = group.CreateToggle("Device:L2:Invert", "Invert", false, true);
            EnableOverrideL2Toggle = group.CreateToggle("Device:L2:EnableOverride", "Enable Override", false, true);
            OverrideL2Slider = group.CreateSlider("Device:L2:Override", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }

        private void CreateR0AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            R0AxisTitle = builder.CreateButton("Twist | R0", () => group.SetVisible(visible = !visible), Color.cyan * 0.8f, Color.white, true);
            RangeMaxR0Slider = group.CreateSlider("Device:R0:RangeMax", "Range Max (+/- \u00b0)", 90, 1, 179, true, true, true, "F0");
            OutputMaxR0Slider = group.CreateSlider("Device:R0:OutputMax", "Output Max (+/- %)", 0.5f, 0f, 0.5f, true, true, true, "P0");
            OffsetR0Slider = group.CreateSlider("Device:R0:Offset", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertR0Toggle = group.CreateToggle("Device:R0:Invert", "Invert", false, true);
            EnableOverrideR0Toggle = group.CreateToggle("Device:R0:EnableOverride", "Enable Override", false, true);
            OverrideR0Slider = group.CreateSlider("Device:R0:Override", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }

        private void CreateR1AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            R1AxisTitle = builder.CreateButton("Roll | R1", () => group.SetVisible(visible = !visible), Color.magenta * 0.8f, Color.white, true);
            RangeMaxR1Slider = group.CreateSlider("Device:R1:RangeMax", "Range Max (+/-  \u00b0)", 30, 1, 89, true, true, true, "F0");
            OutputMaxR1Slider = group.CreateSlider("Device:R1:OutputMax", "Output Max (+/- %)", 0.5f, 0f, 0.5f, true, true, true, "P0");
            OffsetR1Slider = group.CreateSlider("Device:R1:Offset", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertR1Toggle = group.CreateToggle("Device:R1:Invert", "Invert", false, true);
            EnableOverrideR1Toggle = group.CreateToggle("Device:R1:EnableOverride", "Enable Override", false, true);
            OverrideR1Slider = group.CreateSlider("Device:R1:Override", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }

        private void CreateR2AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            R2AxisTitle = builder.CreateButton("Pitch | R2", () => group.SetVisible(visible = !visible), Color.yellow * 0.8f, Color.white, true);
            RangeMaxR2Slider = group.CreateSlider("Device:R2:RangeMax", "Range Max (+/-  \u00b0)", 30, 1, 89, true, true, true, "F0");
            OutputMaxR2Slider = group.CreateSlider("Device:R2:OutputMax", "Output Max (+/- %)", 0.5f, 0.01f, 0.5f, true, true, true, "P0");
            OffsetR2Slider = group.CreateSlider("Device:R2:Offset", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertR2Toggle = group.CreateToggle("Device:R2:Invert", "Invert", false, true);
            EnableOverrideR2Toggle = group.CreateToggle("Device:R2:EnableOverride", "Enable Override", false, true);
            OverrideR2Slider = group.CreateSlider("Device:R2:Override", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }

        private void CreateV0AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            V0AxisTitle = builder.CreateButton("Vibe | V0", () =>
            {
                group.SetVisible(visible = !visible);
                OutputV0CurveEditorSettings.SetVisible(visible);
            }, new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputV0CurveEditor = group.CreateCurveEditor(300, true);
            OutputV0Curve = group.CreateCurve("Device:V0:OutputCurve", OutputV0CurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputV0CurveEditor.SetDrawScale(OutputV0Curve, Vector2.one, Vector2.zero, true);

            OutputV0CurveEditorSettings = new DeviceCurveEditorSettings("V0:OutputCurveSettings", OutputV0CurveEditor, OutputV0Curve);
            OutputV0CurveEditorSettings.CreateUI(group);

            EnableOverrideV0Toggle = group.CreateToggle("Device:V0:EnableOverride", "Enable Override", true, true);
            OverrideV0Slider = group.CreateSlider("Device:V0:Override", "Override Value (%)", 0f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }

        private void CreateA0AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            A0AxisTitle = builder.CreateButton("Valve | A0", () =>
            {
                group.SetVisible(visible = !visible);
                OutputA0CurveEditorSettings.SetVisible(visible);
            }, new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputA0CurveEditor = group.CreateCurveEditor(300, true);
            OutputA0Curve = group.CreateCurve("Device:A0:OutputCurve", OutputA0CurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputA0CurveEditor.SetDrawScale(OutputA0Curve, Vector2.one, Vector2.zero, true);

            OutputA0CurveEditorSettings = new DeviceCurveEditorSettings("A0:OutputCurveSettings", OutputA0CurveEditor, OutputA0Curve);
            OutputA0CurveEditorSettings.CreateUI(group);

            EnableOverrideA0Toggle = group.CreateToggle("Device:A0:EnableOverride", "Enable Override", true, true);
            OverrideA0Slider = group.CreateSlider("Device:A0:Override", "Override Value (%)", 0f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }

        private void CreateA1AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            A1AxisTitle = builder.CreateButton("Suck | A1", () =>
            {
                group.SetVisible(visible = !visible);
                OutputA1CurveEditorSettings.SetVisible(visible);
            }, new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputA1CurveEditor = group.CreateCurveEditor(300, true);
            OutputA1Curve = group.CreateCurve("Device:A1:OutputCurve", OutputA1CurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputA1CurveEditor.SetDrawScale(OutputA1Curve, Vector2.one, Vector2.zero, true);

            OutputA1CurveEditorSettings = new DeviceCurveEditorSettings("A1:OutputCurveSettings", OutputA1CurveEditor, OutputA1Curve);
            OutputA1CurveEditorSettings.CreateUI(group);

            EnableOverrideA1Toggle = group.CreateToggle("Device:A1:EnableOverride", "Enable Override", true, true);
            OverrideA1Slider = group.CreateSlider("Device:A1:Override", "Override Value (%)", 0f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }

        private void CreateA2AxisUI(IUIBuilder builder)
        {
            var group = new UIGroup(builder);
            var visible = false;
            A2AxisTitle = builder.CreateButton("Lube | A2", () =>
            {
                group.SetVisible(visible = !visible);
                OutputA2CurveEditorSettings.SetVisible(visible);
            }, new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputA2CurveEditor = group.CreateCurveEditor(300, true);
            OutputA2Curve = group.CreateCurve("Device:A2:OutputCurve", OutputA2CurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputA2CurveEditor.SetDrawScale(OutputA2Curve, Vector2.one, Vector2.zero, true);

            OutputA2CurveEditorSettings = new DeviceCurveEditorSettings("A2:OutputCurveSettings", OutputA2CurveEditor, OutputA2Curve);
            OutputA2CurveEditorSettings.CreateUI(group);

            EnableOverrideA2Toggle = group.CreateToggle("Device:A2:EnableOverride", "Enable Override", true, true);
            OverrideA2Slider = group.CreateSlider("Device:A2:Override", "Override Value (%)", 0f, 0f, 1f, true, true, true, "P0");

            group.SetVisible(false);
        }
    }

    public class DeviceCurveEditorSettings : IUIProvider, IConfigProvider
    {
        private readonly string _name;
        private readonly UICurveEditor _editor;
        private readonly JSONStorableAnimationCurve _storable;
        private readonly Vector2 _offset;
        private UIGroup _timeGroup;

        private JSONStorableStringChooser CurveXAxisChooser;

        private UIHorizontalGroup TimeSliderGroup, TimeToggleGroup;

        private JSONStorableFloat TimeSpanSlider;
        private JSONStorableFloat TimeScrubberSlider;
        private JSONStorableBool TineRunningToggle;
        private JSONStorableBool TimeLoopingToggle;

        public DeviceCurveEditorSettings(string name, UICurveEditor editor, JSONStorableAnimationCurve storable, Vector2 offset = new Vector2())
        {
            _name = name;
            _editor = editor;
            _storable = storable;
            _offset = offset;
        }

        public void CreateUI(IUIBuilder builder)
        {
            CurveXAxisChooser = builder.CreateScrollablePopup($"Device:{_name}:CurveXAxis", "Curve X Axis", new List<string> { "L0", "L1", "L2", "L1+L2", "R0", "R1", "R2", "R1+R2", "L0+R1+R2", "Time" }, "L0", CurveXAxisChooserCallback, true);
            CreateTimeUI(builder);

            CurveXAxisChooserCallback("L0");
        }

        private void CreateTimeUI(IUIBuilder builder)
        {
            _timeGroup = new UIGroup(builder);

            TimeSliderGroup = _timeGroup.CreateHorizontalGroup(510, 125, new Vector2(10, 0), 2, idx => _timeGroup.CreateSliderEx(), true);
            TimeSpanSlider = new JSONStorableFloat($"Device:{_name}:TimeSpan", 1, v =>
            {
                TimeSpanSlider.valNoCallback = Mathf.Round(v);
                TimeScrubberSlider.max = Mathf.Round(v);
                _editor.SetDrawScale(_storable, new Vector2(v, 1), _offset, true);
            }, 1, 300, true, true);

            TimeScrubberSlider = new JSONStorableFloat($"Device:{_name}:TimeScrubberPosition", 0, 0, TimeSpanSlider.val, true, true);

            var timeSpanSlider = TimeSliderGroup.items[0].GetComponent<UIDynamicSlider>();
            timeSpanSlider.Configure("Time Span", TimeSpanSlider.min, TimeSpanSlider.max, TimeSpanSlider.defaultVal, valFormat: "F0", showQuickButtons: false);
            timeSpanSlider.defaultButtonEnabled = false;
            timeSpanSlider.slider.wholeNumbers = true;
            TimeSpanSlider.slider = timeSpanSlider.slider;

            var timeScrubberSlider = TimeSliderGroup.items[1].GetComponent<UIDynamicSlider>();
            timeScrubberSlider.Configure("Scrubber", TimeScrubberSlider.min, TimeScrubberSlider.max, TimeScrubberSlider.defaultVal, valFormat: "F2", showQuickButtons: false);
            TimeScrubberSlider.slider = timeScrubberSlider.slider;

            TimeToggleGroup = _timeGroup.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => _timeGroup.CreateToggleEx(), true);
            TineRunningToggle = new JSONStorableBool($"Device:{_name}:TimeRunning", false);
            TimeLoopingToggle = new JSONStorableBool($"Device:{_name}:TimeLooping", true);

            var timeRunningToggle = TimeToggleGroup.items[0].GetComponent<UIDynamicToggle>();
            timeRunningToggle.label = "Running";
            TineRunningToggle.toggle = timeRunningToggle.toggle;

            var timelLoopingToggle = TimeToggleGroup.items[1].GetComponent<UIDynamicToggle>();
            timelLoopingToggle.label = "Looping";
            TimeLoopingToggle.toggle = timelLoopingToggle.toggle;

            _timeGroup.SetVisible(false);
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(CurveXAxisChooser);
            _timeGroup.Destroy();
        }

        public void SetVisible(bool visible)
        {
            _timeGroup.SetVisible(visible && CurveXAxisChooser.val == "Time");
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(_storable);
            config.Store(CurveXAxisChooser);
            config.Store(TimeSpanSlider);
            config.Store(TineRunningToggle);
            config.Store(TimeLoopingToggle);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(_storable);
            config.Restore(CurveXAxisChooser);
            CurveXAxisChooserCallback(CurveXAxisChooser.val);

            config.Restore(TimeSpanSlider);
            config.Restore(TineRunningToggle);
            config.Restore(TimeLoopingToggle);
        }

        protected void CurveXAxisChooserCallback(string val)
        {
            _timeGroup.SetVisible(false);

            if (val == null)
                return;

            if (val == "Time")
            {
                _timeGroup.SetVisible(true);
                TimeSpanSlider.valNoCallback = 1;
                TimeScrubberSlider.valNoCallback = 0;
                _editor.SetDrawScale(_storable, new Vector2(TimeSpanSlider.val, 1), _offset, true);
            }
            else
            {
                _editor.SetDrawScale(_storable, Vector2.one, _offset, true);
            }

            _storable.SetValToDefault();
            _editor.UpdateCurve(_storable);
        }

        public float Evaluate(float[] xTarget, float[] rTarget)
        {
            var t = 0.0f;
            if (CurveXAxisChooser.val == "L0") t = Mathf.Clamp01(xTarget[0]);
            else if (CurveXAxisChooser.val == "L1") t = xTarget[1];
            else if (CurveXAxisChooser.val == "L2") t = xTarget[2];
            else if (CurveXAxisChooser.val == "L1+L2") t = Mathf.Clamp01(Mathf.Sqrt(xTarget[1] * xTarget[1] + xTarget[2] * xTarget[2]));
            else if (CurveXAxisChooser.val == "R0") t = Mathf.Clamp01(0.5f + rTarget[0]);
            else if (CurveXAxisChooser.val == "R1") t = Mathf.Clamp01(Mathf.Abs(rTarget[1]));
            else if (CurveXAxisChooser.val == "R2") t = Mathf.Clamp01(Mathf.Abs(rTarget[2]));
            else if (CurveXAxisChooser.val == "R1+R2") t = Mathf.Clamp01(Mathf.Sqrt(rTarget[1] * rTarget[1] + rTarget[2] * rTarget[2]));
            else if (CurveXAxisChooser.val == "L0+R1+R2") t = Mathf.Clamp01(Mathf.Sqrt(xTarget[0] * xTarget[0] + rTarget[1] * rTarget[1] + rTarget[2] * rTarget[2]));
            else if (CurveXAxisChooser.val == "Time")
            {
                var timeLimit = TimeSpanSlider.val;
                var time = TimeScrubberSlider.val;
                if (TineRunningToggle.val)
                    time += Time.deltaTime;

                if (time > timeLimit)
                    time = TimeLoopingToggle.val ? 0 : timeLimit;

                t = Mathf.Clamp(time, 0, timeLimit);
                TimeScrubberSlider.val = t;
            }

            _editor.SetScrubberPosition(_storable, t);
            return _storable.val.Evaluate(t);
        }
    }
}
