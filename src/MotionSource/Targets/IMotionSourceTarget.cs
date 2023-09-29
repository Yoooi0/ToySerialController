using SimpleJSON;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public interface IMotionSourceTarget
    {
        IMotionSourceReference Reference { get; set; }

        Vector3 TargetPosition { get; }
        Vector3 TargetUp { get; }
        Vector3 TargetRight { get; }
        Vector3 TargetForward { get; }

        void RefreshButtonCallback();
        void RestoreConfig(JSONNode config);
        void StoreConfig(JSONNode config);
        bool Update();

        void CreateUI(IUIBuilder builder);
        void DestroyUI(IUIBuilder builder);
    }
}
