﻿using SimpleJSON;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class AssetReference : AbstractAssetMotion, IMotionSourceReference
    {
        private JSONStorableFloat LengthScaleSlider;

        public float Length => LengthScaleSlider.val * Vector3.Project(Extents, Up).magnitude * 2;
        public float Radius => Vector3.ProjectOnPlane(Extents, Up).magnitude;
        public Vector3 PlaneNormal => Up;

        public override void CreateUI(IUIBuilder builder)
        {
            base.CreateUI(builder);
            LengthScaleSlider = builder.CreateSlider("MotionSource:LengthScale", "Length Scale", 1, 0, 2, true, true);
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
    }
}
