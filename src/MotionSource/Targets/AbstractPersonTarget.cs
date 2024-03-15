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
        private readonly Dictionary<string, Func<IMotionSourceReference, bool>> _targets;
        private readonly Dictionary<string, Func<IMotionSourceReference, bool>> _autoTargets;
        private readonly HashSet<string> _enabledAutoTargets;

        protected Atom _personAtom;
        protected Vector3 _position;

        private JSONStorableStringChooser PersonChooser;
        private JSONStorableStringChooser TargetChooser;
        private JSONStorableFloat TargetUpOffsetSlider;
        private JSONStorableFloat TargetForwardOffsetSlider;
        private UIDynamicButton AutoTargetsTitle;
        private List<JSONStorableBool> AutoTargetToggles;

        protected SuperController Controller => SuperController.singleton;

        public Vector3 Position => _position + Up * TargetUpOffsetSlider.val + Forward * TargetForwardOffsetSlider.val;
        public Vector3 Up { get; protected set; }
        public Vector3 Right { get; protected set; }
        public Vector3 Forward { get; protected set; }

        protected abstract string DefaultTarget { get; }

        protected abstract DAZCharacterSelector.Gender TargetGender { get; }

        protected AbstractPersonTarget()
        {
            _targets = new Dictionary<string, Func<IMotionSourceReference, bool>>();
            _autoTargets = new Dictionary<string, Func<IMotionSourceReference, bool>>();
            _enabledAutoTargets = new HashSet<string>();

            RegisterTarget("Auto",  UpdateAutoTarget);
            RegisterTarget("Anus",  UpdateAnusTarget);
            RegisterTarget("Mouth",  UpdateMouthTarget);
            RegisterTarget("Left Hand",  UpdateLeftHandTarget);
            RegisterTarget("Right Hand",  UpdateRightHandTarget);
            RegisterTarget("Left Foot",  UpdateLeftFootTarget);
            RegisterTarget("Right Foot",  UpdateRightFootTarget);
            RegisterTarget("Feet", UpdateFeetTarget);

            RegisterAutoTarget("Mouth");
            RegisterAutoTarget("Left Hand");
            RegisterAutoTarget("Right Hand");
        }

        protected void RegisterTarget(string key, Func<IMotionSourceReference, bool> updater) => _targets.Add(key, updater);

        protected void RegisterAutoTarget(string key, bool enabled = true) => RegisterAutoTarget(key, _targets[key]);
        protected void RegisterAutoTarget(string key, Func<IMotionSourceReference, bool> updater, bool enabled = true)
        {
            _autoTargets.Add(key, updater);

            if (enabled)
                _enabledAutoTargets.Add(key);
        }

        public void CreateUI(IUIBuilder builder)
        {
            var autoGroup = new UIGroup(builder);
            var autoTargetsGroup = new UIGroup(builder);
            var autoTargetsGroupVisible = false;

            AutoTargetToggles = new List<JSONStorableBool>();
            PersonChooser = builder.CreatePopup($"MotionSource:{TargetGender}", $"Select {TargetGender}", null, null, TargetPersonChooserCallback);
            TargetChooser = builder.CreateScrollablePopup($"MotionSource:{TargetGender}Target", "Select Target Point", _targets.Keys.ToList(), DefaultTarget, v =>
            {
                var isAuto = v == "Auto";
                autoGroup.SetVisible(isAuto);
                autoTargetsGroup.SetVisible(isAuto && autoTargetsGroupVisible);
            });

            AutoTargetsTitle = autoGroup.CreateButton("Auto Targets", () => autoTargetsGroup.SetVisible(autoTargetsGroupVisible = !autoTargetsGroupVisible), Color.white, Color.black);
            foreach(var pair in _autoTargets)
            {
                var toggle = autoTargetsGroup.CreateToggle($"MotionSource:{TargetGender}EnabledAutoTargets:{pair.Key.Replace(" ", "")}", $"Enable {pair.Key}", _enabledAutoTargets.Contains(pair.Key), v =>
                {
                    if (v) _enabledAutoTargets.Add(pair.Key);
                    else _enabledAutoTargets.Remove(pair.Key);
                }, false);

                AutoTargetToggles.Add(toggle);
            }

            autoTargetsGroup.SetVisible(false);
            autoGroup.SetVisible(false);

            TargetUpOffsetSlider = builder.CreateSlider("MotionSource:TargetUpOffset", "Target Offset (cm)", 0.0f, -0.15f, 0.15f, false, true, valueFormat: "P2");
            TargetForwardOffsetSlider = builder.CreateSlider("MotionSource:TargetForwardOffset", "Target Forward Offset", 0.0f, -0.05f, 0.05f, false, true, valueFormat: "P2");

            FindTargets();
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(PersonChooser);
            builder.Destroy(TargetChooser);
            builder.Destroy(TargetUpOffsetSlider);
            builder.Destroy(TargetForwardOffsetSlider);
            builder.Destroy(AutoTargetsTitle);

            foreach (var toggle in AutoTargetToggles)
                builder.Destroy(toggle);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(PersonChooser);
            config.Store(TargetChooser);
            config.Store(TargetUpOffsetSlider);
            config.Store(TargetForwardOffsetSlider);

            foreach (var toggle in AutoTargetToggles)
                config.Store(toggle);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(PersonChooser);
            config.Restore(TargetChooser);
            config.Restore(TargetUpOffsetSlider);
            config.Restore(TargetForwardOffsetSlider);

            foreach (var toggle in AutoTargetToggles)
                config.Restore(toggle);

            FindTargets(PersonChooser.val);
        }

        public bool Update(IMotionSourceReference reference)
        {
            if (_personAtom == null || !_personAtom.on)
                return false;

            return _targets.ContainsKey(TargetChooser.val) && _targets[TargetChooser.val](reference);
        }

        private bool UpdateAutoTarget(IMotionSourceReference reference)
        {
            var bestPick = default(Func<IMotionSourceReference, bool>);
            var bestDistance = float.MaxValue;

            foreach (var targetName in _enabledAutoTargets)
            {
                var target = _autoTargets[targetName];
                if (target(reference))
                {
                    var distance = Vector3.Distance(reference.Position, Position);
                    if (distance < bestDistance)
                    {
                        bestPick = target;
                        bestDistance = distance;
                    }
                }
            }

            if (bestPick == null)
                return false;

            return bestPick(reference);
        }

        private bool UpdateAnusTarget(IMotionSourceReference reference)
        {
            var anusLeftName = TargetGender == DAZCharacterSelector.Gender.Female ? "_JointAl" : "_JointAlMale";
            var anusRightName = TargetGender == DAZCharacterSelector.Gender.Female ? "_JointAr" : "_JointArMale";

            var anusLeft = _personAtom.GetComponentByName<CapsuleCollider>(anusLeftName);
            var anusRight = _personAtom.GetComponentByName<CapsuleCollider>(anusRightName);

            if (anusLeft == null || anusRight == null)
                return false;

            Up = ((anusLeft.transform.up + anusRight.transform.up) / 2).normalized;
            Right = ((anusLeft.transform.right + anusRight.transform.right) / 2).normalized;
            Forward = ((anusLeft.transform.forward + anusRight.transform.forward) / 2).normalized;
            _position = (anusLeft.transform.position + anusRight.transform.position) / 2;

            return true;
        }

        private bool UpdateMouthTarget(IMotionSourceReference reference)
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

        private bool UpdateLeftHandTarget(IMotionSourceReference reference) => UpdateHandTarget("l", reference);

        private bool UpdateRightHandTarget(IMotionSourceReference reference) => UpdateHandTarget("r", reference);

        private bool UpdateHandTarget(string side, IMotionSourceReference reference)
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

        private bool UpdateLeftFootTarget(IMotionSourceReference reference) => UpdateFootTarget("l", reference);

        private bool UpdateRightFootTarget(IMotionSourceReference reference) => UpdateFootTarget("r", reference);

        private bool UpdateFootTarget(string side, IMotionSourceReference reference)
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

        private bool UpdateFeetTarget(IMotionSourceReference reference)
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
