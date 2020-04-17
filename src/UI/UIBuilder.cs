using CurveEditor;
using CurveEditor.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ToySerialController.UI
{
    public class UIBuilder : IUIBuilder
    {
        protected MVRScript Plugin { get; private set; }

        public UIBuilder(MVRScript plugin)
        {
            Plugin = plugin;
        }

        public JSONStorableStringChooser CreatePopup(string paramName, string label, List<string> values, string startingValue, JSONStorableStringChooser.SetStringCallback callback, bool scrollable, bool rightSide = false)
        {
            var storable = new JSONStorableStringChooser(paramName, values, startingValue, label, callback);
            if (!scrollable)
            {
                var popup = Plugin.CreatePopup(storable, rightSide);
                popup.labelWidth = 300;
            }
            else
            {
                var popup = Plugin.CreateScrollablePopup(storable, rightSide);
                popup.labelWidth = 300;
            }

            return storable;
        }

        public JSONStorableStringChooser CreatePopup(string paramName, string label, List<string> values, string startingValue, JSONStorableStringChooser.SetStringCallback callback, bool rightSide = false)
            => CreatePopup(paramName, label, values, startingValue, callback, false, rightSide);

        public JSONStorableStringChooser CreateScrollablePopup(string paramName, string label, List<string> values, string startingValue, JSONStorableStringChooser.SetStringCallback callback, bool rightSide = false)
            => CreatePopup(paramName, label, values, startingValue, callback, true, rightSide);

        public UIDynamicButton CreateButton(string label, UnityAction callback, bool rightSide = false)
        {
            var button = Plugin.CreateButton(label, rightSide);
            button.button.onClick.AddListener(callback);
            return button;
        }

        public UIDynamicButton CreateButton(string label, UnityAction callback, Color buttonColor, Color textColor, bool rightSide = false)
        {
            var button = Plugin.CreateButton(label, rightSide);
            button.button.onClick.AddListener(callback);
            button.buttonColor = buttonColor;
            button.textColor = textColor;
            return button;
        }

        public UIDynamicButton CreateDisabledButton(string label, Color buttonColor, Color textColor, bool rightSide = false)
        {
            var button = Plugin.CreateButton(label, rightSide);
            button.buttonColor = Color.white;
            button.textColor = textColor;

            button.button.interactable = false;
            var colors = button.button.colors;
            colors.disabledColor = buttonColor;
            button.button.colors = colors;

            return button;
        }

        public JSONStorableFloat CreateSlider(string paramName, string label, float startingValue, float minValue, float maxValue, JSONStorableFloat.SetFloatCallback callback, bool constrain, bool interactable, bool rightSide = false)
        {
            var storable = new JSONStorableFloat(paramName, startingValue, callback, minValue, maxValue, constrain, interactable);
            Plugin.RegisterFloat(storable);
            var slider = Plugin.CreateSlider(storable, rightSide);
            slider.label = label;
            return storable;
        }

        public JSONStorableFloat CreateSlider(string paramName, string label, float startingValue, float minValue, float maxValue, bool constrain, bool interactable, bool rightSide = false)
            => CreateSlider(paramName, label, startingValue, minValue, maxValue, null, constrain, interactable, rightSide);

        public JSONStorableString CreateTextField(string paramName, string startingValue, float height, JSONStorableString.SetStringCallback callback, bool rightSide = false)
        {
            var storable = new JSONStorableString(paramName, startingValue, callback);
            var textField = Plugin.CreateTextField(storable, rightSide);
            textField.height = height;
            return storable;
        }

        public JSONStorableString CreateTextField(string paramName, string startingValue, float height, bool rightSide = false)
            => CreateTextField(paramName, startingValue, height, null, rightSide);


        public JSONStorableBool CreateToggle(string paramName, string label, bool startingValue, JSONStorableBool.SetBoolCallback callback, bool rightSide = false)
        {
            var storable = new JSONStorableBool(paramName, startingValue, callback);
            var toggle = Plugin.CreateToggle(storable, rightSide);
            toggle.label = label;
            return storable;
        }

        public JSONStorableBool CreateToggle(string paramName, string label, bool startingValue, bool rightSide = false)
            => CreateToggle(paramName, label, startingValue, null, rightSide);

        public UIDynamic CreateSpacer(float height, bool rightSide = false)
        {
            var spacer = Plugin.CreateSpacer(rightSide);
            spacer.height = height;
            return spacer;
        }

        public UICurveEditor CreateCurveEditor(float height, bool rightSide = false)
        {
            var container = CreateSpacer(height, rightSide);

            UICurveEditor curveEditor = null;
            var curveEditorButtons = Enumerable.Range(0, 4)
                .Select(_ => UnityEngine.Object.Instantiate(Plugin.manager.configurableButtonPrefab))
                .Select(t => t.GetComponent<UIDynamicButton>())
                .ToList();

            foreach (var b in curveEditorButtons)
            {
                b.buttonText.fontSize = 18;
                b.buttonColor = Color.white;
            }

            curveEditorButtons[0].label = "Mode";
            curveEditorButtons[1].label = "In Mode";
            curveEditorButtons[2].label = "Out Mode";
            curveEditorButtons[3].label = "Linear";

            curveEditorButtons[0].button.onClick.AddListener(() => curveEditor.ToggleHandleMode());
            curveEditorButtons[1].button.onClick.AddListener(() => curveEditor.ToggleInHandleMode());
            curveEditorButtons[2].button.onClick.AddListener(() => curveEditor.ToggleOutHandleMode());
            curveEditorButtons[3].button.onClick.AddListener(() => curveEditor.SetLinear());

            curveEditor = new UICurveEditor(container, 510, height);
            return curveEditor;
        }

        public JSONStorableAnimationCurve CreateCurve(string paramName, UICurveEditor curveEditor, IEnumerable<Keyframe> keyframes = null)
        {
            var storable = new JSONStorableAnimationCurve(paramName);
            if (keyframes != null)
            {
                storable.val = new AnimationCurve(keyframes.ToArray());
                storable.SetDefaultFromCurrent();
            }

            curveEditor.AddCurve(storable);
            return storable;
        }

        public UIHorizontalGroup CreateHorizontalGroup(float width, float height, Vector2 spacing, int count, Func<int, Transform> itemCreator, bool rightSide = false)
        {
            var container = CreateSpacer(height, rightSide);
            return new UIHorizontalGroup(container, Mathf.Clamp(width, 0, 510), height, spacing, count, itemCreator);
        }

        public Transform CreateButtonEx() => GameObject.Instantiate<Transform>(Plugin.manager.configurableButtonPrefab);
        public Transform CreateSliderEx() => GameObject.Instantiate<Transform>(Plugin.manager.configurableSliderPrefab);
        public Transform CreateToggleEx() => GameObject.Instantiate<Transform>(Plugin.manager.configurableTogglePrefab);
        public Transform CreatePopupEx() => GameObject.Instantiate<Transform>(Plugin.manager.configurablePopupPrefab);

        public void Destroy(object o)
        {
            if (o is JSONStorableStringChooser) Plugin.RemovePopup((JSONStorableStringChooser)o);
            else if (o is JSONStorableFloat) Plugin.RemoveSlider((JSONStorableFloat)o);
            else if (o is JSONStorableBool) Plugin.RemoveToggle((JSONStorableBool)o);
            else if (o is JSONStorableString) Plugin.RemoveTextField((JSONStorableString)o);
            else if (o is UIDynamicButton) Plugin.RemoveButton((UIDynamicButton)o);
            else if (o is UICurveEditor) Plugin.RemoveSpacer(((UICurveEditor)o).container);
            else if (o is UIHorizontalGroup) Plugin.RemoveSpacer(((UIHorizontalGroup)o).container);
            else if (o is UIDynamic) Plugin.RemoveSpacer((UIDynamic)o);
        }
    }
}
