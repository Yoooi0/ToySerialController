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
        private JSONStorableFloat ActivationDistanceSlider;

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

            var visible = false;
            var mainGroup = new UIGroup(_group);

            MainTitle = _group.CreateButton("Main", () => mainGroup.SetVisible(visible = !visible), new Color(0.3f, 0.3f, 0.3f), Color.white, true);

            LimitsButtonGroup = mainGroup.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => mainGroup.CreateButtonEx(), true);
            var resetButton = LimitsButtonGroup.items[0].GetComponent<UIDynamicButton>();
            resetButton.label = "Reset Limits";
            resetButton.button.onClick.AddListener(PositionResetButtonCallback);
            
            var applyButton = LimitsButtonGroup.items[1].GetComponent<UIDynamicButton>();
            applyButton.label = "Apply Limits";
            applyButton.button.onClick.AddListener(PositionApplyButtonCallback);

            SmoothingSlider = mainGroup.CreateSlider("Plugin:Smoothing", "Smoothing", 0.1f, 0.0f, 0.99f, true, true, true);
            ActivationDistanceSlider = mainGroup.CreateSlider("Device:ActivationDistance", "Activation Distance", 1.1f, 0, 10, true, true, true);

            CreateCustomUI(mainGroup);
            mainGroup.SetVisible(false);

            CreateXAxisUI(_group);
            CreateYAxisUI(_group);
            CreateZAxisUI(_group);
            CreateRXAxisUI(_group);
            CreateRYAxisUI(_group);
            CreateRZAxisUI(_group);
            CreateVibe0UI(_group);
            CreateVibe1UI(_group);
        }

        public virtual void CreateCustomUI(UIGroup group) { }
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
            var xGroup = new UIGroup(builder);
            var visible = false;
            XAxisTitle = builder.CreateButton("X Axis", () => xGroup.SetVisible(visible = !visible), Color.red * 0.8f, Color.white, true);
            RangeMaxXSlider = xGroup.CreateSlider("Device:RangeMaxX", "Range Max", 1f, 0f, 1f, v => RangeMinXSlider.max = v, true, true, true);
            RangeMinXSlider = xGroup.CreateSlider("Device:RangeMinX", "Range Min", 0f, 0f, 1f, v => RangeMaxXSlider.min = v, true, true, true);
            OutputMaxXSlider = xGroup.CreateSlider("Device:OutputMaxX", "Output Max", 1f, 0f, 1f, v => OutputMinXSlider.max = v, true, true, true);
            OutputMinXSlider = xGroup.CreateSlider("Device:OutputMinX", "Output Min", 0, 0f, 1f, v => OutputMaxXSlider.min = v, true, true, true);
            InvertXToggle = xGroup.CreateToggle("Device:InvertX", "Invert", true, true);
            EnableOverrideXToggle = xGroup.CreateToggle("Device:EnableOverrideX", "Enable Override", false, true);
            OverrideXSlider = xGroup.CreateSlider("Device:OverrideX", "Override Value", 0.5f, 0f, 1f, true, true, true);
            ProjectXChooser = xGroup.CreateScrollablePopup("Device:ProjectX", "Select Projection Axis", new List<string> { "Difference", "Reference Up", "Target Up" }, "Difference", null, true);

            xGroup.SetVisible(false);
        }

        private void CreateYAxisUI(IUIBuilder builder)
        {
            var yGroup = new UIGroup(builder);
            var visible = false;
            YAxisTitle = builder.CreateButton("Y Axis", () => yGroup.SetVisible(visible = !visible), Color.green * 0.8f, Color.white, true);
            RangeMaxYSlider = yGroup.CreateSlider("Device:RangeMaxY", "Range Max (+/-)", 1f, 0, 2f, true, true, true);
            OutputMaxYSlider = yGroup.CreateSlider("Device:OutputMaxY", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            AdjustYSlider = yGroup.CreateSlider("Device:AdjustY", "Adjust", 0f, -0.25f, 0.25f, true, true, true);
            InvertYToggle = yGroup.CreateToggle("Device:InvertY", "Invert", false, true);
            EnableOverrideYToggle = yGroup.CreateToggle("Device:EnableOverrideY", "Enable Override", false, true);
            OverrideYSlider = yGroup.CreateSlider("Device:OverrideY", "Override Value", 0.5f, 0f, 1f, true, true, true);

            yGroup.SetVisible(false);
        }

        private void CreateZAxisUI(IUIBuilder builder)
        {
            var zGroup = new UIGroup(builder);
            var visible = false;
            ZAxisTitle = builder.CreateButton("Z Axis", () => zGroup.SetVisible(visible = !visible), Color.blue * 0.8f, Color.white, true);
            RangeMaxZSlider = zGroup.CreateSlider("Device:RangeMaxZ", "Range Max (+/-)", 1f, 0, 2f, true, true, true);
            OutputMaxZSlider = zGroup.CreateSlider("Device:OutputMaxZ", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            AdjustZSlider = zGroup.CreateSlider("Device:AdjustZ", "Adjust", 0f, -0.25f, 0.25f, true, true, true);
            InvertZToggle = zGroup.CreateToggle("Device:InvertZ", "Invert", false, true);
            EnableOverrideZToggle = zGroup.CreateToggle("Device:EnableOverrideZ", "Enable Override", false, true);
            OverrideZSlider = zGroup.CreateSlider("Device:OverrideZ", "Override Value", 0.5f, 0f, 1f, true, true, true);

            zGroup.SetVisible(false);
        }

        private void CreateRXAxisUI(IUIBuilder builder)
        {
            var rxGroup = new UIGroup(builder);
            var visible = false;
            RXAxisTitle = builder.CreateButton("RX Axis", () =>
            {
                rxGroup.SetVisible(visible = !visible);
                OutputRXCurveEditorSettings.SetVisible(visible);
            }, Color.cyan * 0.8f, Color.white, true);
            OutputRXCurveEditor = rxGroup.CreateCurveEditor(300, true);
            OutputRXCurve = rxGroup.CreateCurve("Device:OutputRXCurve", OutputRXCurveEditor, new List<Keyframe> { new Keyframe(0, 0, 0, 0.5f), new Keyframe(1, 0.5f, 0.5f, 0) });
            OutputRXCurveEditor.SetDrawScale(OutputRXCurve, Vector2.one, new Vector2(0, 0.5f), true);

            OutputRXCurveEditorSettings = new AbstractGenericDeviceCurveSettings("OutputRXCurveSettings", OutputRXCurveEditor, OutputRXCurve, new Vector2(0, 0.5f));
            OutputRXCurveEditorSettings.CreateUI(rxGroup);

            OutputMaxRXSlider = rxGroup.CreateSlider("Device:OutputMaxRX", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            InvertRXToggle = rxGroup.CreateToggle("Device:InvertRX", "Invert", false, true);
            EnableOverrideRXToggle = rxGroup.CreateToggle("Device:EnableOverrideRX", "Enable Override", true, true);
            OverrideRXSlider = rxGroup.CreateSlider("Device:OverrideRX", "Override Value", 0.5f, 0f, 1f, true, true, true);

            rxGroup.SetVisible(false);
        }

        private void CreateRYAxisUI(IUIBuilder builder)
        {
            var ryGroup = new UIGroup(builder);
            var visible = false;
            RYAxisTitle = builder.CreateButton("RY Axis", () => ryGroup.SetVisible(visible = !visible), Color.magenta * 0.8f, Color.white, true);
            RangeMaxRYSlider = ryGroup.CreateSlider("Device:RangeMaxRY", "Range Max (+/-)", 0.4f, 0f, 1f, true, true, true);
            OutputMaxRYSlider = ryGroup.CreateSlider("Device:OutputMaxRY", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            AdjustRYSlider = ryGroup.CreateSlider("Device:AdjustRY", "Adjust", 0f, -0.25f, 0.25f, true, true, true);
            InvertRYToggle = ryGroup.CreateToggle("Device:InvertRY", "Invert", false, true);
            EnableOverrideRYToggle = ryGroup.CreateToggle("Device:EnableOverrideRY", "Enable Override", false, true);
            OverrideRYSlider = ryGroup.CreateSlider("Device:OverrideRY", "Override Value", 0.5f, 0f, 1f, true, true, true);

            ryGroup.SetVisible(false);
        }

        private void CreateRZAxisUI(IUIBuilder builder)
        {
            var rzGroup = new UIGroup(builder);
            var visible = false;
            RZAxisTitle = builder.CreateButton("RZ Axis", () => rzGroup.SetVisible(visible = !visible), Color.yellow * 0.8f, Color.white, true);
            RangeMaxRZSlider = rzGroup.CreateSlider("Device:RangeMaxRZ", "Range Max (+/-)", 0.4f, 0f, 1f, true, true, true);
            OutputMaxRZSlider = rzGroup.CreateSlider("Device:OutputMaxRZ", "Output Max (+/-)", 0.5f, 0f, 0.5f, true, true, true);
            AdjustRZSlider = rzGroup.CreateSlider("Device:AdjustRZ", "Adjust", 0f, -0.25f, 0.25f, true, true, true);
            InvertRZToggle = rzGroup.CreateToggle("Device:InvertRZ", "Invert", false, true);
            EnableOverrideRZToggle = rzGroup.CreateToggle("Device:EnableOverrideRZ", "Enable Override", false, true);
            OverrideRZSlider = rzGroup.CreateSlider("Device:OverrideRZ", "Override Value", 0.5f, 0f, 1f, true, true, true);

            rzGroup.SetVisible(false);
        }

        private void CreateVibe0UI(IUIBuilder builder)
        {
            var v0Group = new UIGroup(builder);
            var visible = false;
            Vibe0Title = builder.CreateButton("Vibe 0", () => 
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
            OverrideV0Slider = v0Group.CreateSlider("Device:OverrideV0", "Override Value", 0f, 0f, 1f, true, true, true);

            v0Group.SetVisible(false);
        }

        private void CreateVibe1UI(IUIBuilder builder)
        {
            var v1Group = new UIGroup(builder);
            var visible = false;
            Vibe1Title = builder.CreateButton("Vibe 1", () =>
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
            OverrideV1Slider = v1Group.CreateSlider("Device:OverrideV1", "Override Value", 0f, 0f, 1f, true, true, true);

            v1Group.SetVisible(false);
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
            CurveXAxisChooser = builder.CreateScrollablePopup($"Device:{_name}:CurveXAxis", "Curve X Axis", new List<string> { "X", "Y", "Z", "Y+Z", "RX", "RY", "RZ", "RY+RZ", "X+RY+RZ", "Time" }, "X", CurveXAxisChooserCallback, true);
            CreateTimeUI(builder);

            CurveXAxisChooserCallback("X");
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
            if (CurveXAxisChooser.val == "X") t = Mathf.Clamp01(xTarget.x);
            else if (CurveXAxisChooser.val == "Y") t = xTarget.y;
            else if (CurveXAxisChooser.val == "Z") t = xTarget.z;
            else if (CurveXAxisChooser.val == "Y+Z") t = Mathf.Clamp01(Mathf.Sqrt(xTarget.y * xTarget.y + xTarget.z * xTarget.z));
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
