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
        private UIHorizontalGroup LimitsButtonGroup;

        private UIDynamicButton XAxisTitle;
        private JSONStorableBool InvertXToggle;
        private JSONStorableFloat OutputMaxXSlider;
        private JSONStorableFloat OutputMinXSlider;
        private JSONStorableBool EnableOverrideXToggle;
        private JSONStorableFloat OverrideXSlider;
        private JSONStorableFloat RangeMaxXSlider;
        private JSONStorableFloat RangeMinXSlider;
        private JSONStorableStringChooser ProjectXChooser;

        private UIDynamicButton YAxisTitle;
        private JSONStorableBool InvertYToggle;
        private JSONStorableBool EnableOverrideYToggle;
        private JSONStorableFloat OutputMaxYSlider;
        private JSONStorableFloat AdjustYSlider;
        private JSONStorableFloat OverrideYSlider;
        private JSONStorableFloat RangeMaxYSlider;

        private UIDynamicButton ZAxisTitle;
        private JSONStorableBool InvertZToggle;
        private JSONStorableBool EnableOverrideZToggle;
        private JSONStorableFloat OutputMaxZSlider;
        private JSONStorableFloat OutputMaxRZSlider;
        private JSONStorableFloat AdjustZSlider;
        private JSONStorableFloat OverrideZSlider;
        private JSONStorableFloat RangeMaxZSlider;

        private UIDynamicButton RXAxisTitle;
        private JSONStorableBool InvertRXToggle;
        private JSONStorableBool EnableOverrideRXToggle;
        private JSONStorableFloat OverrideRXSlider;
        private JSONStorableFloat OutputMaxRXSlider;
        private UICurveEditor OutputRXCurveEditor;
        private JSONStorableAnimationCurve OutputRXCurve;
        private JSONStorableStringChooser OutputRXCurveXAxisChooser;
        private AbstractGenericDeviceCurveSettings OutputRXCurveEditorSettings;

        private UIDynamicButton RYAxisTitle;
        private JSONStorableBool InvertRYToggle;
        private JSONStorableFloat OutputMaxRYSlider;
        private JSONStorableBool EnableOverrideRYToggle;
        private JSONStorableFloat AdjustRYSlider;
        private JSONStorableFloat OverrideRYSlider;
        private JSONStorableFloat RangeMaxRYSlider;

        private UIDynamicButton RZAxisTitle;
        private JSONStorableBool InvertRZToggle;
        private JSONStorableFloat AdjustRZSlider;
        private JSONStorableFloat OverrideRZSlider;
        private JSONStorableFloat RangeMaxRZSlider;
        private JSONStorableFloat SmoothingSlider;
        private JSONStorableBool EnableOverrideRZToggle;

        private UIDynamicButton Vibe0Title;
        private UICurveEditor OutputV0CurveEditor;
        private JSONStorableAnimationCurve OutputV0Curve;
        private JSONStorableStringChooser OutputV0CurveXAxisChooser;
        private JSONStorableFloat OverrideV0Slider;
        private JSONStorableBool EnableOverrideV0Toggle;
        private AbstractGenericDeviceCurveSettings OutputV0CurveEditorSettings;

        private UIDynamicButton Vibe1Title;
        private UICurveEditor OutputV1CurveEditor;
        private JSONStorableAnimationCurve OutputV1Curve;
        private JSONStorableStringChooser OutputV1CurveXAxisChooser;
        private JSONStorableFloat OverrideV1Slider;
        private JSONStorableBool EnableOverrideV1Toggle;
        private AbstractGenericDeviceCurveSettings OutputV1CurveEditorSettings;

        private UIGroup _group;

        public virtual void CreateUI(IUIBuilder builder)
        {
            _group = new UIGroup(builder);
            LimitsButtonGroup = _group.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => _group.CreateButtonEx(), true);
            var resetButton = LimitsButtonGroup.items[0].GetComponent<UIDynamicButton>();
            resetButton.label = "Reset Limits";
            resetButton.button.onClick.AddListener(PositionResetButtonCallback);
            
            var applyButton = LimitsButtonGroup.items[1].GetComponent<UIDynamicButton>();
            applyButton.label = "Apply Limits";
            applyButton.button.onClick.AddListener(PositionApplyButtonCallback);

            SmoothingSlider = _group.CreateSlider("Plugin:Smoothing", "Smoothing", 0.1f, 0.0f, 0.99f, true, true, true);

            CreateXAxisUI(_group);
            CreateYAxisUI(_group);
            CreateZAxisUI(_group);
            CreateRXAxisUI(_group);
            CreateRYAxisUI(_group);
            CreateRZAxisUI(_group);
            CreateVibe0UI(_group);
            CreateVibe1UI(_group);
        }

        public virtual void DestroyUI(IUIBuilder builder) => _group.Destroy();
        public virtual void StoreConfig(JSONNode config)
        {
            _group.StoreConfig(config);

            OutputRXCurveEditorSettings?.StoreConfig(config);
            OutputV0CurveEditorSettings?.StoreConfig(config);
            OutputV1CurveEditorSettings?.StoreConfig(config);
        }

        public virtual void RestoreConfig(JSONNode config)
        {
            _group.RestoreConfig(config);

            OutputRXCurveEditorSettings?.RestoreConfig(config);
            OutputV0CurveEditorSettings?.RestoreConfig(config);
            OutputV1CurveEditorSettings?.RestoreConfig(config);
        }

        private void CreateXAxisUI(IUIBuilder builder)
        {
            XAxisTitle = builder.CreateDisabledButton("X Axis", Color.red * 0.8f, Color.white, true);
            RangeMaxXSlider = builder.CreateSlider("Device:RangeMaxX", "Range Max", 1f, 0f, 1f, true, true, true);
            RangeMinXSlider = builder.CreateSlider("Device:RangeMinX", "Range Min", 0f, 0f, 1f, true, true, true);
            OutputMaxXSlider = builder.CreateSlider("Device:OutputMaxX", "Output Max", 1f, 0.5f, 1f, true, true, true);
            OutputMinXSlider = builder.CreateSlider("Device:OutputMinX", "Output Min", 0, 0f, 0.5f, true, true, true);
            InvertXToggle = builder.CreateToggle("Device:InvertX", "Invert", true, true);
            EnableOverrideXToggle = builder.CreateToggle("Device:EnableOverrideX", "Enable Override", false, true);
            OverrideXSlider = builder.CreateSlider("Device:OverrideX", "Override Value", 0.5f, 0f, 1f, true, true, true);
            ProjectXChooser = builder.CreateScrollablePopup("Device:ProjectX", "Select Projection Axis", new List<string> { "Default", "Reference Up", "Target Up" }, "Default", null, true);
        }

        private void CreateYAxisUI(IUIBuilder builder)
        {
            YAxisTitle = builder.CreateDisabledButton("Y Axis", Color.green * 0.8f, Color.white, true);
            RangeMaxYSlider = builder.CreateSlider("Device:RangeMaxY", "Range Max (+/-)", 1f, 0, 2f, true, true, true);
            OutputMaxYSlider = builder.CreateSlider("Device:OutputMaxY", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            AdjustYSlider = builder.CreateSlider("Device:AdjustY", "Adjust", 0f, -0.25f, 0.25f, true, true, true);
            InvertYToggle = builder.CreateToggle("Device:InvertY", "Invert", false, true);
            EnableOverrideYToggle = builder.CreateToggle("Device:EnableOverrideY", "Enable Override", false, true);
            OverrideYSlider = builder.CreateSlider("Device:OverrideY", "Override Value", 0.5f, 0f, 1f, true, true, true);
        }

        private void CreateZAxisUI(IUIBuilder builder)
        {
            ZAxisTitle = builder.CreateDisabledButton("Z Axis", Color.blue * 0.8f, Color.white, true);
            RangeMaxZSlider = builder.CreateSlider("Device:RangeMaxZ", "Range Max (+/-)", 1f, 0, 2f, true, true, true);
            OutputMaxZSlider = builder.CreateSlider("Device:OutputMaxZ", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            AdjustZSlider = builder.CreateSlider("Device:AdjustZ", "Adjust", 0f, -0.25f, 0.25f, true, true, true);
            InvertZToggle = builder.CreateToggle("Device:InvertZ", "Invert", false, true);
            EnableOverrideZToggle = builder.CreateToggle("Device:EnableOverrideZ", "Enable Override", false, true);
            OverrideZSlider = builder.CreateSlider("Device:OverrideZ", "Override Value", 0.5f, 0f, 1f, true, true, true);
        }

        private void CreateRXAxisUI(IUIBuilder builder)
        {
            RXAxisTitle = builder.CreateDisabledButton("RX Axis", Color.cyan * 0.8f, Color.white, true);
            OutputRXCurveEditor = builder.CreateCurveEditor(300, true);
            OutputRXCurve = builder.CreateCurve("Device:OutputRXCurve", OutputRXCurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 0.5f), new Keyframe(1, 0.5f, 0.5f, 0) });
            OutputRXCurveEditor.SetDrawScale(OutputRXCurve, Vector2.one, new Vector2(0, 0.5f), true);

            OutputRXCurveEditorSettings = new AbstractGenericDeviceCurveSettings("OutputRXCurveSettings", OutputRXCurveEditor, OutputRXCurve);
            OutputRXCurveEditorSettings.CreateUI(builder);

            OutputMaxRXSlider = builder.CreateSlider("Device:OutputMaxRX", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            InvertRXToggle = builder.CreateToggle("Device:InvertRX", "Invert", false, true);
            EnableOverrideRXToggle = builder.CreateToggle("Device:EnableOverrideRX", "Enable Override", true, true);
            OverrideRXSlider = builder.CreateSlider("Device:OverrideRX", "Override Value", 0.5f, 0f, 1f, true, true, true);
        }

        private void CreateRYAxisUI(IUIBuilder builder)
        {
            RYAxisTitle = builder.CreateDisabledButton("RY Axis", Color.magenta * 0.8f, Color.white, true);
            RangeMaxRYSlider = builder.CreateSlider("Device:RangeMaxRY", "Range Max (+/-)", 0.4f, 0f, 1f, true, true, true);
            OutputMaxRYSlider = builder.CreateSlider("Device:OutputMaxRY", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            AdjustRYSlider = builder.CreateSlider("Device:AdjustRY", "Adjust", 0f, -0.25f, 0.25f, true, true, true);
            InvertRYToggle = builder.CreateToggle("Device:InvertRY", "Invert", false, true);
            EnableOverrideRYToggle = builder.CreateToggle("Device:EnableOverrideRY", "Enable Override", false, true);
            OverrideRYSlider = builder.CreateSlider("Device:OverrideRY", "Override Value", 0.5f, 0f, 1f, true, true, true);
        }

        private void CreateRZAxisUI(IUIBuilder builder)
        {
            RZAxisTitle = builder.CreateDisabledButton("RZ Axis", Color.yellow * 0.8f, Color.white, true);
            RangeMaxRZSlider = builder.CreateSlider("Device:RangeMaxRZ", "Range Max (+/-)", 0.4f, 0f, 1f, true, true, true);
            OutputMaxRZSlider = builder.CreateSlider("Device:OutputMaxRZ", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            AdjustRZSlider = builder.CreateSlider("Device:AdjustRZ", "Adjust", 0f, -0.25f, 0.25f, true, true, true);
            InvertRZToggle = builder.CreateToggle("Device:InvertRZ", "Invert", false, true);
            EnableOverrideRZToggle = builder.CreateToggle("Device:EnableOverrideRZ", "Enable Override", false, true);
            OverrideRZSlider = builder.CreateSlider("Device:OverrideRZ", "Override Value", 0.5f, 0f, 1f, true, true, true);
        }

        private void CreateVibe0UI(IUIBuilder builder)
        {
            Vibe0Title = builder.CreateDisabledButton("Vibe 0", new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputV0CurveEditor = builder.CreateCurveEditor(300, true);
            OutputV0Curve = builder.CreateCurve("Device:OutputV0Curve", OutputV0CurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputV0CurveEditor.SetDrawScale(OutputV0Curve, Vector2.one, Vector2.zero, true);

            OutputV0CurveEditorSettings = new AbstractGenericDeviceCurveSettings("OutputV0CurveSettings", OutputV0CurveEditor, OutputV0Curve);
            OutputV0CurveEditorSettings.CreateUI(builder);

            EnableOverrideV0Toggle = builder.CreateToggle("Device:EnableOverrideV0", "Enable Override", true, true);
            OverrideV0Slider = builder.CreateSlider("Device:OverrideV0", "Override Value", 0f, 0f, 1f, true, true, true);
        }

        private void CreateVibe1UI(IUIBuilder builder)
        {
            Vibe1Title = builder.CreateDisabledButton("Vibe 1", new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputV1CurveEditor = builder.CreateCurveEditor(300, true);
            OutputV1Curve = builder.CreateCurve("Device:OutputV1Curve", OutputV1CurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputV1CurveEditor.SetDrawScale(OutputV1Curve, Vector2.one, Vector2.zero, true);

            OutputV1CurveEditorSettings = new AbstractGenericDeviceCurveSettings("OutputV1CurveSettings", OutputV1CurveEditor, OutputV1Curve);
            OutputV1CurveEditorSettings.CreateUI(builder);

            EnableOverrideV1Toggle = builder.CreateToggle("Device:EnableOverrideV1", "Enable Override", true, true);
            OverrideV1Slider = builder.CreateSlider("Device:OverrideV1", "Override Value", 0f, 0f, 1f, true, true, true);
        }

        protected void PositionResetButtonCallback()
        {
            _xTargetMin = _xTargetMax = _xTarget;
            _rTargetMin = _rTargetMax = _rTarget;
        }

        protected void PositionApplyButtonCallback()
        {
            RangeMaxXSlider.val = _xTargetMax.x;
            RangeMinXSlider.val = _xTargetMin.x;

            RangeMaxYSlider.val = Math.Max(Math.Abs(_xTargetMax.y), Math.Abs(_xTargetMin.y));
            RangeMaxZSlider.val = Math.Max(Math.Abs(_xTargetMax.z), Math.Abs(_xTargetMin.z));

            //RangeMaxRX.val = Math.Max(Math.Abs(_rTargetMax.x), Math.Abs(_rTargetMin.x));
            RangeMaxRYSlider.val = Math.Max(Math.Abs(_rTargetMax.y), Math.Abs(_rTargetMin.y));
            RangeMaxRZSlider.val = Math.Max(Math.Abs(_rTargetMax.z), Math.Abs(_rTargetMin.z));
        }
    }

    public class AbstractGenericDeviceCurveSettings : IUIProvider, IConfigProvider
    {
        private readonly string _name;
        private readonly UICurveEditor _editor;
        private readonly JSONStorableAnimationCurve _storable;
        private UIGroup _group;

        private JSONStorableStringChooser CurveXAxisChooser;

        private UIHorizontalGroup TimeSliderGroup, TimeToggleGroup;

        private JSONStorableFloat TimeSpanSlider;
        private JSONStorableFloat TimeScrubberSlider;
        private JSONStorableBool TineRunningToggle;
        private JSONStorableBool TimeLoopingToggle;

        public AbstractGenericDeviceCurveSettings(string name, UICurveEditor editor, JSONStorableAnimationCurve storable)
        {
            _name = name;
            _editor = editor;
            _storable = storable;
        }

        public void CreateUI(IUIBuilder builder)
        {
            CurveXAxisChooser = builder.CreateScrollablePopup($"Device:{_name}:CurveXAxis", "Curve X Axis", new List<string> { "X", "Y", "Z", "Y+Z", "RX", "RY", "RZ", "RY+RZ", "X+RY+RZ", "Time" }, "X", CurveXAxisChooserCallback, true);
            _group = new UIGroup(builder);

            CurveXAxisChooserCallback("X");
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(CurveXAxisChooser);
            _group.Destroy();
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
            _group.Destroy();
            if (val == "Time")
            {
                var baseIndex = CurveXAxisChooser.popup.transform.GetSiblingIndex();

                TimeSliderGroup = _group.CreateHorizontalGroup(510, 125, new Vector2(10, 0), 2, idx => _group.CreateSliderEx(), true);
                TimeSpanSlider = new JSONStorableFloat($"Device:{_name}:TimeSpan", 1, v =>
                {
                    TimeSpanSlider.valNoCallback = Mathf.Round(v);
                    TimeScrubberSlider.max = Mathf.Round(v);
                    _editor.SetDrawScale(_storable, Vector2.zero, new Vector2(v, 1), true);
                }, 1, 300, true, true);

                TimeScrubberSlider = new JSONStorableFloat($"Device:{_name}:TimeScrubberPosition", 0, 0, TimeSpanSlider.val, true, true);

                var timeSpanSlider = TimeSliderGroup.items[0].GetComponent<UIDynamicSlider>();
                timeSpanSlider.Configure("Time Span", TimeSpanSlider.min, TimeSpanSlider.max, TimeSpanSlider.defaultVal, valFormat: "F0", showQuickButtons: false);
                timeSpanSlider.defaultButtonEnabled = false;
                TimeSpanSlider.slider = timeSpanSlider.slider;

                var timeScrubberSlider = TimeSliderGroup.items[1].GetComponent<UIDynamicSlider>();
                timeScrubberSlider.Configure("Scrubber", TimeScrubberSlider.min, TimeScrubberSlider.max, TimeScrubberSlider.defaultVal, valFormat: "F2", showQuickButtons: false);
                TimeScrubberSlider.slider = timeScrubberSlider.slider;

                TimeToggleGroup = _group.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => _group.CreateToggleEx(), true);
                TineRunningToggle = new JSONStorableBool($"Device:{_name}:TimeRunning", false);
                TimeLoopingToggle = new JSONStorableBool($"Device:{_name}:TimeLooping", true);

                var timeRunningToggle = TimeToggleGroup.items[0].GetComponent<UIDynamicToggle>();
                timeRunningToggle.label = "Running";
                TineRunningToggle.toggle = timeRunningToggle.toggle;

                var timelLoopingToggle = TimeToggleGroup.items[1].GetComponent<UIDynamicToggle>();
                timelLoopingToggle.label = "Looping";
                TimeLoopingToggle.toggle = timelLoopingToggle.toggle;

                TimeSliderGroup.container.transform.SetSiblingIndex(baseIndex + 1);
                TimeToggleGroup.container.transform.SetSiblingIndex(baseIndex + 2);
            }
            else
            {
                _storable.SetValToDefault();
            }
        }

        public float Evaluate(Vector3 xTarget, Vector3 rTarget)
        {
            var t = xTarget.x;
            if (CurveXAxisChooser.val == "X") t = Mathf.Clamp01(xTarget.x);
            if (CurveXAxisChooser.val == "Y") t = xTarget.y;
            if (CurveXAxisChooser.val == "Z") t = xTarget.z;
            if (CurveXAxisChooser.val == "Y+Z") t = Mathf.Clamp01(Mathf.Sqrt(xTarget.y * xTarget.y + xTarget.z * xTarget.z));
            else if (CurveXAxisChooser.val == "RX") t = Mathf.Clamp01(Mathf.Abs(rTarget.x));
            else if (CurveXAxisChooser.val == "RY") t = Mathf.Clamp01(Mathf.Abs(rTarget.y));
            else if (CurveXAxisChooser.val == "RZ") t = Mathf.Clamp01(Mathf.Abs(rTarget.z));
            else if (CurveXAxisChooser.val == "RY+RZ") t = Mathf.Clamp01(Mathf.Sqrt(rTarget.y * rTarget.y + rTarget.z * rTarget.z));
            else if (CurveXAxisChooser.val == "X+RY+RZ") t = Mathf.Clamp01(Mathf.Sqrt(xTarget.x * xTarget.x + rTarget.y * rTarget.y + rTarget.z * rTarget.z));
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
