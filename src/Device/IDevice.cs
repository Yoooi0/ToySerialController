using System;
using System.IO.Ports;
using ToySerialController.Config;
using ToySerialController.MotionSource;
using ToySerialController.UI;

namespace ToySerialController
{
    public interface IDevice : IUIProvider, IConfigProvider, IDisposable
    {
        bool Update(IMotionSource motionSource);
        void Write(SerialWrapper serial);

        string GetDeviceReport();
    }
}
