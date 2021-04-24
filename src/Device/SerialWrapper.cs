using DebugUtils;
using System;
using System.IO.Ports;

namespace ToySerialController
{
    public class SerialWrapper
    {
        private SerialPort _serial;
        public int ReadTimeout;
        public int WriteTimeout;
        public bool DtrEnable;
        public bool RtsEnable;
        
        public SerialWrapper(string portName, int portSpeed=115200)
        {
            SuperController.LogMessage("SerialWrapper init " + portName + ":" + portSpeed);
            if (portSpeed > 0)
            {
            _serial = new SerialPort(portName, portSpeed)
                {
                    ReadTimeout = ReadTimeout,
                    WriteTimeout = WriteTimeout,
                    DtrEnable = DtrEnable,
                    RtsEnable = RtsEnable
                };
            }
        }
        
        public virtual void Open()
        {
            SuperController.LogMessage("SerialWrapper Open()");
            _serial.Open();
        }
        
        public virtual void Close()
        {
            SuperController.LogMessage("SerialWrapper Close()");
            _serial.Close();
        }
        
        public virtual void Write(string data)
        {
            SuperController.LogMessage("SerialWrapper Write()");
            _serial.Write(data);
        }
        
        public virtual bool IsOpen()
        {
            SuperController.LogMessage("SerialWrapper IsOpen()");
            return _serial.IsOpen;
        }
    }
}
