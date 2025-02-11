using System;
using ToySerialController.Config;
using ToySerialController.MotionSource;
using DebugUtils;
using ToySerialController.Utils;
using ToySerialController.UI;
using MVR.FileManagementSecure;

namespace ToySerialController
{
    public partial class Plugin : MVRScript
    {
        public static readonly string PluginName = "Toy Serial Controller";
        public static readonly string PluginAuthor = "Yoooi";
        public static readonly string PluginDir = $@"Custom\Scripts\{PluginAuthor}\{PluginName.Replace(" ", "")}";

        private IDevice _device;
        private IMotionSource _motionSource;
        private IDeviceRecorder _recorder;
        private bool _initialized;
        private bool _isLoading;
        private int _physicsIteration;

        public override void Init()
        {
            base.Init();
            UIManager.Initialize(this);

            _recorder = new BinaryDeviceRecorder();
        }

        public override void InitUI()
        {
            base.InitUI();
            if (UITransform == null)
                return;

            try
            {
                CreateUI();
                ConfigManager.LoadConfig($@"{PluginDir}\default.json", this);
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }

            _initialized = true;
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
            _device?.RecordValues(_recorder);
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
            _device?.RecordValues(_recorder);
        }

        private void UpdateDevice()
        {
            try
            {
                var motionValid = _motionSource?.Update() == true;
                _device?.Update(motionValid ? _motionSource : null, _outputTarget);

                if (DebugDrawEnableToggle.val)
                {
                    DeviceReportText.val = _device?.GetDeviceReport() ?? string.Empty;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        protected void OnSceneChanging()
        {
            _recorder?.StopRecording();

            _device?.OnSceneChanging();
            _motionSource?.OnSceneChanging();

            DebugDraw.Reset();
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
                _recorder?.Dispose();
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
    }
}