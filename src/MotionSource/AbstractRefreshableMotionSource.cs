using SimpleJSON;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public abstract class AbstractRefreshableMotionSource : IMotionSource
    {
        private UIDynamicButton RefreshButton;

        public abstract Vector3 ReferencePosition { get; }
        public abstract Vector3 ReferenceUp { get; }
        public abstract Vector3 ReferenceRight { get; }
        public abstract Vector3 ReferenceForward { get; }
        public abstract float ReferenceLength { get; }
        public abstract Vector3 ReferencePlaneNormal { get; }
        public abstract Vector3 TargetPosition { get; }
        public abstract Vector3 TargetNormal { get; }

        public abstract bool Update();
        public abstract void StoreConfig(JSONNode config);
        public abstract void RestoreConfig(JSONNode config);

        public virtual void CreateUI(IUIBuilder builder)
        {
            RefreshButton = builder.CreateButton("Refresh", RefreshButtonCallback);
            RefreshButton.buttonColor = new Color(0, 0.75f, 1f) * 0.8f;
            RefreshButton.textColor = Color.white;
        }

        public virtual void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(RefreshButton);
        }

        protected abstract void RefreshButtonCallback();
    }
}
