using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.Config;
using ToySerialController.Device.OutputTarget;
using ToySerialController.MotionSource;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController
{
    public partial class Plugin : MVRScript, IConfigProvider
    {
        private UIBuilder _builder;
        private UIGroup _group;
        private IOutputTarget _outputTarget;

        private OutputTargetSettings OutputTargetSettings;
        private UIDynamicButton PluginTitle, MotionSourceTitle, DebugTitle;
        private JSONStorableStringChooser MotionSourceChooser;
        private JSONStorableString DeviceReportText;
        private JSONStorableBool DebugDrawEnableToggle;

        private UIHorizontalGroup PresetButtonGroup;

        public void CreateUI()
        {
            pluginLabelJSON.val = PluginName;

            _builder = new UIBuilder();
            _group = new UIGroup(_builder);
            _group.BlacklistStorable("Device Report");

            PluginTitle = _group.CreateDisabledButton("Plugin", new Color(0.3f, 0.3f, 0.3f), Color.white);

            PresetButtonGroup = _group.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 3, idx => _group.CreateButtonEx());
            var saveButton = PresetButtonGroup.items[0].GetComponent<UIDynamicButton>();
            saveButton.buttonText.fontSize = 25;
            saveButton.label = "Save Config";
            saveButton.buttonColor = new Color(0.309f, 1f, 0.039f) * 0.8f;
            saveButton.textColor = Color.white;
            saveButton.button.onClick.AddListener(SaveConfigCallback);

            var loadButton = PresetButtonGroup.items[1].GetComponent<UIDynamicButton>();
            loadButton.buttonText.fontSize = 25;
            loadButton.label = "Load Config";
            loadButton.buttonColor = new Color(1f, 0.168f, 0.039f) * 0.8f;
            loadButton.textColor = Color.white;
            loadButton.button.onClick.AddListener(LoadConfigCallback);

            var defaultButton = PresetButtonGroup.items[2].GetComponent<UIDynamicButton>();
            defaultButton.buttonText.fontSize = 25;
            defaultButton.label = "As Default";
            defaultButton.buttonColor = new Color(1f, 0.870f, 0.039f) * 0.8f;
            defaultButton.textColor = Color.white;
            defaultButton.button.onClick.AddListener(SaveDefaultConfigCallback);

            OutputTargetSettings = new OutputTargetSettings(t => _outputTarget = t);
            OutputTargetSettings.CreateUI(_group);

            var debugGroup = new UIGroup(_group);
            var visible = false;
            DebugTitle = _group.CreateButton("Debug", () => debugGroup.SetVisible(visible = !visible), new Color(0.3f, 0.3f, 0.3f), Color.white);

            DebugDrawEnableToggle = debugGroup.CreateToggle("Plugin:DebugDrawEnable", "Enable Debug", false);
            DeviceReportText = debugGroup.CreateTextField("Device Report", "", 320);
            DeviceReportText.text.font = Font.CreateDynamicFontFromOSFont("Consolas", 24);
            DeviceReportText.text.fontSize = 24;

            debugGroup.SetVisible(false);

            MotionSourceTitle = _group.CreateDisabledButton("Motion Source", new Color(0.3f, 0.3f, 0.3f), Color.white);

            var motionSources = new List<string>
            {
                "Male + Female", "Asset + Female", "Dildo + Female",
                "Male + Male", "Asset + Male", "Dildo + Male",
                "Animation Pattern", "Range Test"
            };
            MotionSourceChooser = _group.CreatePopup("Plugin:MotionSourceChooser", "Select motion source", motionSources, "Male + Female", MotionSourceChooserCallback);

            DeviceChooserCallback("T-code");
            MotionSourceChooserCallback("Male + Female");
        }

        public void StoreConfig(JSONNode config)
        {
            OutputTargetSettings?.StoreConfig(config);

            _group.StoreConfig(config);
            _device?.StoreConfig(config);
            _motionSource?.StoreConfig(config);
        }

        public void RestoreConfig(JSONNode config)
        {
            OutputTargetSettings?.RestoreConfig(config);

            _group.RestoreConfig(config);
            _device?.RestoreConfig(config);
            _motionSource?.RestoreConfig(config);
        }

        public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
        {
            if (!awakecalled)
                Awake();

            needsStore = false;
            var config = ConfigManager.GetJSON(this);
            config["id"] = storeId;
            needsStore = true;

            return config;
        }


        public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
        {
            insideRestore = true;
            if (!awakecalled)
                Awake();

            ConfigManager.RestoreFromJSON(jc, this);
            insideRestore = false;
        }

        protected void SaveConfigCallback() => ConfigManager.OpenSaveDialog(SaveDialogCallback);
        protected void LoadConfigCallback() => ConfigManager.OpenLoadDialog(LoadDialogCallback);
        protected void SaveDefaultConfigCallback() => ConfigManager.SaveConfig($@"{PluginDir}\default.json", this);
        protected void SaveDialogCallback(string path) => ConfigManager.SaveConfig(path, this);
        protected void LoadDialogCallback(string path) => ConfigManager.LoadConfig(path, this);

        protected void DeviceChooserCallback(string s)
        {
            _device?.DestroyUI(_builder);
            _device?.Dispose();
            _device = null;

            _device = new TCodeDevice();
            _device.CreateUI(_builder);
        }

        protected void MotionSourceChooserCallback(string s)
        {
            _motionSource?.DestroyUI(_builder);
            _motionSource = null;

            if (s == "Male + Female")
                _motionSource = new CompositeMotionSource<MaleActor, FemaleTarget>();
            else if (s == "Asset + Female")
                _motionSource = new CompositeMotionSource<AssetActor, FemaleTarget>();
            else if (s == "Dildo + Female")
                _motionSource = new CompositeMotionSource<DildoActor, FemaleTarget>();
            else if (s == "Male + Male")
                _motionSource = new CompositeMotionSource<MaleActor, MaleTarget>();
            else if (s == "Asset + Male")
                _motionSource = new CompositeMotionSource<AssetActor, MaleTarget>();
            else if (s == "Dildo + Male")
                _motionSource = new CompositeMotionSource<DildoActor, MaleTarget>();
            else if (s == "Animation Pattern")
                _motionSource = new AnimationMotionSource();
            else if (s == "Range Test")
                _motionSource = new RangeTestMotionSource();
            else
                return;

            _motionSource.CreateUI(_builder);
        }
    }

    public class OutputTargetSettings : IUIProvider, IConfigProvider, IDisposable
    {
        private readonly Action<IOutputTarget> _callback;
        private readonly Dictionary<string, IOutputTarget> _outputTargets;
        private readonly Dictionary<string, UIGroup> _uiGroups;

        private JSONStorableStringChooser OutputTargetChooser;

        private string _selectedOutputTarget;

        public OutputTargetSettings(Action<IOutputTarget> callback)
        {
            _callback = callback;
            _outputTargets = new Dictionary<string, IOutputTarget>()
            {
                ["None"] = null,
                ["Serial"] = new SerialOutputTarget(),
                ["Udp"] = new UdpOutputTarget()
            };
            _uiGroups = new Dictionary<string, UIGroup>();
        }

        public void CreateUI(IUIBuilder builder)
        {
            var names = _outputTargets.Select(x => x.Key).ToList();
            OutputTargetChooser = builder.CreateScrollablePopup("Device:OutputTarget", "Select Output Target", names, names.First(), OuputTargetChooserCallback);

            foreach (var item in _outputTargets)
            {
                var group = new UIGroup(builder);
                item.Value?.CreateUI(group);
                group.SetVisible(false);
                _uiGroups[item.Key] = group;
            }

            OuputTargetChooserCallback(OutputTargetChooser.val);

        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(OutputTargetChooser);
            foreach (var item in _uiGroups)
                item.Value?.Destroy(builder);

            _uiGroups.Clear();
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(OutputTargetChooser);
            foreach (var item in _outputTargets)
                item.Value?.RestoreConfig(config);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(OutputTargetChooser);
            foreach (var item in _outputTargets)
                item.Value?.StoreConfig(config);
        }

        public void UpdateVisibility(bool parentVisible)
        {
            foreach (var item in _uiGroups)
                item.Value?.SetVisible(false);

            if (parentVisible && _selectedOutputTarget != null)
                _uiGroups[_selectedOutputTarget]?.SetVisible(true);
        }

        private void OuputTargetChooserCallback(string s)
        {
            if (_selectedOutputTarget != null)
                _uiGroups[_selectedOutputTarget]?.SetVisible(false);

            _uiGroups[s]?.SetVisible(true);
            _selectedOutputTarget = s;

            if (s != null)
                _callback(_outputTargets[s]);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (var item in _outputTargets)
                item.Value?.Dispose();

            _outputTargets.Clear();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}