using System;
using System.Linq;
using ToySerialController.Config;
using ToySerialController.MotionSource;
using DebugUtils;
using ToySerialController.Utils;
using ToySerialController.UI;

namespace ToySerialController
{
    public partial class Plugin : MVRScript
    {
        public static readonly string PluginName = "Toy Serial Controller";
        public static readonly string PluginAuthor = "Yoooi";
        public static readonly string PluginDir = $@"Custom\Scripts\{PluginAuthor}\{PluginName.Replace(" ", "")}";

        private IDevice _device;
        private IMotionSource _motionSource;
        private bool _initialized;
        private bool _isLoading;
        private int _physicsIteration;

        private SuperController Controller => SuperController.singleton;

        public override void Init()
        {
            base.Init();

            UIManager.Initialize(this);

            try
            {
                try
                {
                    var defaultPath = Controller.GetFilesAtPath(PluginDir, "*.json").FirstOrDefault(s => s.EndsWith("default.json"));
                    if (defaultPath != null)
                        ConfigManager.LoadConfig(defaultPath, this);
                } catch { }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        public override void InitUI()
        {
            base.InitUI();
            if (UITransform == null)
                return;

            try
            {
                CreateUI();
                _initialized = true;
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        protected void Update()
        {
            if (SuperController.singleton.isLoading)
                ComponentCache.Clear();

            if (!_initialized || SuperController.singleton.isLoading)
                return;

            DebugDraw.Draw();
            DebugDraw.Enabled = DebugDrawEnableToggle.val;
            _physicsIteration = 0;
        }

        protected void FixedUpdate()
        {
            if (!_initialized)
                return;

            var isLoading = SuperController.singleton.isLoading;
            if (!_isLoading && isLoading)
                OnSceneChanging();
            else if (_isLoading && !isLoading)
                OnSceneChanged();
            _isLoading = isLoading;

            if (_physicsIteration == 0)
                DebugDraw.Clear();

            UpdateDevice();

            if (_physicsIteration == 0)
                DebugDraw.Enabled = false;

            _physicsIteration++;
        }

        private void UpdateDevice()
        {
            try
            {
                if (_motionSource?.Update() == true)
                {
                    _device?.Update(_motionSource, _outputTarget);
                    DeviceReportText.val = _device.GetDeviceReport() ?? string.Empty;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        protected void OnSceneChanging()
        {
            _device?.OnSceneChanging();
            _motionSource?.OnSceneChanging();
        }

        protected void OnSceneChanged()
        {
            _device?.OnSceneChanged();
            _motionSource?.OnSceneChanged();
        }

        protected void OnDestroy()
        {
            try
            {
                DebugDraw.Clear();
                _device?.Dispose();
                _outputTarget?.Dispose();
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
    }
}