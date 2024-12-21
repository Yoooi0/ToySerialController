using ToySerialController.Config;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public interface IMotionSourceReference : IUIProvider, IConfigProvider
    {
        Vector3 Position { get; }
        Vector3 Up { get; }
        Vector3 Right { get; }
        Vector3 Forward { get; }
        float Length { get; }
        float Radius { get; }
        Vector3 PlaneNormal { get; }
        Vector3 PlaneTangent { get; }

        void Refresh();
        bool Update();
    }
}
