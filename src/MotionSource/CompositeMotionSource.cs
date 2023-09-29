using DebugUtils;
using SimpleJSON;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class CompositeMotionSource<TActor, TTarget> : AbstractRefreshableMotionSource, IMotionSource
        where TActor : IMotionSourceReference, new()
        where TTarget : IMotionSourceTarget, new()
    {
        protected IMotionSourceReference Reference { get; } = new TActor();
        protected IMotionSourceTarget Target { get; } = new TTarget();

        public override Vector3 ReferencePosition => Reference.Position;
        public override Vector3 ReferenceUp => Reference.Up;
        public override Vector3 ReferenceRight => Reference.Right;
        public override Vector3 ReferenceForward => Reference.Forward;
        public override float ReferenceLength => Reference.Length;
        public override float ReferenceRadius => Reference.Radius;
        public override Vector3 ReferencePlaneNormal => Reference.PlaneNormal;
        public override Vector3 TargetPosition => Target.Position;
        public override Vector3 TargetUp => Target.Up;
        public override Vector3 TargetRight => Target.Right;
        public override Vector3 TargetForward => Target.Forward;

        public CompositeMotionSource() : base()
        {
            //only required by auto target /shrug
            Target.Reference = Reference;
        }

        public override void RestoreConfig(JSONNode config)
        {
            Reference.RestoreConfig(config);
            Target.RestoreConfig(config);
        }

        public override void StoreConfig(JSONNode config)
        {
            Reference.StoreConfig(config);
            Target.StoreConfig(config);
        }

        public override bool Update()
        {
            if (Reference.Update() && Target.Update())
            {
                DebugDraw.DrawSquare(ReferencePosition, ReferencePlaneNormal, ReferenceRight, Color.white, 0.33f);
                DebugDraw.DrawTransform(ReferencePosition, ReferenceUp, ReferenceRight, ReferenceForward, 0.15f);
                DebugDraw.DrawRay(ReferencePosition, ReferenceUp, ReferenceLength, Color.white);
                DebugDraw.DrawLine(ReferencePosition, TargetPosition, Color.yellow);
                DebugDraw.DrawTransform(TargetPosition, TargetUp, TargetRight, TargetForward, 0.15f);

                return true;
            }

            return false;
        }

        public override void CreateUI(IUIBuilder builder)
        {
            Target.CreateUI(builder);
            Reference.CreateUI(builder);
            base.CreateUI(builder);
        }

        public override void DestroyUI(IUIBuilder builder)
        {
            base.DestroyUI(builder);
            Target.DestroyUI(builder);
            Reference.DestroyUI(builder);
        }

        protected override void RefreshButtonCallback()
        {
            Reference.Refresh();
            Target.Refresh();
        }
    }
}
