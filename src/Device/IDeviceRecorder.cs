using System;
using ToySerialController.UI;

namespace ToySerialController
{
    public interface IDeviceRecorder : IUIProvider, IDisposable
    {
        void StartRecording();
        void StopRecording();
        void RecordValues(float time, float l0, float l1, float l2, float r0, float r1, float r2);
    }
}
