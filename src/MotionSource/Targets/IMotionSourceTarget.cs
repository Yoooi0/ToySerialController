using ToySerialController.Config;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public interface IMotionSourceTarget : IUIProvider, IConfigProvider
    {
        IMotionSourceReference Reference { get; set; }

        Vector3 Position { get; }
        Vector3 Up { get; }
        Vector3 Right { get; }
        Vector3 Forward { get; }

        void Refresh();
        bool Update();
    }
}
