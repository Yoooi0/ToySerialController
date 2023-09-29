using SimpleJSON;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public interface IMotionSourceActor
    {
        Vector3 ReferencePosition { get; }
        Vector3 ReferenceUp { get; }
        Vector3 ReferenceRight { get; }
        Vector3 ReferenceForward { get; }
        float ReferenceLength { get; }
        float ReferenceRadius { get; }
        Vector3 ReferencePlaneNormal { get; }

        void CreateUI(IUIBuilder builder);
        void DestroyUI(IUIBuilder builder);
        void RefreshButtonCallback();
        void RestoreConfig(JSONNode config);
        void StoreConfig(JSONNode config);
        bool Update();
    }
}
