using CurveEditor;
using CurveEditor.UI;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace ToySerialController.UI
{
    public class UIGroup : IUIBuilder
    {
        private readonly IUIBuilder _builder;
        private readonly List<object> _objects;
        private readonly List<string> _storableBlacklist;

        public UIGroup(IUIBuilder builder)
        {
            _builder = builder;
            _objects = new List<object>();
            _storableBlacklist = new List<string>();
        }

        public JSONStorableStringChooser CreatePopup(string paramName, string label, List<string> values, string startingValue, JSONStorableStringChooser.SetStringCallback callback, bool scrollable, bool rightSide = false)
        {
            var storable = _builder.CreatePopup(paramName, label, values, startingValue, callback, scrollable, rightSide);
            _objects.Add(storable);
            return storable;
        }

        public JSONStorableStringChooser CreatePopup(string paramName, string label, List<string> values, string startingValue, JSONStorableStringChooser.SetStringCallback callback, bool rightSide = false)
            => CreatePopup(paramName, label, values, startingValue, callback, false, rightSide);

        public JSONStorableStringChooser CreateScrollablePopup(string paramName, string label, List<string> values, string startingValue, JSONStorableStringChooser.SetStringCallback callback, bool rightSide = false)
            => CreatePopup(paramName, label, values, startingValue, callback, true, rightSide);

        public UIDynamicButton CreateButton(string label, UnityAction callback, bool rightSide = false)
        {
            var o = _builder.CreateButton(label, callback, rightSide);
            _objects.Add(o);
            return o;
        }

        public UIDynamicButton CreateButton(string label, UnityAction callback, Color buttonColor, Color textColor, bool rightSide = false)
        {
            var o = _builder.CreateButton(label, callback, buttonColor, textColor, rightSide);
            _objects.Add(o);
            return o;
        }

        public UIDynamicButton CreateDisabledButton(string label, Color buttonColor, Color textColor, bool rightSide = false)
        {
            var o = _builder.CreateDisabledButton(label,  buttonColor, textColor, rightSide);
            _objects.Add(o);
            return o;
        }

        public JSONStorableFloat CreateSlider(string paramName, string label, float startingValue, float minValue, float maxValue, JSONStorableFloat.SetFloatCallback callback, bool constrain, bool interactable, bool rightSide = false, string valueFormat = "F2")
        {
            var storable = _builder.CreateSlider(paramName, label, startingValue, minValue, maxValue, callback, constrain, interactable, rightSide, valueFormat);
            _objects.Add(storable);
            return storable;
        }

        public JSONStorableFloat CreateSlider(string paramName, string label, float startingValue, float minValue, float maxValue, bool constrain, bool interactable, bool rightSide = false, string valueFormat = "F2")
            => CreateSlider(paramName, label, startingValue, minValue, maxValue, null, constrain, interactable, rightSide, valueFormat);

        public JSONStorableString CreateTextField(string paramName, string startingValue, float height, JSONStorableString.SetStringCallback callback, bool rightSide = false)
        {
            var storable = _builder.CreateTextField(paramName, startingValue, height, callback, rightSide);
            _objects.Add(storable);
            return storable;
        }

        public JSONStorableString CreateTextField(string paramName, string startingValue, float height, bool rightSide = false)
            => CreateTextField(paramName, startingValue, height, null, rightSide);


        public JSONStorableBool CreateToggle(string paramName, string label, bool startingValue, JSONStorableBool.SetBoolCallback callback, bool rightSide = false)
        {
            var storable = _builder.CreateToggle(paramName, label, startingValue, callback, rightSide);
            _objects.Add(storable);
            return storable;
        }

        public JSONStorableBool CreateToggle(string paramName, string label, bool startingValue, bool rightSide = false)
            => CreateToggle(paramName, label, startingValue, null, rightSide);

        public UIDynamic CreateSpacer(float height, bool rightSide = false)
        {
            var o = _builder.CreateSpacer(height, rightSide);
            _objects.Add(o);
            return o;
        }

        public UICurveEditor CreateCurveEditor(float height, bool rightSide = false)
        {
            var o = _builder.CreateCurveEditor(height, rightSide);
            _objects.Add(o);
            return o;
        }

        public JSONStorableAnimationCurve CreateCurve(string paramName, UICurveEditor curveEditor, IEnumerable<Keyframe> keyframes = null)
        {
            var storable = _builder.CreateCurve(paramName, curveEditor, keyframes);
            _objects.Add(storable);
            return storable;
        }

        public UIHorizontalGroup CreateHorizontalGroup(float width, float height, Vector2 spacing, int count, Func<int, Transform> itemCreator, bool rightSide = false)
        {
            var o = _builder.CreateHorizontalGroup(width, height, spacing, count, itemCreator, rightSide);
            _objects.Add(o);
            return o;
        }

        public Transform CreateButtonEx() => _builder.CreateButtonEx();
        public Transform CreateSliderEx() => _builder.CreateSliderEx();
        public Transform CreateToggleEx() => _builder.CreateToggleEx();
        public Transform CreatePopupEx() => _builder.CreatePopupEx();

        public void Destroy(object o) => _builder.Destroy(o);
        public void Destroy()
        {
            foreach (var o in _objects)
                Destroy(o);

            _objects.Clear();
        }

        public void BlacklistStorable(string name) => _storableBlacklist.Add(name);

        public void SetVisible(bool visible)
        {
            foreach (var o in _objects)
            {
                if (o is JSONStorableStringChooser) ((JSONStorableStringChooser)o).popup.transform.gameObject.SetActive(visible);
                else if (o is JSONStorableFloat) ((JSONStorableFloat)o).slider.transform.parent.gameObject.SetActive(visible);
                else if (o is JSONStorableBool) ((JSONStorableBool)o).toggle.transform.gameObject.SetActive(visible);
                else if (o is JSONStorableString) ((JSONStorableString)o).dynamicText.transform.gameObject.SetActive(visible);
                else if (o is UIDynamicButton) ((UIDynamicButton)o).button.transform.gameObject.SetActive(visible);
                else if (o is UICurveEditor) ((UICurveEditor)o).container.transform.gameObject.SetActive(visible);
                else if (o is UIHorizontalGroup) ((UIHorizontalGroup)o).container.transform.gameObject.SetActive(visible);
                else if (o is UIDynamic) ((UIDynamic)o).transform.gameObject.SetActive(visible);
            }
        }

        public void StoreConfig(JSONNode config)
        {
            foreach (var s in _objects.OfType<JSONStorableParam>())
                if (!_storableBlacklist.Contains(s.name))
                    config.Store(s);
        }

        public void RestoreConfig(JSONNode config)
        {
            foreach (var s in _objects.OfType<JSONStorableParam>().ToList())
                if (!_storableBlacklist.Contains(s.name))
                    config.Restore(s);
        }
    }
}
