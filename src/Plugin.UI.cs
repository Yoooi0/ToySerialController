using SimpleJSON;
using System.Collections.Generic;
using System.IO.Ports;
using ToySerialController.Config;
using ToySerialController.MotionSource;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController
{
    public partial class Plugin : MVRScript, IConfigProvider
    {
        private UIBuilder _builder;
        private UIGroup _group;

        private UIDynamicButton PluginTitle, MotionSourceTitle, HardwareTitle;
        private JSONStorableStringChooser ComPortChooser, BaudRateChooser;
        private JSONStorableStringChooser DeviceChooser, MotionSourceChooser;
        private UIDynamicButton StartSerialButton, StopSerialButton;
        private JSONStorableString SerialReportText, DeviceReportText;
        private JSONStorableBool RecordingToggle, DebugDrawEnableToggle;

        private UIHorizontalGroup PresetButtonGroup, SerialButtonGroup;

        public void CreateUI()
        {
            pluginLabelJSON.val = PluginName;

            _builder = new UIBuilder(this);
            _group = new UIGroup(_builder);
            _group.BlacklistStorable("Device Report");
            _group.BlacklistStorable("Serial Report");

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

            RecordingToggle = _group.CreateToggle("Plugin:Recording", "Recording", false);
            _group.BlacklistStorable("Plugin:Recording");

            DebugDrawEnableToggle = _group.CreateToggle("Plugin:DebugDrawEnable", "Enable Debug", false);

            var hardwareGroup = new UIGroup(_group);
            var visible = false;
            HardwareTitle = _group.CreateButton("Hardware", () => hardwareGroup.SetVisible(visible = !visible), new Color(0.3f, 0.3f, 0.3f), Color.white);
            DeviceChooser = hardwareGroup.CreatePopup("Plugin:DeviceChooser", "Select device", new List<string> { "T-code" }, "T-code", DeviceChooserCallback);
            DeviceReportText = hardwareGroup.CreateTextField("Device Report", "", 290);

            ComPortChooser = hardwareGroup.CreatePopup("Plugin:ComPortChooser", "Select COM port", SerialPort.GetPortNames().ToList(), "None", null);
            BaudRateChooser = hardwareGroup.CreatePopup("Plugin:BaudRateChooser", "Select baud rate", new List<string> { "9600", "19200", "38400", "74880", "115200", "230400", "250000" }, "115200", null);

            SerialButtonGroup = hardwareGroup.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => _group.CreateButtonEx());
            var startSerialButton = SerialButtonGroup.items[0].GetComponent<UIDynamicButton>();
            startSerialButton.label = "Start Serial";
            startSerialButton.button.onClick.AddListener(StartButtonCallback);

            var stopSerialButton = SerialButtonGroup.items[1].GetComponent<UIDynamicButton>();
            stopSerialButton.label = "Stop Serial";
            stopSerialButton.button.onClick.AddListener(StopButtonCallback);

            SerialReportText = hardwareGroup.CreateTextField("Serial Report", "", 290);

            hardwareGroup.SetVisible(false);

            MotionSourceTitle = _group.CreateDisabledButton("Motion Source", new Color(0.3f, 0.3f, 0.3f), Color.white);
            MotionSourceChooser = _group.CreatePopup("Plugin:MotionSourceChooser", "Select motion source", new List<string> { "Male + Female", "Asset + Female", "Dildo + Female", "Animation Pattern", "Range Test" }, "Male + Female", MotionSourceChooserCallback);

            DeviceChooserCallback("T-code");
            MotionSourceChooserCallback("Male + Female");
        }

        public void StoreConfig(JSONNode config)
        {
            _group.StoreConfig(config);
            _device?.StoreConfig(config);
            _motionSource?.StoreConfig(config);
        }

        public void RestoreConfig(JSONNode config)
        {
            _group.RestoreConfig(config);
            _device?.RestoreConfig(config);
            _motionSource?.RestoreConfig(config);
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

            if (s == "T-code")
                _device = new TCodeDevice();
            else
                return;

            _device.CreateUI(_builder);
        }

        protected void MotionSourceChooserCallback(string s)
        {
            _motionSource?.DestroyUI(_builder);
            _motionSource = null;

            if (s == "Male + Female")
                _motionSource = new MaleFemaleMotionSource();
            else if (s == "Asset + Female")
                _motionSource = new AssetFemaleMotionSource();
            else if (s == "Dildo + Female")
                _motionSource = new DildoFemaleMotionSource();
            else if (s == "Animation Pattern")
                _motionSource = new AnimationMotionSource();
            else if (s == "Range Test")
                _motionSource = new RangeTestMotionSource();
            else
                return;

            _motionSource.CreateUI(_builder);
        }

        protected void StartButtonCallback() => StartSerial();
        protected void StopButtonCallback() => StopSerial();
    }
}