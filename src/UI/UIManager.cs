using UnityEngine;

namespace ToySerialController.UI
{
    public class UIManager
    {
        private static UIManager _instance;
        private MVRScript _plugin;

        public static Transform ConfigurableTextFieldPrefab => _instance?._plugin.manager.configurableTextFieldPrefab;
        public static Transform ConfigurablePopupPrefab => _instance?._plugin.manager.configurablePopupPrefab;
        public static Transform ConfigurableTogglePrefab => _instance?._plugin.manager.configurableTogglePrefab;
        public static Transform ConfigurableSliderPrefab => _instance?._plugin.manager.configurableSliderPrefab;
        public static Transform ConfigurableButtonPrefab => _instance?._plugin.manager.configurableButtonPrefab;

        private UIManager(MVRScript plugin)
        {
            _plugin = plugin;
        }

        public static void Initialize(MVRScript plugin)
        {
            if (_instance != null)
                return;
            
            _instance = new UIManager(plugin);
        }

        public static void RemoveSpacer(UIDynamic o) => _instance._plugin.RemoveSpacer(o);
        public static void RemoveTextField(JSONStorableString o) => _instance._plugin.RemoveTextField(o);
        public static void RemoveButton(UIDynamicButton o) => _instance._plugin.RemoveButton(o);
        public static void RemoveToggle(JSONStorableBool o) => _instance._plugin.RemoveToggle(o);
        public static void RemoveSlider(JSONStorableFloat o) => _instance._plugin.RemoveSlider(o);
        public static void RemovePopup(JSONStorableStringChooser o) => _instance._plugin.RemovePopup(o);

        public static UIDynamic CreateSpacer(bool rightSide) => _instance._plugin.CreateSpacer(rightSide);
        public static UIDynamicToggle CreateToggle(JSONStorableBool storable, bool rightSide) => _instance._plugin.CreateToggle(storable, rightSide);
        public static UIDynamicTextField CreateTextField(JSONStorableString storable, bool rightSide) => _instance._plugin.CreateTextField(storable, rightSide);
        public static UIDynamicSlider CreateSlider(JSONStorableFloat storable, bool rightSide) => _instance._plugin.CreateSlider(storable, rightSide);
        public static UIDynamicButton CreateButton(string label, bool rightSide) => _instance._plugin.CreateButton(label, rightSide);
        public static UIDynamicPopup CreateScrollablePopup(JSONStorableStringChooser storable, bool rightSide) => _instance._plugin.CreateScrollablePopup(storable, rightSide);
        public static UIDynamicPopup CreatePopup(JSONStorableStringChooser storable, bool rightSide) => _instance._plugin.CreatePopup(storable, rightSide);

        public static void RegisterString(JSONStorableString storable) => _instance._plugin.RegisterString(storable);
        public static void RegisterFloat(JSONStorableFloat storable) => _instance._plugin.RegisterFloat(storable);
    }
}
