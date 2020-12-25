using System;
using System.IO.Ports;
using System.Linq;
using ToySerialController.Config;
using ToySerialController.MotionSource;
using DebugUtils;
using ToySerialController.Utils;
using MVR.FileManagementSecure;
using Microsoft.Win32.SafeHandles;
using UnityEngine;

namespace ToySerialController
{
    public partial class Plugin : MVRScript
    {
        public static readonly string PluginName = "Toy Serial Controller";
        public static readonly string PluginAuthor = "Yoooi";
        public static readonly string PluginDir = $@"Custom\Scripts\{PluginAuthor}\{PluginName.Replace(" ", "")}";

        private SerialPort _serial;
        private IDevice _device;
        private IMotionSource _motionSource;
        private bool _initialized;
        private int _physicsIteration;

        private SuperController Controller => SuperController.singleton;

        public override void Init()
        {
            base.Init();
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
            if (!_initialized || SuperController.singleton.isLoading)
                return;

            if(_physicsIteration == 0)
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
                if (_motionSource?.Update() == true && _device?.Update(_motionSource) == true)
                {
                    _device.Write(_serial);
                    DeviceReportText.val = _device.GetDeviceReport() ?? string.Empty;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void StartSerial()
        {
            var portName = ComPortChooser.val;
            if (portName != "None")
            {
                if (portName.Substring(0, 3) == "COM" && portName.Length != 4)
                    portName = $@"\\.\{portName}";

                _serial = new SerialPort(portName, 115200)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000,
                    DtrEnable = true,
                    RtsEnable = true
                };
                _serial.Open();

                SuperController.LogMessage($"Serial connection started: {portName}");
            }
        }

        private void StopSerial()
        {
            if (_serial?.IsOpen == true)
            {
                _serial.Close();
                SuperController.LogMessage("Serial connection stopped");
            }
        }

        protected void OnDestroy()
        {
            try
            {
                DebugDraw.Clear();
                StopSerial();
                _device?.Dispose();
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
    }
}