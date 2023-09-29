using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;
using DebugUtils;
using System;

namespace ToySerialController.MotionSource
{
    public abstract class AbstractPersonTarget : IMotionSourceTarget
    {
        protected Atom _personAtom;
        protected Vector3 _targetPosition;
        protected Vector3 _targetUp;
        protected Vector3 _targetRight;
        protected Vector3 _targetForward;

        private JSONStorableStringChooser PersonChooser;
        private JSONStorableStringChooser TargetChooser;
        private JSONStorableFloat TargetOffsetSlider;
        
        protected SuperController Controller => SuperController.singleton;

        public IMotionSourceActor Actor { get; set; }

        public Vector3 TargetPosition => _targetPosition + _targetUp * TargetOffsetSlider.val;
        public Vector3 TargetUp => _targetUp;
        public Vector3 TargetRight => _targetRight;
        public Vector3 TargetForward => _targetForward;

        protected IDictionary<string, Func<bool>> TargetUpdaters { get; } = new Dictionary<string, Func<bool>>();

        protected IList<Func<bool>> AutoUpdaters { get; }

        protected abstract IEnumerable<string> Targets { get; }

        protected abstract string DefaultTarget { get; }

        protected virtual string TargetPersonStorageKey => "Target";

        protected virtual string TargetPointStorageKey => "TargetPoint";

        protected virtual string DropDownLabel => "Select Target";

        protected abstract DAZCharacterSelector.Gender TargetGender { get; }

        protected AbstractPersonTarget()
        {
            TargetUpdaters["Auto"] = UpdateAutoTarget;
            TargetUpdaters["Anus"] = UpdateAnusTarget;
            TargetUpdaters["Mouth"] = UpdateMouthTarget;
            TargetUpdaters["Left Hand"] = UpdateLeftHandTarget;
            TargetUpdaters["Right Hand"] = UpdateRightHandTarget;
            TargetUpdaters["Left Foot"] = UpdateLeftFootTarget;
            TargetUpdaters["Right Foot"] = UpdateRightFootTarget;
            TargetUpdaters["Feet"] = UpdateFeetTarget;

            AutoUpdaters = new List<Func<bool>>
            {
                UpdateMouthTarget,
                UpdateLeftHandTarget,
                UpdateRightHandTarget
            };
        }

        public void CreateUI(IUIBuilder builder)
        {
            var targets = new List<string>(Targets);

            PersonChooser = builder.CreatePopup($"MotionSource:{TargetPersonStorageKey}", DropDownLabel, null, null, TargetPersonChooserCallback);
            TargetChooser = builder.CreateScrollablePopup($"MotionSource:{TargetPointStorageKey}", "Select Target Point", targets, DefaultTarget, null);
            TargetOffsetSlider = builder.CreateSlider("MotionSource:TargetOffset", "Target Offset (cm)", 0.0f, -0.15f, 0.15f, true, true, valueFormat: "P2");

            FindTargets();
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(PersonChooser);
            builder.Destroy(TargetChooser);
            builder.Destroy(TargetOffsetSlider);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(PersonChooser);
            config.Store(TargetChooser);
            config.Store(TargetOffsetSlider);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(PersonChooser);
            config.Restore(TargetChooser);
            config.Restore(TargetOffsetSlider);

            FindTargets(PersonChooser.val);
        }

        public bool Update()
        {
            if (_personAtom == null || !_personAtom.on)
                return false;

            if (UpdateTarget())
            {
                return true;
            }

            return false;
        }

        private bool UpdateTarget() => TargetUpdaters.ContainsKey(TargetChooser.val) && TargetUpdaters[TargetChooser.val]();

        private bool UpdateAutoTarget()
        {
            var targets = AutoUpdaters.ToArray();
            var bestPick = targets.First();
            var bestDistance = float.MaxValue;

            foreach (var target in targets)
            {
                if (target())
                {
                    var distance = Vector3.Distance(Actor.ReferencePosition, TargetPosition);
                    if (distance < bestDistance)
                    {
                        bestPick = target;
                        bestDistance = distance;
                    }
                }
            }

            return bestPick();
        }

        private bool UpdateAnusTarget()
        {
            var anusLeft = _personAtom.GetComponentByName<CapsuleCollider>("_JointAl");
            var anusRight = _personAtom.GetComponentByName<CapsuleCollider>("_JointAr");

            if (anusLeft == null || anusRight == null)
                return false;

            _targetUp = ((anusLeft.transform.up + anusRight.transform.up) / 2).normalized;
            _targetRight = ((anusLeft.transform.right + anusRight.transform.right) / 2).normalized;
            _targetForward = ((anusLeft.transform.forward + anusRight.transform.forward) / 2).normalized;
            _targetPosition = (anusLeft.transform.position + anusRight.transform.position) / 2;

            return true;
        }

        private bool UpdateMouthTarget()
        {
            var bottomLip = _personAtom.GetComponentByName<Transform>("lowerJawStandardColliders")?.GetComponentByName<CapsuleCollider>("_ColliderLipM");
            var topLip = _personAtom.GetComponentByName<Transform>("AutoCollidersTongueUpperLip")?.GetComponentByName<Transform>("AutoColliderAutoCollidersFaceCentral2Hard");
            var mouthTrigger = _personAtom.GetRigidBodyByName("MouthTrigger");

            if (bottomLip == null || topLip == null || mouthTrigger == null)
                return false;

            var center = (topLip.transform.position + bottomLip.transform.position) / 2;
            _targetUp = (mouthTrigger.transform.position - center).normalized;
            _targetRight = mouthTrigger.transform.right;
            _targetForward = Vector3.Cross(_targetUp, _targetRight);
            _targetPosition = center - TargetUp * Vector3.Distance(center, mouthTrigger.transform.position) * 0.2f;

            DebugDraw.DrawCircle(TargetPosition, TargetUp, TargetRight, Color.gray, (topLip.transform.position - bottomLip.transform.position).magnitude / 2);

            return true;
        }

        private bool UpdateLeftHandTarget() => UpdateHandTarget("l");

        private bool UpdateRightHandTarget() => UpdateHandTarget("r");

        private bool UpdateHandTarget(string side)
        {
            var carpal = _personAtom.GetRigidBodyByName($"{side}Carpal2");
            var fingerBase = carpal?.GetComponentByName<CapsuleCollider>("_Collider3");
            var fingerTip = _personAtom.GetRigidBodyByName($"{side}Pinky3")?.GetComponentInChildren<CapsuleCollider>();

            if (carpal == null || fingerBase == null || fingerTip == null)
                return false;

            var fingerBasePosition = fingerBase.transform.position - fingerBase.transform.right * (fingerBase.height / 2 - fingerBase.radius) - fingerBase.transform.up * fingerBase.radius;
            var fingerTipPosition = fingerTip.transform.position - fingerTip.transform.right * (fingerTip.height / 2 - fingerTip.radius) - fingerTip.transform.up * fingerTip.radius;
            _targetPosition = (fingerBasePosition + fingerTipPosition) / 2;
            _targetUp = fingerBase.transform.forward;

            if (side == "l")
            {
                _targetRight = -fingerBase.transform.up;
                _targetForward = -fingerBase.transform.right;
            }
            else if (side == "r")
            {
                _targetRight = fingerBase.transform.up;
                _targetForward = fingerBase.transform.right;
            }

            DebugDraw.DrawLine(fingerBasePosition, fingerTipPosition, Color.gray);

            return true;
        }

        private bool UpdateLeftFootTarget() => UpdateFootTarget("l");

        private bool UpdateRightFootTarget() => UpdateFootTarget("r");

        private bool UpdateFootTarget(string side)
        {
            var footBase = _personAtom.GetRigidBodyByName($"{side}Foot")?.GetComponentByName<CapsuleCollider>("_Collider6");

            if (footBase == null)
                return false;

            _targetRight = footBase.transform.forward;
            _targetForward = -footBase.transform.up;

            if (side == "l")
                _targetUp = footBase.transform.right;
            else if (side == "r")
                _targetUp = -footBase.transform.right;

            _targetPosition = footBase.transform.position + _targetForward * footBase.radius;

            return true;
        }

        private bool UpdateFeetTarget()
        {
            var leftFootBase = _personAtom.GetRigidBodyByName("lFoot")?.GetComponentByName<CapsuleCollider>("_Collider6");
            var rightFootBase = _personAtom.GetRigidBodyByName("rFoot")?.GetComponentByName<CapsuleCollider>("_Collider6");

            if (leftFootBase == null || rightFootBase == null)
                return false;

            var leftPosition = leftFootBase.transform.position - leftFootBase.transform.up * leftFootBase.radius;
            var rightPosition = rightFootBase.transform.position - rightFootBase.transform.up * rightFootBase.radius;

            _targetPosition = (leftPosition + rightPosition) / 2;
            _targetForward = ((leftFootBase.transform.forward + rightFootBase.transform.forward) / 2).normalized;
            _targetUp = Vector3.Cross((leftPosition - rightPosition).normalized, _targetForward).normalized;
            _targetRight = Vector3.Cross(_targetUp, _targetForward).normalized;

            DebugDraw.DrawCircle(_targetPosition, Quaternion.FromToRotation(Vector3.up, _targetUp), Color.white, (leftPosition - rightPosition).magnitude / 2);

            return true;
        }

        private void FindTargets(string defaultUid = null)
        {
            var people = Controller.GetAtoms().Where(a => a.type == "Person");
            var targetUids = people
                .Where(a => a.GetComponentInChildren<DAZCharacterSelector>()?.gender == TargetGender)
                .Select(a => a.uid)
                .ToList();

            if (!targetUids.Contains(defaultUid))
                defaultUid = targetUids.FirstOrDefault(uid => uid == _personAtom?.uid) ?? targetUids.FirstOrDefault() ?? "None";

            targetUids.Insert(0, "None");

            PersonChooser.choices = targetUids;
            TargetPersonChooserCallback(defaultUid);
        }

        protected void TargetPersonChooserCallback(string s)
        {
            _personAtom = Controller.GetAtomByUid(s);
            PersonChooser.valNoCallback = _personAtom == null ? "None" : s;
        }

        public void RefreshButtonCallback() => FindTargets(PersonChooser.val);
    }
}