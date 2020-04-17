using System;
using System.IO.Ports;
using System.Linq;
using ToySerialController.Config;
using ToySerialController.MotionSource;
using DebugUtils;

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

        private SuperController Controller => SuperController.singleton;

        public override void Init()
        {
            try
            {
                CreateUI();

                var defaultPath = Controller.GetFilesAtPath(PluginDir, "*.json").FirstOrDefault(s => s.EndsWith("default.json"));
                if (defaultPath != null)
                    ConfigManager.LoadConfig(defaultPath, this);
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        protected void Update()
        {
            try
            {
                DebugDraw.Enabled = DebugDrawEnableToggle.val;
                DebugDraw.Clear();

                if (_motionSource?.Update() == true && _device?.Update(_motionSource) == true)
                {
                    if (_serial?.IsOpen == true)
                        _device.Write(_serial);

                    DeviceReportText.val = _device.GetDeviceReport() ?? string.Empty;
                    SerialReportText.val = _device.GetSerialReport() ?? string.Empty;
                }

                DebugDraw.Draw();
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void StartSerial()
        {
            var comPort = ComPortChooser.val;
            var baudRate = BaudRateChooser.val;
            if (comPort != "None")
            {
                // Add extra characters for COM ports > 9
                if (comPort.Substring(0, 3) == "COM" && comPort.Length != 4)
                    comPort = $@"\\.\{comPort}";

                _serial = new SerialPort(comPort, Convert.ToInt32(baudRate))
                {
                    ReadTimeout = 10
                };
                _serial.Open();

                SuperController.LogMessage($"Serial connection started: {comPort}, baud {baudRate}");
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