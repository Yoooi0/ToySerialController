using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;
using DebugUtils;
using System;
using Leap.Unity.Infix;
using Leap.Unity;

namespace ToySerialController.MotionSource
{
    public abstract class AbstractFemaleMotionSource : AbstractRefreshableMotionSource
    {
        private Atom _femaleAtom;
        private Vector3 _targetPosition;
        private Vector3 _targetUp;
        private Vector3 _targetRight;
        private Vector3 _targetForward;

        private JSONStorableStringChooser FemaleChooser;
        private JSONStorableStringChooser TargetChooser;

        private SuperController Controller => SuperController.singleton;

        public override Vector3 TargetPosition => _targetPosition;
        public override Vector3 TargetUp => _targetUp;
        public override Vector3 TargetRight => _targetRight;
        public override Vector3 TargetForward => _targetForward;

        public override void CreateUI(IUIBuilder builder)
        {
            var targets = new List<string> { "Auto", "Vagina", "Pelvis", "Hips", "Anus", "Mouth", "Left Hand", "Right Hand", "Chest", "Left Foot", "Right Foot", "Feet" };
            var defaultTarget = "Vagina";

            FemaleChooser = builder.CreatePopup("MotionSource:Female", "Select Female", null, null, FemaleChooserCallback);
            TargetChooser = builder.CreateScrollablePopup("MotionSource:FemaleTarget", "Select Target Point", targets, defaultTarget, null);

            base.CreateUI(builder);

            FindFemales();
        }

        public override void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(FemaleChooser);
            builder.Destroy(TargetChooser);

            base.DestroyUI(builder);
        }

        public override void StoreConfig(JSONNode config)
        {
            config.Store(FemaleChooser);
            config.Store(TargetChooser);
        }

        public override void RestoreConfig(JSONNode config)
        {
            config.Restore(FemaleChooser);
            config.Restore(TargetChooser);

            FindFemales(FemaleChooser.val);
        }

        public override bool Update()
        {
            if (_femaleAtom == null || !_femaleAtom.on)
                return false;

            if (UpdateTarget())
            {
                DebugDraw.DrawTransform(TargetPosition, TargetUp, TargetRight, TargetForward, 0.15f);
                return true;
            }

            return false;
        }

        private bool UpdateTarget()
        {
            if (TargetChooser.val == "Auto") return UpdateAutoTarget();
            else if (TargetChooser.val == "Vagina") return UpdateVaginaTarget();
            else if (TargetChooser.val == "Pelvis") return UpdateFreeControllerTarget("pelvisControl");
            else if (TargetChooser.val == "Hips") return UpdateFreeControllerTarget("hipControl");
            else if (TargetChooser.val == "Anus") return UpdateAnusTarget();
            else if (TargetChooser.val == "Mouth") return UpdateMouthTarget();
            else if (TargetChooser.val == "Left Hand") return UpdateLeftHandTarget();
            else if (TargetChooser.val == "Right Hand") return UpdateRightHandTarget();
            else if (TargetChooser.val == "Chest") return UpdateChestTarget();
            else if (TargetChooser.val == "Left Foot") return UpdateLeftFootTarget();
            else if (TargetChooser.val == "Right Foot") return UpdateRightFootTarget();
            else if (TargetChooser.val == "Feet") return UpdateFeetTarget();

            return false;
        }

        private bool UpdateAutoTarget()
        {
            var targets = new List<Func<bool>>
            {
                UpdateVaginaTarget,
                UpdateMouthTarget,
                UpdateLeftHandTarget,
                UpdateRightHandTarget
            };

            var bestPick = targets.First();
            var bestDistance = float.MaxValue;
            foreach (var target in targets)
            {
                if (target())
                {
                    var distance = Vector3.Distance(ReferencePosition, TargetPosition);
                    if (distance < bestDistance)
                    {
                        bestPick = target;
                        bestDistance = distance;
                    }
                }
            }

            return bestPick();
        }

        private bool UpdateVaginaTarget()
        {
            var labiaTrigger = _femaleAtom.GetRigidBodyByName("LabiaTrigger");
            var vaginaTrigger = _femaleAtom.GetRigidBodyByName("VaginaTrigger");
            var positionOffsetCollider = _femaleAtom.GetComponentByName<CapsuleCollider>("_JointB");

            if (labiaTrigger == null || vaginaTrigger == null || positionOffsetCollider == null)
                return false;

            _targetPosition = labiaTrigger.transform.position;
            _targetUp = (vaginaTrigger.transform.position - labiaTrigger.transform.position).normalized;
            _targetRight = vaginaTrigger.transform.right;
            _targetForward = Vector3.Cross(_targetRight, _targetUp);

            var positionOffset = positionOffsetCollider.transform.position
                + positionOffsetCollider.transform.forward * positionOffsetCollider.radius
                - _targetUp * 0.0025f;
            _targetPosition += _targetUp * Vector3.Dot(positionOffset - _targetPosition, _targetUp);

            return true;
        }

        private bool UpdateFreeControllerTarget(string id)
        {
            var control = _femaleAtom.GetStorableByID(id) as FreeControllerV3;
            if (control == null)
                return false;

            var followBody = control.followWhenOffRB;
            var labiaTrigger = _femaleAtom.GetRigidBodyByName("LabiaTrigger");
            var vaginaTrigger = _femaleAtom.GetRigidBodyByName("VaginaTrigger");
            var positionOffsetCollider = _femaleAtom.GetComponentByName<CapsuleCollider>("_JointB");

            if (followBody == null || labiaTrigger == null || vaginaTrigger == null || positionOffsetCollider == null)
                return false;

            var vaginaPosition = labiaTrigger.transform.position;
            var vaginaUp = (vaginaTrigger.transform.position - labiaTrigger.transform.position).normalized;
            var vaginaRight = vaginaTrigger.transform.right;
            var vaginaForward = Vector3.Cross(vaginaRight, vaginaUp);

            var positionOffset = positionOffsetCollider.transform.position
                + positionOffsetCollider.transform.forward * positionOffsetCollider.radius
                - vaginaUp * 0.0025f;
            vaginaPosition += vaginaUp * Vector3.Dot(positionOffset - vaginaPosition, vaginaUp);

            var controlToBody = control.control.position - followBody.position;
            var referenceToVagina = vaginaPosition - ReferencePosition;

            var controlToBodyRotation = Quaternion.Slerp(control.control.rotation, followBody.rotation, 0.5f).ToNormalized();
            var vaginaRotation = Quaternion.LookRotation(vaginaForward, vaginaUp);
            var rotation = Quaternion.Slerp(vaginaRotation, controlToBodyRotation, 0.125f).ToNormalized();

            _targetUp = rotation.GetUp();
            _targetRight = rotation.GetRight();
            _targetForward = rotation.GetForward();

            var maxOffsetDistance = Mathf.Min(0.05f, referenceToVagina.magnitude);
            var maxControlDistance = 0.25f;

            var controlToBodyUp = Vector3.Project(controlToBody, vaginaUp);
            var controlT = Mathf.Clamp01(controlToBodyUp.magnitude / maxControlDistance);
            var strength = controlT < 0.5 ? 2 * controlT * controlT : -1 + (4 - 2 * controlT) * controlT;
            var sign = -Mathf.Sign(Vector3.Dot(vaginaUp, controlToBodyUp));
            var offsetN = referenceToVagina.normalized;
            var offset = offsetN * sign * maxOffsetDistance * strength;

            _targetPosition = vaginaPosition - offset;
            return true;
        }

        private bool UpdateAnusTarget()
        {
            var anusLeft = _femaleAtom.GetComponentByName<CapsuleCollider>("_JointAl");
            var anusRight = _femaleAtom.GetComponentByName<CapsuleCollider>("_JointAr");

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
            var bottomLip = _femaleAtom.GetComponentByName<Transform>("lowerJawStandardColliders")?.GetComponentByName<CapsuleCollider>("_ColliderLipM");
            var topLip = _femaleAtom.GetComponentByName<Transform>("AutoCollidersTongueUpperLip")?.GetComponentByName<Transform>("AutoColliderAutoCollidersFaceCentral2Hard");
            var mouthTrigger = _femaleAtom.GetRigidBodyByName("MouthTrigger");

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
            var carpal = _femaleAtom.GetRigidBodyByName($"{side}Carpal2");
            var fingerBase = carpal?.GetComponentByName<CapsuleCollider>("_Collider3");
            var fingerTip = _femaleAtom.GetRigidBodyByName($"{side}Pinky3").GetComponentInChildren<CapsuleCollider>();

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

        private bool UpdateChestTarget()
        {
            var left = _femaleAtom.GetComponentByName<AutoCollider>("AutoColliderFemaleAutoColliderslNipple1").jointCollider as CapsuleCollider;
            var right = _femaleAtom.GetComponentByName<AutoCollider>("AutoColliderFemaleAutoCollidersrNipple1").jointCollider as CapsuleCollider;
            var chest = _femaleAtom.GetComponentByName<AutoCollider>("AutoColliderFemaleAutoColliderschest2c").jointCollider as CapsuleCollider;

            if (left == null || right == null || chest == null)
                return false;

            var leftPosition = left.transform.position;
            var rightPosition = right.transform.position;
            var chestPosition = chest.transform.position + _targetForward * chest.radius;

            _targetRight = (rightPosition - leftPosition).normalized;
            _targetForward = chest.transform.forward;
            _targetUp = Vector3.Cross(_targetForward, _targetRight);

            _targetPosition = Vector3.Lerp(chestPosition, (leftPosition + rightPosition) / 2, 0.3f);

            return true;
        }

        private bool UpdateLeftFootTarget() => UpdateFootTarget("l");

        private bool UpdateRightFootTarget() => UpdateFootTarget("r");

        private bool UpdateFootTarget(string side)
        {
            var foot = _femaleAtom.GetRigidBodyByName($"{side}Foot");
            var footBase = foot?.GetComponentByName<CapsuleCollider>("_Collider6");

            if (foot == null || footBase == null)
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
            var leftFoot = _femaleAtom.GetRigidBodyByName("lFoot");
            var rightFoot = _femaleAtom.GetRigidBodyByName("rFoot");
            var leftFootBase = leftFoot?.GetComponentByName<CapsuleCollider>("_Collider6");
            var rightFootBase = rightFoot?.GetComponentByName<CapsuleCollider>("_Collider6");

            if (leftFoot == null || rightFoot == null || leftFootBase == null || rightFootBase == null)
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

        private void FindFemales(string defaultUid = null)
        {
            var people = Controller.GetAtoms().Where(a => a.type == "Person");
            var femaleUids = people
                .Where(a => a.GetComponentInChildren<DAZCharacterSelector>().gender == DAZCharacterSelector.Gender.Female)
                .Select(a => a.uid)
                .ToList();

            if (!femaleUids.Contains(defaultUid))
                defaultUid = femaleUids.FirstOrDefault(uid => uid == _femaleAtom?.uid) ?? femaleUids.FirstOrDefault() ?? "None";

            femaleUids.Insert(0, "None");

            FemaleChooser.choices = femaleUids;
            FemaleChooserCallback(defaultUid);
        }

        protected void FemaleChooserCallback(string s)
        {
            _femaleAtom = Controller.GetAtomByUid(s);
            FemaleChooser.valNoCallback = _femaleAtom == null ? "None" : s;
        }

        protected override void RefreshButtonCallback() => FindFemales(FemaleChooser.val);
    }
}