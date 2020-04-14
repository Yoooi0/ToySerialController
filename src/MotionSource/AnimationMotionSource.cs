using Leap.Unity;
using SimpleJSON;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class AnimationMotionSource : AbstractRefreshableMotionSource
    {
        private AnimationPattern _animation;
        private Transform _animatedObject;
        private Bounds _animationBounds;

        private JSONStorableStringChooser AnimationChooser;

        private SuperController Controller => SuperController.singleton;

        public override Vector3 ReferencePosition => _animatedObject.position;
        public override Vector3 ReferenceUp => _animatedObject.up;
        public override Vector3 ReferenceRight => _animatedObject.right;
        public override Vector3 ReferenceForward => _animatedObject.forward;
        public override float ReferenceLength => _animationBounds.extents.y * 2;
        public override Vector3 ReferencePlaneNormal => Vector3.up;

        public override Vector3 TargetPosition => _animationBounds.center + _animationBounds.extents.y * Vector3.up;
        public override Vector3 TargetNormal => Vector3.up;

        public override void CreateUI(UIBuilder builder)
        {
            AnimationChooser = builder.CreatePopup("MotionSource:Animation", "Select Animation", null, null, AnimationChooserCallback);

            FindAnimations();

            base.CreateUI(builder);
        }

        public override void DestroyUI(UIBuilder builder)
        {
            builder.Destroy(AnimationChooser);
            base.DestroyUI(builder);
        }

        public override void StoreConfig(JSONNode config)
        {
            config.Store(AnimationChooser);
        }

        public override void RestoreConfig(JSONNode config)
        {
            config.Restore(AnimationChooser);
        }

        public override bool Update()
        {
            if (_animation == null)
                return false;

            _animatedObject = _animation.animatedTransform;
            if (_animatedObject == null)
                return false;

            var min = Vector3.one * float.MaxValue;
            var max = Vector3.one * float.MinValue;

            for (var i = 0; i < _animation.points.Length; i++)
            {
                for (float t = 0; t <= 1.0f; t += 0.05f)
                {
                    var p = _animation.GetPositionFromPoint(i, t);
                    min = Vector3.Min(min, p);
                    max = Vector3.Max(max, p);
                }
            }

            _animationBounds = new Bounds((min + max) / 2, max - min);

            DebugDraw.DrawTransform(_animatedObject, 0.15f);
            DebugDraw.DrawBox(_animationBounds, Color.white);
            DebugDraw.DrawLine(TargetPosition, TargetPosition + TargetNormal * ReferenceLength, Color.cyan);
            return true;
        }

        private void FindAnimations()
        {
            var animationUids = Controller.GetAtoms()
                .SelectMany(a => a.GetComponentsInChildren<AnimationPattern>())
                .Select(a => a.uid)
                .ToList();

            var defaultAnimation = animationUids.FirstOrDefault(uid => uid == _animation?.uid) ?? animationUids.FirstOrDefault() ?? "None";
            animationUids.Insert(0, "None");

            AnimationChooser.choices = animationUids;
            AnimationChooserCallback(defaultAnimation);
        }

        protected void AnimationChooserCallback(string s)
        {
            _animation = Controller.GetAtoms()
                .SelectMany(a => a.GetComponentsInChildren<AnimationPattern>())
                .FirstOrDefault(c => c.uid == s);
            AnimationChooser.valNoCallback = _animation == null ? "None" : s;
        }

        protected override void RefreshButtonCallback() => FindAnimations();
    }
}
