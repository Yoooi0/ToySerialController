using SimpleJSON;
using System;
using System.Collections.Generic;
using ToySerialController.UI;
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
        private UICurveEditor OutputRXCurve;
        private JSONStorableStringChooser OutputRXCurveXAxisChooser;

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
        private UICurveEditor OutputV0Curve;
        private JSONStorableStringChooser OutputV0CurveXAxisChooser;
        private JSONStorableFloat OverrideV0Slider;
        private JSONStorableBool EnableOverrideV0Toggle;

        private UIDynamicButton Vibe1Title;
        private UICurveEditor OutputV1Curve;
        private JSONStorableStringChooser OutputV1CurveXAxisChooser;
        private JSONStorableFloat OverrideV1Slider;
        private JSONStorableBool EnableOverrideV1Toggle;

        private UIGroup _group;

        public virtual void CreateUI(UIBuilder builder)
        {
            _group = new UIGroup(builder);
            LimitsButtonGroup = _group.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => _group.CreateButtonEx(), true);
            var resetButton = LimitsButtonGroup.items[0].GetComponent<UIDynamicButton>();
            resetButton.label = "Reset Limits";
            resetButton.button.onClick.AddListener(PositionResetButtonCallback);
            
            var applyButton = LimitsButtonGroup.items[1].GetComponent<UIDynamicButton>();
            applyButton.label = "Apply Limits";
            applyButton.button.onClick.AddListener(PositionApplyButtonCallback);

            SmoothingSlider = _group.CreateSlider("Plugin:Smoothing", "Smoothing", 0.2f, 0f, 0.95f, true, true, true);

            CreateXAxisUI(_group);
            CreateYAxisUI(_group);
            CreateZAxisUI(_group);
            CreateRXAxisUI(_group);
            CreateRYAxisUI(_group);
            CreateRZAxisUI(_group);
            CreateVibe0UI(_group);
            CreateVibe1UI(_group);
        }

        public virtual void DestroyUI(UIBuilder builder) => _group.Destroy();
        public virtual void StoreConfig(JSONNode config) => _group.StoreConfig(config);
        public virtual void RestoreConfig(JSONNode config) => _group.RestoreConfig(config);

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
            OutputRXCurve = builder.CreateCurveEditor("Device:OutputRXCurve", 300, true);
            OutputRXCurve.SetPointsFromKeyframes(new List<Keyframe> { new Keyframe(0, 0.5f, 0, 0.5f), new Keyframe(1, 1, 0.5f, 0) });
            OutputRXCurve.SetDefaultFromCurrent();

            OutputRXCurveXAxisChooser = builder.CreateScrollablePopup("Device:OutputRXCurveXAxis", "Curve X Axis", new List<string> { "X", "RY", "RZ", "RY+RZ", "X+RY+RZ" }, "X", null, true);
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
            OutputV0Curve = builder.CreateCurveEditor("Device:OutputV0Curve", 300, true);
            OutputV0Curve.SetPointsFromKeyframes(new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputV0Curve.SetDefaultFromCurrent();

            OutputV0CurveXAxisChooser = builder.CreateScrollablePopup("Device:OutputV0CurveXAxis", "Curve X Axis", new List<string> { "X", "RY", "RZ", "RY+RZ", "X+RY+RZ" }, "X", null, true);
            EnableOverrideV0Toggle = builder.CreateToggle("Device:EnableOverrideV0", "Enable Override", true, true);
            OverrideV0Slider = builder.CreateSlider("Device:OverrideV0", "Override Value", 0f, 0f, 1f, true, true, true);
        }

        private void CreateVibe1UI(IUIBuilder builder)
        {
            Vibe1Title = builder.CreateDisabledButton("Vibe 1", new Color(0.4f, 0.4f, 0.4f), Color.white, true);
            OutputV1Curve = builder.CreateCurveEditor("Device:OutputV1Curve", 300, true);
            OutputV1Curve.SetPointsFromKeyframes(new List<Keyframe> { new Keyframe(0, 0, 0, 1), new Keyframe(1, 1, 1, 0) });
            OutputV1Curve.SetDefaultFromCurrent();

            OutputV1CurveXAxisChooser = builder.CreateScrollablePopup("Device:OutputV1CurveXAxis", "Curve X Axis", new List<string> { "X", "RY", "RZ", "RY+RZ", "X+RY+RZ" }, "X", null, true);
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
}
