using SimpleJSON;
using System;
using System.IO.Ports;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.Device.OutputTarget
{
    public class SerialOutputTarget : IOutputTarget
    {
        private JSONStorableStringChooser ComPortChooser;
        private UIHorizontalGroup  ButtonGroup;

        private SerialPort _serial;

        public void CreateUI(IUIBuilder builder)
        {
            ComPortChooser = builder.CreatePopup("OutputTarget:Serial:ComPortChooser", "Select COM port", SerialPort.GetPortNames().ToList(), "None", null);

            ButtonGroup = builder.CreateHorizontalGroup(510, 50, new Vector2(10, 0), 2, idx => builder.CreateButtonEx());
            var startSerialButton = ButtonGroup.items[0].GetComponent<UIDynamicButton>();
            startSerialButton.label = "Start Serial";
            startSerialButton.button.onClick.AddListener(StartSerial);

            var stopSerialButton = ButtonGroup.items[1].GetComponent<UIDynamicButton>();
            stopSerialButton.label = "Stop Serial";
            stopSerialButton.button.onClick.AddListener(StopSerial);
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(ComPortChooser);
            builder.Destroy(ButtonGroup);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(ComPortChooser);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(ComPortChooser);
        }

        public void Write(string data)
        {
            if (_serial?.IsOpen == true)
                _serial.Write(data);
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
                _serial = null;
                SuperController.LogMessage("Serial connection stopped");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            StopSerial();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
