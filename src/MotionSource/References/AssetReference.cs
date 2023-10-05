using SimpleJSON;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class AssetReference : AbstractAssetBase, IMotionSourceReference
    {
        private JSONStorableFloat LengthScaleSlider;

        public float Length { get; private set; }
        public float Radius { get; private set; }
        public Vector3 PlaneNormal => Up;

        public override void CreateUI(IUIBuilder builder)
        {
            base.CreateUI(builder);
            LengthScaleSlider = builder.CreateSlider("MotionSource:LengthScale", "Length Scale (%)", 1, 0, 2, true, true, valueFormat: "P0");
        }

        public override void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(LengthScaleSlider);
            base.DestroyUI(builder);
        }

        public override void StoreConfig(JSONNode config)
        {
            base.StoreConfig(config);
            config.Store(LengthScaleSlider);
        }

        public override void RestoreConfig(JSONNode config)
        {
            base.RestoreConfig(config);
            config.Restore(LengthScaleSlider);
        }

        public override bool Update()
        {
            if (!base.Update())
            {
                return false;
            }

            Length = LengthScaleSlider.val * Vector3.Project(Extents, Up).magnitude * 2;
            Radius = Mathf.Min(Vector3.Project(Extents, Right).magnitude, Vector3.Project(Extents, Forward).magnitude);

            return true;
        }
    }
}
