using SimpleJSON;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;
using DebugUtils;

namespace ToySerialController.MotionSource
{
    public class AnimationMotionSource : AbstractRefreshableMotionSource
    {
        private AnimationPattern _animation;
        private Transform _animatedObject;

        private float _referenceLength;
        private float _referenceRadius;

        private Vector3 _targetPosition;
        private Vector3 _targetUp, _targetRight, _targetForward;

        private JSONStorableStringChooser AnimationChooser;

        private SuperController Controller => SuperController.singleton;

        public override Vector3 ReferencePosition => _animatedObject.position;
        public override Vector3 ReferenceUp => _animatedObject.up;
        public override Vector3 ReferenceRight => _animatedObject.right;
        public override Vector3 ReferenceForward => _animatedObject.forward;
        public override float ReferenceLength => _referenceLength;
        public override float ReferenceRadius => _referenceRadius;
        public override Vector3 ReferencePlaneNormal => _targetUp;
        public override Vector3 ReferencePlaneTangent => _targetRight;

        public override Vector3 TargetPosition => _targetPosition;
        public override Vector3 TargetUp => _targetUp;
        public override Vector3 TargetRight => _targetRight;
        public override Vector3 TargetForward => _targetForward;

        public override void CreateUI(IUIBuilder builder)
        {
            AnimationChooser = builder.CreatePopup("MotionSource:Animation", "Select Animation", null, null, AnimationChooserCallback);

            base.CreateUI(builder);

            FindAnimations();
        }

        public override void DestroyUI(IUIBuilder builder)
        {
            base.DestroyUI(builder);
            builder.Destroy(AnimationChooser);
        }

        public override void StoreConfig(JSONNode config)
        {
            config.Store(AnimationChooser);
        }

        public override void RestoreConfig(JSONNode config)
        {
            config.Restore(AnimationChooser);
            FindAnimations(AnimationChooser.val);
        }

        public override bool Update()
        {
            if (_animation == null)
                return false;

            _animatedObject = _animation.animatedTransform;
            if (_animatedObject == null)
                return false;

            var transform = _animation.transform;
            _targetUp = transform.up;
            _targetRight = transform.right;
            _targetForward = transform.forward;

            var min = Vector3.one * float.MaxValue;
            var max = Vector3.one * float.MinValue;

            for (var i = 0; i < _animation.points.Length; i++)
            {
                for (float t = 0; t <= 1.0f; t += 0.05f)
                {
                    var p = Quaternion.Inverse(transform.rotation) * (_animation.GetPositionFromPoint(i, t) - transform.position);
                    min = Vector3.Min(min, p);
                    max = Vector3.Max(max, p);
                }
            }

            var bounds = new Bounds((min + max) / 2, max - min);
            _referenceLength = bounds.extents.y * 2;
            _referenceRadius = Mathf.Sqrt(bounds.extents.x * bounds.extents.x + bounds.extents.z * bounds.extents.z);
            _targetPosition = transform.position + transform.rotation * (bounds.center + bounds.extents.y * Vector3.up);

            DebugDraw.DrawTransform(_animatedObject, 0.15f);
            DebugDraw.DrawLocalBox(bounds, transform.position, transform.rotation, Color.white);
            DebugDraw.DrawRay(_targetPosition, _targetUp, ReferenceLength, Color.cyan);

            return true;
        }

        private void FindAnimations(string defaultUid = null)
        {
            var animationUids = Controller.GetAtoms()
                .SelectMany(a => a.GetComponentsInChildren<AnimationPattern>())
                .Select(a => a.uid)
                .ToList();

            if (!animationUids.Contains(defaultUid))
                defaultUid = animationUids.FirstOrDefault(uid => uid == _animation?.uid) ?? animationUids.FirstOrDefault() ?? "None";

            animationUids.Insert(0, "None");

            AnimationChooser.choices = animationUids;
            AnimationChooserCallback(defaultUid);
        }

        protected void AnimationChooserCallback(string s)
        {
            _animation = Controller.GetAtoms()
                .SelectMany(a => a.GetComponentsInChildren<AnimationPattern>())
                .FirstOrDefault(c => c.uid == s);
            AnimationChooser.valNoCallback = _animation == null ? "None" : s;
        }

        protected override void RefreshButtonCallback() => FindAnimations(AnimationChooser.val);
    }
}
