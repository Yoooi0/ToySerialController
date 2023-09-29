using DebugUtils;
using SimpleJSON;
using ToySerialController.UI;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class CompositeMotionSource<TActor, TTarget> : AbstractRefreshableMotionSource, IMotionSource
        where TActor : IMotionSourceActor, new()
        where TTarget : IMotionSourceTarget, new()
    {
        protected IMotionSourceActor Actor { get; } = new TActor();
        protected IMotionSourceTarget Target { get; } = new TTarget();

        public override Vector3 ReferencePosition => Actor.ReferencePosition;
        public override Vector3 ReferenceUp => Actor.ReferenceUp;
        public override Vector3 ReferenceRight => Actor.ReferenceRight;
        public override Vector3 ReferenceForward => Actor.ReferenceForward;
        public override float ReferenceLength => Actor.ReferenceLength;
        public override float ReferenceRadius => Actor.ReferenceRadius;
        public override Vector3 ReferencePlaneNormal => Actor.ReferencePlaneNormal;
        public override Vector3 TargetPosition => Target.TargetPosition;
        public override Vector3 TargetUp => Target.TargetUp;
        public override Vector3 TargetRight => Target.TargetRight;
        public override Vector3 TargetForward => Target.TargetForward;

        public CompositeMotionSource() : base()
        {
            //only required by auto target /shrug
            Target.Actor = Actor;
        }

        public override void RestoreConfig(JSONNode config)
        {
            Actor.RestoreConfig(config);
            Target.RestoreConfig(config);
        }

        public override void StoreConfig(JSONNode config)
        {
            Actor.StoreConfig(config);
            Target.StoreConfig(config);
        }

        public override bool Update()
        {
            if (Actor.Update() && Target.Update())
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
            Actor.CreateUI(builder);
            base.CreateUI(builder);
        }

        public override void DestroyUI(IUIBuilder builder)
        {
            base.DestroyUI(builder);
            Target.DestroyUI(builder);
            Actor.DestroyUI(builder);
        }

        protected override void RefreshButtonCallback()
        {
            Actor.RefreshButtonCallback();
            Target.RefreshButtonCallback();
        }
    }
}
