using System.Collections.Generic;
using UnityEngine;

namespace ToySerialController.UI
{
    public class UIManager
    {
        private static UIManager Instance;
        private MVRScript plugin;
        private Dictionary<JSONStorableFloat, UIDynamicSlider> jsonStorableFloatToSlider;

        public static Transform ConfigurableTextFieldPrefab => Instance.plugin.manager.configurableTextFieldPrefab;
        public static Transform ConfigurablePopupPrefab => Instance.plugin.manager.configurablePopupPrefab;
        public static Transform ConfigurableTogglePrefab => Instance.plugin.manager.configurableTogglePrefab;
        public static Transform ConfigurableSliderPrefab => Instance.plugin.manager.configurableSliderPrefab;
        public static Transform ConfigurableButtonPrefab => Instance.plugin.manager.configurableButtonPrefab;

        private UIManager(MVRScript plugin)
        {
            this.plugin = plugin;

            jsonStorableFloatToSlider = new Dictionary<JSONStorableFloat, UIDynamicSlider>();
        }

        public static void Initialize(MVRScript plugin)
        {
            if (Instance != null)
                return;
            
            Instance = new UIManager(plugin);
        }

        public static void RemoveSpacer(UIDynamic o) => Instance.plugin.RemoveSpacer(o);
        public static void RemoveButton(UIDynamicButton o) => Instance.plugin.RemoveButton(o);

        public static void RemoveToggle(JSONStorableBool storable)
        {
            Instance.plugin.DeregisterBool(storable);
            Instance.plugin.RemoveToggle(storable);
        }

        public static void RemoveTextField(JSONStorableString storable)
        {
            Instance.plugin.DeregisterString(storable);
            Instance.plugin.RemoveTextField(storable);
        }

        public static void RemoveSlider(JSONStorableFloat storable)
        {
            var dynamicSlider = Instance.jsonStorableFloatToSlider[storable];
            Instance.jsonStorableFloatToSlider.Remove(storable);

            Instance.plugin.DeregisterFloat(storable);
            Instance.plugin.RemoveSlider(dynamicSlider);
        }

        public static void RemovePopup(JSONStorableStringChooser storable)
        {
            Instance.plugin.DeregisterStringChooser(storable);
            Instance.plugin.RemovePopup(storable);
        }

        public static UIDynamic CreateSpacer(bool rightSide) => Instance.plugin.CreateSpacer(rightSide);
        public static UIDynamicButton CreateButton(string label, bool rightSide) => Instance.plugin.CreateButton(label, rightSide);

        public static UIDynamicToggle CreateToggle(JSONStorableBool storable, bool rightSide)
        {
            Instance.plugin.RegisterBool(storable);
            return Instance.plugin.CreateToggle(storable, rightSide);
        }

        public static UIDynamicTextField CreateTextField(JSONStorableString storable, bool rightSide)
        {
            Instance.plugin.RegisterString(storable);
            return Instance.plugin.CreateTextField(storable, rightSide);
        }

        public static UIDynamicSlider CreateSlider(JSONStorableFloat storable, bool rightSide)
        {
            Instance.plugin.RegisterFloat(storable);
            var dynamicSlider = Instance.plugin.CreateSlider(storable, rightSide);

            Instance.jsonStorableFloatToSlider.Add(storable, dynamicSlider);
            return dynamicSlider;
        }

        public static UIDynamicPopup CreateScrollablePopup(JSONStorableStringChooser storable, bool rightSide)
        {
            Instance.plugin.RegisterStringChooser(storable);
            return Instance.plugin.CreateScrollablePopup(storable, rightSide);
        }

        public static UIDynamicPopup CreatePopup(JSONStorableStringChooser storable, bool rightSide)
        {
            Instance.plugin.RegisterStringChooser(storable);
            return Instance.plugin.CreatePopup(storable, rightSide);
        }
    }
}
