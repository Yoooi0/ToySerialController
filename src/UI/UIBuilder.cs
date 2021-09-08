using CurveEditor;
using CurveEditor.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ToySerialController.UI
{
    public class UIBuilder : IUIBuilder
    {
        public JSONStorableStringChooser CreatePopup(string paramName, string label, List<string> values, string startingValue, JSONStorableStringChooser.SetStringCallback callback, bool scrollable, bool rightSide = false)
        {
            var storable = new JSONStorableStringChooser(paramName, values, startingValue, label, callback);
            if (!scrollable)
            {
                var popup = UIManager.CreatePopup(storable, rightSide);
                popup.labelWidth = 300;
            }
            else
            {
                var popup = UIManager.CreateScrollablePopup(storable, rightSide);
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
            var button = UIManager.CreateButton(label, rightSide);
            button.button.onClick.AddListener(callback);
            return button;
        }

        public UIDynamicButton CreateButton(string label, UnityAction callback, Color buttonColor, Color textColor, bool rightSide = false)
        {
            var button = UIManager.CreateButton(label, rightSide);
            button.button.onClick.AddListener(callback);
            button.buttonColor = buttonColor;
            button.textColor = textColor;
            return button;
        }

        public UIDynamicButton CreateDisabledButton(string label, Color buttonColor, Color textColor, bool rightSide = false)
        {
            var button = UIManager.CreateButton(label, rightSide);
            button.buttonColor = Color.white;
            button.textColor = textColor;

            button.button.interactable = false;
            var colors = button.button.colors;
            colors.disabledColor = buttonColor;
            button.button.colors = colors;

            return button;
        }

        public JSONStorableFloat CreateSlider(string paramName, string label, float startingValue, float minValue, float maxValue, JSONStorableFloat.SetFloatCallback callback, bool constrain, bool interactable, bool rightSide = false, string valueFormat = "F2")
        {
            var storable = new JSONStorableFloat(paramName, startingValue, callback, minValue, maxValue, constrain, interactable);
            var slider = UIManager.CreateSlider(storable, rightSide);
            slider.label = label;
            slider.valueFormat = valueFormat;
            return storable;
        }

        public JSONStorableFloat CreateSlider(string paramName, string label, float startingValue, float minValue, float maxValue, bool constrain, bool interactable, bool rightSide = false,string valueFormat = "F2")
            => CreateSlider(paramName, label, startingValue, minValue, maxValue, null, constrain, interactable, rightSide);

        public JSONStorableString CreateTextField(string paramName, string startingValue, float height, JSONStorableString.SetStringCallback callback, bool rightSide = false)
        {
            var storable = new JSONStorableString(paramName, startingValue, callback);
            var textField = UIManager.CreateTextField(storable, rightSide);
            var layoutElement = textField.gameObject.GetComponent<LayoutElement>();
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;

            return storable;
        }

        public JSONStorableString CreateTextField(string paramName, string startingValue, float height, bool rightSide = false)
            => CreateTextField(paramName, startingValue, height, null, rightSide);

        public UITextInput CreateTextInput(string paramName, string label, string startingValue, float height, bool rightSide = false)
        {
            var container = CreateSpacer(height, rightSide);
            return new UITextInput(container, height, label, paramName, startingValue);
        }

        public JSONStorableBool CreateToggle(string paramName, string label, bool startingValue, JSONStorableBool.SetBoolCallback callback, bool rightSide = false)
        {
            var storable = new JSONStorableBool(paramName, startingValue, callback);
            var toggle = UIManager.CreateToggle(storable, rightSide);
            toggle.label = label;
            return storable;
        }

        public JSONStorableBool CreateToggle(string paramName, string label, bool startingValue, bool rightSide = false)
            => CreateToggle(paramName, label, startingValue, null, rightSide);

        public UIDynamic CreateSpacer(float height, bool rightSide = false)
        {
            var spacer = UIManager.CreateSpacer(rightSide);
            spacer.height = height;
            return spacer;
        }

        public UICurveEditor CreateCurveEditor(float height, bool rightSide = false)
        {
            var container = CreateSpacer(height, rightSide);

            UICurveEditor curveEditor = null;
            var buttons = Enumerable.Range(0, 4)
                .Select(_ => UnityEngine.Object.Instantiate(UIManager.ConfigurableButtonPrefab))
                .Select(t => t.GetComponent<UIDynamicButton>())
                .ToList();

            foreach (var b in buttons)
            {
                b.buttonText.fontSize = 18;
                b.buttonColor = Color.white;
            }

            buttons[0].label = "Mode";
            buttons[1].label = "In Mode";
            buttons[2].label = "Out Mode";
            buttons[3].label = "Linear";

            buttons[0].button.onClick.AddListener(() => curveEditor.ToggleHandleMode());
            buttons[1].button.onClick.AddListener(() => curveEditor.ToggleInHandleMode());
            buttons[2].button.onClick.AddListener(() => curveEditor.ToggleOutHandleMode());
            buttons[3].button.onClick.AddListener(() => curveEditor.SetLinear());

            curveEditor = new UICurveEditor(container, 510, height, buttons: buttons);
            curveEditor.settings.allowViewDragging = false;
            curveEditor.settings.allowViewScaling = false;
            curveEditor.settings.allowViewZooming = false;
            curveEditor.settings.showScrubbers = true;
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

        public Transform CreateButtonEx() => GameObject.Instantiate<Transform>(UIManager.ConfigurableButtonPrefab);
        public Transform CreateSliderEx() => GameObject.Instantiate<Transform>(UIManager.ConfigurableSliderPrefab);
        public Transform CreateToggleEx() => GameObject.Instantiate<Transform>(UIManager.ConfigurableTogglePrefab);
        public Transform CreatePopupEx() => GameObject.Instantiate<Transform>(UIManager.ConfigurablePopupPrefab);

        public void Destroy(object o)
        {
            if (o is JSONStorableStringChooser) UIManager.RemovePopup((JSONStorableStringChooser)o);
            else if (o is JSONStorableFloat) UIManager.RemoveSlider((JSONStorableFloat)o);
            else if (o is JSONStorableBool) UIManager.RemoveToggle((JSONStorableBool)o);
            else if (o is JSONStorableString) UIManager.RemoveTextField((JSONStorableString)o);
            else if (o is UIDynamicButton) UIManager.RemoveButton((UIDynamicButton)o);
            else if (o is UICurveEditor) UIManager.RemoveSpacer(((UICurveEditor)o).container);
            else if (o is UIHorizontalGroup) UIManager.RemoveSpacer(((UIHorizontalGroup)o).container);
            else if (o is UITextInput) UIManager.RemoveSpacer(((UITextInput)o).container);
            else if (o is UIDynamic) UIManager.RemoveSpacer((UIDynamic)o);
        }
    }
}
