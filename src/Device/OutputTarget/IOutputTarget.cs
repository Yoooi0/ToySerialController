using System;
using ToySerialController.Config;
using ToySerialController.UI;

namespace ToySerialController.Device.OutputTarget
{
    public interface IOutputTarget : IUIProvider, IConfigProvider, IDisposable
    {
        void Write(string data);
    }
}
