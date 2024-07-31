﻿using System;
using ToySerialController.Config;
using ToySerialController.Device.OutputTarget;
using ToySerialController.MotionSource;
using ToySerialController.UI;

namespace ToySerialController
{
    public interface IDevice : IUIProvider, IConfigProvider, IDisposable
    {
        bool Update(IMotionSource motionSource, IOutputTarget outputTarget);
        void RecordValues(IDeviceRecorder deviceRecorder);

        string GetDeviceReport();
        void OnSceneChanging();
        void OnSceneChanged();
    }
}
