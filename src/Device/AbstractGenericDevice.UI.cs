using CurveEditor;
using CurveEditor.UI;
using SimpleJSON;
using System;
using System.Collections.Generic;
using ToySerialController.Config;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController
{
    public abstract partial class AbstractGenericDevice : IDevice
    {
        private UIDynamicButton MainTitle;
        private UIHorizontalGroup LimitsButtonGroup;
        private JSONStorableFloat SmoothingSlider;
        private JSONStorableFloat ReferenceLengthScaleSlider;
        private JSONStorableFloat ReferenceRadiusScaleSlider;

        private UIDynamicButton L0AxisTitle;
        private JSONStorableBool InvertL0Toggle;
        private JSONStorableFloat OutputMaxL0Slider;
        private JSONStorableFloat OutputMinL0Slider;
        private JSONStorableBool EnableOverrideL0Toggle;
        private JSONStorableFloat OverrideL0Slider;
        private JSONStorableFloat RangeMaxL0Slider;
        private JSONStorableFloat RangeMinL0Slider;
        private JSONStorableStringChooser ProjectL0Chooser;

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
        private JSONStorableStringChooser OutputV0CurveXAxisChooser;
        private JSONStorableFloat OverrideV0Slider;
        private JSONStorableBool EnableOverrideV0Toggle;
        private AbstractGenericDeviceCurveSettings OutputV0CurveEditorSettings;

        private UIDynamicButton V1AxisTitle;
        private UICurveEditor OutputV1CurveEditor;
        private JSONStorableAnimationCurve OutputV1Curve;
        private JSONStorableStringChooser OutputV1CurveXAxisChooser;
        private JSONStorableFloat OverrideV1Slider;
        private JSONStorableBool EnableOverrideV1Toggle;
        private AbstractGenericDeviceCurveSettings OutputV1CurveEditorSettings;

        private UIDynamicButton L3AxisTitle;
        private UICurveEditor OutputL3CurveEditor;
        private JSONStorableAnimationCurve OutputL3Curve;
        private JSONStorableStringChooser OutputL3CurveXAxisChooser;
        private JSONStorableFloat OverrideL3Slider;
        private JSONStorableBool EnableOverrideL3Toggle;
        private AbstractGenericDeviceCurveSettings OutputL3CurveEditorSettings;

        private UIGroup _group;

        public virtual void CreateUI(IUIBuilder builder)
        {
            _group = new UIGroup(builder);

            var visible = false;
            var mainGroup = new UIGroup(_group);

            MainTitle = _group.CreateButton("Main", () => mainGroup.SetVisible(visible = !visible), new Color(0.3f, 0.3f, 0.3f), Color.white, true);

            SmoothingSlider = mainGroup.CreateSlider("Plugin:Smoothing", "Smoothing (%)", 0.1f, 0.0f, 0.99f, true, true, true, "P0");
            ReferenceLengthScaleSlider = mainGroup.CreateSlider("Device:ReferenceLengthScale", "Reference Length (%)", 1.0f, 0, 10, true, true, true, "P0");
            ReferenceRadiusScaleSlider = mainGroup.CreateSlider("Device:ReferenceRadiusScale", "Reference Radius (%)", 3.0f, 0, 10, true, true, true, "P0");

            CreateCustomUI(mainGroup);
            mainGroup.SetVisible(false);

            CreateL0AxisUI(_group);
            CreateL1AxisUI(_group);
            CreateL2AxisUI(_group);
            CreateR0AxisUI(_group);
            CreateR1AxisUI(_group);
            CreateR2AxisUI(_group);
            CreateV0AxisUI(_group);
            CreateV1AxisUI(_group);
            CreateL3AxisUI(_group);
        }

        public virtual void CreateCustomUI(UIGroup group) { }
        public virtual void DestroyUI(IUIBuilder builder) => _group.Destroy();
        public virtual void StoreConfig(JSONNode config)
        {
            _group.StoreConfig(config);

            OutputV0CurveEditorSettings?.StoreConfig(config);
            OutputV1CurveEditorSettings?.StoreConfig(config);
        }

        public virtual void RestoreConfig(JSONNode config)
        {
            _group.RestoreConfig(config);

            OutputV0CurveEditorSettings?.RestoreConfig(config);
            OutputV1CurveEditorSettings?.RestoreConfig(config);
        }

        private void CreateL0AxisUI(IUIBuilder builder)
        {
            var l0Group = new UIGroup(builder);
            var visible = false;
            L0AxisTitle = builder.CreateButton("Up/Down | L0", () => l0Group.SetVisible(visible = !visible), Color.red * 0.8f, Color.white, true);
            RangeMaxL0Slider = l0Group.CreateSlider("Device:RangeMaxL0", "Range Max (%)", 1f, 0.01f, 1f, v => RangeMinL0Slider.max = v - 0.01f, true, true, true, "P0");
            RangeMinL0Slider = l0Group.CreateSlider("Device:RangeMinL0", "Range Min (%)", 0f, 0f, 0.99f, v => RangeMaxL0Slider.min = v + 0.01f, true, true, true, "P0");
            OutputMaxL0Slider = l0Group.CreateSlider("Device:OutputMaxL0", "Output Max (%)", 1f, 0f, 1f, v => OutputMinL0Slider.max = v, true, true, true, "P0");
            OutputMinL0Slider = l0Group.CreateSlider("Device:OutputMinL0", "Output Min (%)", 0, 0f, 1f, v => OutputMaxL0Slider.min = v, true, true, true, "P0");
            InvertL0Toggle = l0Group.CreateToggle("Device:InvertL0", "Invert", true, true);
            EnableOverrideL0Toggle = l0Group.CreateToggle("Device:EnableOverrideL0", "Enable Override", false, true);
            OverrideL0Slider = l0Group.CreateSlider("Device:OverrideL0", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");
            ProjectL0Chooser = l0Group.CreateScrollablePopup("Device:ProjectL0", "Select Projection Axis", new List<string> { "Difference", "Reference Up", "Target Up" }, "Reference Up", null, true);

            l0Group.SetVisible(false);
        }

        private void CreateL1AxisUI(IUIBuilder builder)
        {
            var l1Group = new UIGroup(builder);
            var visible = false;
            L1AxisTitle = builder.CreateButton("Left/Right | L1", () => l1Group.SetVisible(visible = !visible), Color.green * 0.8f, Color.white, true);
            RangeMaxL1Slider = l1Group.CreateSlider("Device:RangeMaxL1", "Range Max (+/- cm)", 0.15f, 0.01f, 1f, true, true, true, "P2");
            OutputMaxL1Slider = l1Group.CreateSlider("Device:OutputMaxL1", "Output Max (+/- %)", 0.5f, 0f, 0.5f, true, true, true, "P0");
            OffsetL1Slider = l1Group.CreateSlider("Device:OffsetL1", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertL1Toggle = l1Group.CreateToggle("Device:InvertL1", "Invert", false, true);
            EnableOverrideL1Toggle = l1Group.CreateToggle("Device:EnableOverrideL1", "Enable Override", false, true);
            OverrideL1Slider = l1Group.CreateSlider("Device:OverrideL1", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            l1Group.SetVisible(false);
        }

        private void CreateL2AxisUI(IUIBuilder builder)
        {
            var l2Group = new UIGroup(builder);
            var visible = false;
            L2AxisTitle = builder.CreateButton("Forward/Backward | L2", () => l2Group.SetVisible(visible = !visible), Color.blue * 0.8f, Color.white, true);
            RangeMaxL2Slider = l2Group.CreateSlider("Device:RangeMaxL2", "Range Max (+/- cm)", 0.15f, 0.01f, 1f, true, true, true, "P2");
            OutputMaxL2Slider = l2Group.CreateSlider("Device:OutputMaxL2", "Output Max (+/- %)", 0.5f, 0f, 0.5f, true, true, true, "P0");
            OffsetL2Slider = l2Group.CreateSlider("Device:OffsetL2", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertL2Toggle = l2Group.CreateToggle("Device:InvertL2", "Invert", false, true);
            EnableOverrideL2Toggle = l2Group.CreateToggle("Device:EnableOverrideL2", "Enable Override", false, true);
            OverrideL2Slider = l2Group.CreateSlider("Device:OverrideL2", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            l2Group.SetVisible(false);
        }

        private void CreateR0AxisUI(IUIBuilder builder)
        {
            var r0Group = new UIGroup(builder);
            var visible = false;
            R0AxisTitle = builder.CreateButton("Twist | R0", () => r0Group.SetVisible(visible = !visible), Color.cyan * 0.8f, Color.white, true);
            RangeMaxR0Slider = r0Group.CreateSlider("Device:RangeMaxR0", "Range Max (+/- \u00b0)", 90, 1, 179, true, true, true, "F0");
            OutputMaxR0Slider = r0Group.CreateSlider("Device:OutputMaxR0", "Output Max (+/- %)", 0.5f, 0f, 0.5f, true, true, true, "P0");
            OffsetR0Slider = r0Group.CreateSlider("Device:OffsetR0", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertR0Toggle = r0Group.CreateToggle("Device:InvertR0", "Invert", false, true);
            EnableOverrideR0Toggle = r0Group.CreateToggle("Device:EnableOverrideR0", "Enable Override", false, true);
            OverrideR0Slider = r0Group.CreateSlider("Device:OverrideR0", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            r0Group.SetVisible(false);
        }

        private void CreateR1AxisUI(IUIBuilder builder)
        {
            var r1Group = new UIGroup(builder);
            var visible = false;
            R1AxisTitle = builder.CreateButton("Pitch | R1", () => r1Group.SetVisible(visible = !visible), Color.magenta * 0.8f, Color.white, true);
            RangeMaxR1Slider = r1Group.CreateSlider("Device:RangeMaxR1", "Range Max (+/-  \u00b0)", 30, 1, 89, true, true, true, "F0");
            OutputMaxR1Slider = r1Group.CreateSlider("Device:OutputMaxR1", "Output Max (+/- %)", 0.5f, 0f, 0.5f, true, true, true, "P0");
            OffsetR1Slider = r1Group.CreateSlider("Device:OffsetR1", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertR1Toggle = r1Group.CreateToggle("Device:InvertR1", "Invert", false, true);
            EnableOverrideR1Toggle = r1Group.CreateToggle("Device:EnableOverrideR1", "Enable Override", false, true);
            OverrideR1Slider = r1Group.CreateSlider("Device:OverrideR1", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            r1Group.SetVisible(false);
        }

        private void CreateR2AxisUI(IUIBuilder builder)
        {
            var r2Group = new UIGroup(builder);
            var visible = false;
            R2AxisTitle = builder.CreateButton("Roll | R2", () => r2Group.SetVisible(visible = !visible), Color.yellow * 0.8f, Color.white, true);
            RangeMaxR2Slider = r2Group.CreateSlider("Device:RangeMaxR2", "Range Max (+/-  \u00b0)", 30, 1, 89, true, true, true, "F0");
            OutputMaxR2Slider = r2Group.CreateSlider("Device:OutputMaxR2", "Output Max (+/- %)", 0.5f, 0.01f, 0.5f, true, true, true, "P0");
            OffsetR2Slider = r2Group.CreateSlider("Device:OffsetR2", "Offset (%)", 0f, -0.25f, 0.25f, true, true, true, "P0");
            InvertR2Toggle = r2Group.CreateToggle("Device:InvertR2", "Invert", false, true);
            EnableOverrideR2Toggle = r2Group.CreateToggle("Device:EnableOverrideR2", "Enable Override", false, true);
            OverrideR2Slider = r2Group.CreateSlider("Device:OverrideR2", "Override Value (%)", 0.5f, 0f, 1f, true, true, true, "P0");

            r2Group.SetVisible(false);
        }

        private void CreateV0AxisUI(IUIBuilder builder)
        {
            var v0Group = new UIGroup(builder);
            var visible = false;
            V0AxisTitle = builder.CreateButton("Custom 0 | V0", () => 
            { 
                v0Group.SetVisible(visible = !visible);
                OutputV0CurveEditorSettings.SetVisible(visible);
            }, new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputV0CurveEditor = v0Group.CreateCurveEditor(300, true);
            OutputV0Curve = v0Group.CreateCurve("Device:OutputV0Curve", OutputV0CurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputV0CurveEditor.SetDrawScale(OutputV0Curve, Vector2.one, Vector2.zero, true);

            OutputV0CurveEditorSettings = new AbstractGenericDeviceCurveSettings("OutputV0CurveSettings", OutputV0CurveEditor, OutputV0Curve);
            OutputV0CurveEditorSettings.CreateUI(v0Group);

            EnableOverrideV0Toggle = v0Group.CreateToggle("Device:EnableOverrideV0", "Enable Override", true, true);
            OverrideV0Slider = v0Group.CreateSlider("Device:OverrideV0", "Override Value (%)", 0f, 0f, 1f, true, true, true, "P0");

            v0Group.SetVisible(false);
        }

        private void CreateV1AxisUI(IUIBuilder builder)
        {
            var v1Group = new UIGroup(builder);
            var visible = false;
            V1AxisTitle = builder.CreateButton("Custom 1 | V1", () =>
            {
                v1Group.SetVisible(visible = !visible);
                OutputV1CurveEditorSettings.SetVisible(visible);
            }, new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputV1CurveEditor = v1Group.CreateCurveEditor(300, true);
            OutputV1Curve = v1Group.CreateCurve("Device:OutputV1Curve", OutputV1CurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputV1CurveEditor.SetDrawScale(OutputV1Curve, Vector2.one, Vector2.zero, true);

            OutputV1CurveEditorSettings = new AbstractGenericDeviceCurveSettings("OutputV1CurveSettings", OutputV1CurveEditor, OutputV1Curve);
            OutputV1CurveEditorSettings.CreateUI(v1Group);

            EnableOverrideV1Toggle = v1Group.CreateToggle("Device:EnableOverrideV1", "Enable Override", true, true);
            OverrideV1Slider = v1Group.CreateSlider("Device:OverrideV1", "Override Value (%)", 0f, 0f, 1f, true, true, true, "P0");

            v1Group.SetVisible(false);
        }

        private void CreateL3AxisUI(IUIBuilder builder)
        {
            var l3Group = new UIGroup(builder);
            var visible = false;
            L3AxisTitle = builder.CreateButton("Valve | L3", () =>
            {
                l3Group.SetVisible(visible = !visible);
                OutputL3CurveEditorSettings.SetVisible(visible);
            }, new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputL3CurveEditor = l3Group.CreateCurveEditor(300, true);
            OutputL3Curve = l3Group.CreateCurve("Device:OutputL3Curve", OutputL3CurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputL3CurveEditor.SetDrawScale(OutputL3Curve, Vector2.one, Vector2.zero, true);

            OutputL3CurveEditorSettings = new AbstractGenericDeviceCurveSettings("OutputL3CurveSettings", OutputL3CurveEditor, OutputL3Curve);
            OutputL3CurveEditorSettings.CreateUI(l3Group);

            EnableOverrideL3Toggle = l3Group.CreateToggle("Device:EnableOverrideL3", "Enable Override", true, true);
            OverrideL3Slider = l3Group.CreateSlider("Device:OverrideL3", "Override Value (%)", 0f, 0f, 1f, true, true, true, "P0");

            l3Group.SetVisible(false);
        }
    }

    public class AbstractGenericDeviceCurveSettings : IUIProvider, IConfigProvider
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

        public AbstractGenericDeviceCurveSettings(string name, UICurveEditor editor, JSONStorableAnimationCurve storable, Vector2 offset = new Vector2())
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
            config.Store(CurveXAxisChooser);
            config.Store(TimeSpanSlider);
            config.Store(TineRunningToggle);
            config.Store(TimeLoopingToggle);
        }

        public void RestoreConfig(JSONNode config)
        {
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

        public float Evaluate(Vector3 xTarget, Vector3 rTarget)
        {
            var t = 0.0f;
            if (CurveXAxisChooser.val == "L0") t = Mathf.Clamp01(xTarget.x);
            else if (CurveXAxisChooser.val == "L1") t = xTarget.y;
            else if (CurveXAxisChooser.val == "L2") t = xTarget.z;
            else if (CurveXAxisChooser.val == "L1+L2") t = Mathf.Clamp01(Mathf.Sqrt(xTarget.y * xTarget.y + xTarget.z * xTarget.z));
            else if (CurveXAxisChooser.val == "R0") t = Mathf.Clamp01(0.5f + rTarget.x);
            else if (CurveXAxisChooser.val == "R1") t = Mathf.Clamp01(Mathf.Abs(rTarget.y));
            else if (CurveXAxisChooser.val == "R2") t = Mathf.Clamp01(Mathf.Abs(rTarget.z));
            else if (CurveXAxisChooser.val == "R1+R2") t = Mathf.Clamp01(Mathf.Sqrt(rTarget.y * rTarget.y + rTarget.z * rTarget.z));
            else if (CurveXAxisChooser.val == "L0+R1+R2") t = Mathf.Clamp01(Mathf.Sqrt(xTarget.x * xTarget.x + rTarget.y * rTarget.y + rTarget.z * rTarget.z));
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
