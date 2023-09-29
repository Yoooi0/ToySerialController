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
        protected Vector3 _position;

        private JSONStorableStringChooser PersonChooser;
        private JSONStorableStringChooser TargetChooser;
        private JSONStorableFloat TargetOffsetSlider;
        
        protected SuperController Controller => SuperController.singleton;

        public IMotionSourceReference Reference { get; set; }

        public Vector3 Position => _position + Up * TargetOffsetSlider.val;
        public Vector3 Up { get; protected set; }
        public Vector3 Right { get; protected set; }
        public Vector3 Forward { get; protected set; }

        private IDictionary<string, Func<bool>> TargetUpdaters { get; }

        private IList<Func<bool>> AutoUpdaters { get; }

        protected abstract IList<string> Targets { get; }

        protected abstract string DefaultTarget { get; }

        protected virtual string TargetPersonStorageKey => "Target";

        protected virtual string TargetPointStorageKey => "TargetPoint";

        protected virtual string TargetPersonChooserLabel => "Select Target";

        protected abstract DAZCharacterSelector.Gender TargetGender { get; }

        protected AbstractPersonTarget()
        {
            TargetUpdaters = new Dictionary<string, Func<bool>>
            {
                { "Auto",  UpdateAutoTarget },
                { "Anus",  UpdateAnusTarget },
                { "Mouth",  UpdateMouthTarget },
                { "Left Hand",  UpdateLeftHandTarget },
                { "Right Hand",  UpdateRightHandTarget },
                { "Left Foot",  UpdateLeftFootTarget },
                { "Right Foot",  UpdateRightFootTarget },
                { "Feet",  UpdateFeetTarget }
            };

            AutoUpdaters = new List<Func<bool>>
            {
                UpdateMouthTarget,
                UpdateLeftHandTarget,
                UpdateRightHandTarget
            };
        }

        protected void RegisterUpdater(string key, Func<bool> updater)
        {
            TargetUpdaters[key] = updater;
        }

        protected void RegisterAutoUpdater(Func<bool> updater)
        {
            AutoUpdaters.Add(updater);
        }

        protected void RegisterAutoUpdater(string key)
        {
            if (!TargetUpdaters.ContainsKey(key))
            {
                throw new ArgumentException("No matching updater found. Did you call RegisterUpdater?", key);
            }

            RegisterAutoUpdater(TargetUpdaters[key]);
        }

        public void CreateUI(IUIBuilder builder)
        {
            PersonChooser = builder.CreatePopup($"MotionSource:{TargetPersonStorageKey}", TargetPersonChooserLabel, null, null, TargetPersonChooserCallback);
            TargetChooser = builder.CreateScrollablePopup($"MotionSource:{TargetPointStorageKey}", "Select Target Point", Targets.ToList(), DefaultTarget, null);
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
            var bestPick = AutoUpdaters.First();
            var bestDistance = float.MaxValue;

            foreach (var target in AutoUpdaters)
            {
                if (target())
                {
                    var distance = Vector3.Distance(Reference.Position, Position);
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

            Up = ((anusLeft.transform.up + anusRight.transform.up) / 2).normalized;
            Right = ((anusLeft.transform.right + anusRight.transform.right) / 2).normalized;
            Forward = ((anusLeft.transform.forward + anusRight.transform.forward) / 2).normalized;
            _position = (anusLeft.transform.position + anusRight.transform.position) / 2;

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
            Up = (mouthTrigger.transform.position - center).normalized;
            Right = mouthTrigger.transform.right;
            Forward = Vector3.Cross(Up, Right);
            _position = center - Up * Vector3.Distance(center, mouthTrigger.transform.position) * 0.2f;

            DebugDraw.DrawCircle(Position, Up, Right, Color.gray, (topLip.transform.position - bottomLip.transform.position).magnitude / 2);

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
            _position = (fingerBasePosition + fingerTipPosition) / 2;
            Up = fingerBase.transform.forward;

            if (side == "l")
            {
                Right = -fingerBase.transform.up;
                Forward = -fingerBase.transform.right;
            }
            else if (side == "r")
            {
                Right = fingerBase.transform.up;
                Forward = fingerBase.transform.right;
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

            Right = footBase.transform.forward;
            Forward = -footBase.transform.up;

            if (side == "l")
                Up = footBase.transform.right;
            else if (side == "r")
                Up = -footBase.transform.right;

            _position = footBase.transform.position + Forward * footBase.radius;

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

            _position = (leftPosition + rightPosition) / 2;
            Forward = ((leftFootBase.transform.forward + rightFootBase.transform.forward) / 2).normalized;
            Up = Vector3.Cross((leftPosition - rightPosition).normalized, Forward).normalized;
            Right = Vector3.Cross(Up, Forward).normalized;

            DebugDraw.DrawCircle(_position, Quaternion.FromToRotation(Vector3.up, Up), Color.white, (leftPosition - rightPosition).magnitude / 2);

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

        public void Refresh() => FindTargets(PersonChooser.val);
    }
}