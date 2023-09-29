using ToySerialController.Config;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public interface IMotionSourceTarget : IUIProvider, IConfigProvider
    {
        Vector3 Position { get; }
        Vector3 Up { get; }
        Vector3 Right { get; }
        Vector3 Forward { get; }

        void Refresh();
        bool Update(IMotionSourceReference reference);
    }
}
