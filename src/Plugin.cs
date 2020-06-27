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

        private SafeFileHandle _recordingFile;
        private float _recordingStart;
        private string _lastRecordingBuffer;

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
            UpdateRecording();

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
                    if (_serial?.IsOpen == true)
                        _device.Write(_serial);

                    DeviceReportText.val = _device.GetDeviceReport() ?? string.Empty;
                    SerialReportText.val = _device.GetSerialReport() ?? string.Empty;
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        private void UpdateRecording()
        {
            try
            {
                if (_recordingFile == null)
                {
                    if (RecordingToggle.val)
                    {
                        if (!FileManagerSecure.DirectoryExists(PluginDir))
                            PInvoke.MakeSureDirectoryPathExists(PluginDir);

                        var recordingPath = $@"{PluginDir}\{DateTime.Now:yyyyMMddTHHmmss}_recording.txt";
                        _recordingFile = PInvoke.CreateFile(recordingPath, 2, 0, IntPtr.Zero, 1, 128, IntPtr.Zero);

                        if (_recordingFile == null || _recordingFile.IsInvalid)
                            SuperController.LogError($"Failed to create \"{recordingPath}\"!");
                        else
                            SuperController.LogMessage($"Started recording to \"{recordingPath}\"!");

                        _recordingStart = Time.time;
                    }
                }

                if (_recordingFile != null && !_recordingFile.IsInvalid && !_recordingFile.IsClosed)
                {
                    if (RecordingToggle.val)
                    {
                        var buffer = SerialReportText.val.Replace('\n', ' ').Trim();
                        if (!string.IsNullOrEmpty(buffer) && buffer != _lastRecordingBuffer)
                        {
                            var bytes = System.Text.Encoding.ASCII.GetBytes($"{(Time.time - _recordingStart)};{buffer}\r\n");
                            var written = 0u;
                            var result = PInvoke.WriteFile(_recordingFile.DangerousGetHandle(), bytes, (uint)bytes.Length, out written, IntPtr.Zero);

                            if (!result || written != bytes.Length)
                                SuperController.LogError($"Write to file failed!");

                            _lastRecordingBuffer = buffer;
                        }
                    }
                    else
                    {
                        _recordingFile.Close();
                        _recordingFile.SetHandleAsInvalid();
                        _recordingFile = null;

                        SuperController.LogMessage($"Recording stopped!");
                    }
                }
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