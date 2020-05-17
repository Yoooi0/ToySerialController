using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;
using DebugUtils;

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
            var targets = new List<string> { "Vagina", "Anus", "Mouth", "Left Hand", "Right Hand", "Left Foot", "Right Foot", "Feet" };
            var defaultTarget = targets.First();

            FemaleChooser = builder.CreatePopup("MotionSource:Female", "Select Female", null, null, FemaleChooserCallback);
            TargetChooser = builder.CreateScrollablePopup("MotionSource:FemaleTarget", "Select Target Point", targets, defaultTarget, null);

            FindFemales();

            base.CreateUI(builder);
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

            if (_femaleAtom == null)
                FemaleChooser.valNoCallback = "None";
        }

        public override bool Update()
        {
            if (_femaleAtom == null)
                return false;

            if (TargetChooser.val == "Vagina")
            {
                var labiaTrigger = _femaleAtom.GetRigidBodyByName("LabiaTrigger")?.transform;
                var vaginaTrigger = _femaleAtom.GetRigidBodyByName("VaginaTrigger")?.transform;
                var positionOffset = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJointtopblock0")?.transform;

                if (labiaTrigger == null || vaginaTrigger == null || positionOffset == null)
                    return false;

                _targetPosition = labiaTrigger.position;
                _targetUp = (labiaTrigger.position - vaginaTrigger.position).normalized;
                _targetRight = vaginaTrigger.right;
                _targetForward = Vector3.Cross(_targetUp, _targetRight);

                _targetPosition += _targetUp * Vector3.Dot(positionOffset.position - _targetPosition, _targetUp);
            }
            else if (TargetChooser.val == "Anus")
            {
                var bottom = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJointan9")?.transform;
                var top0 = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJointantopsides0")?.transform;
                var top1 = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJointantopsides1")?.transform;

                if (bottom == null || top0 == null || top1 == null)
                    return false;

                _targetUp = -bottom.forward;
                _targetRight = bottom.right;
                _targetForward = -bottom.up;
                _targetPosition = (bottom.position + top0.position + top1.position) / 3;
            }
            else if (TargetChooser.val == "Mouth")
            {
                var bottomLip = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJoint7")?.transform;
                var topLip = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJoint23")?.transform;
                var mouthTrigger = _femaleAtom.GetRigidBodyByName("MouthTrigger")?.transform;

                if (bottomLip == null || topLip == null || mouthTrigger == null)
                    return false;

                var center = (topLip.position + bottomLip.position) / 2;
                _targetUp = (center - mouthTrigger.position).normalized;
                _targetRight = mouthTrigger.right;
                _targetForward = Vector3.Cross(_targetUp, _targetRight);
                _targetPosition = center - TargetUp * Vector3.Distance(center, mouthTrigger.position) * 0.2f;

                DebugDraw.DrawCircle(TargetPosition, TargetUp, Color.gray, 0.03f);
            }
            else if (TargetChooser.val.Contains("Hand"))
            {
                var side = TargetChooser.val.Contains("Left") ? "l" : "r";
                var carpal = _femaleAtom.GetRigidBodyByName($"{side}Carpal2");
                var fingerBase = carpal?.GetComponentByName<CapsuleCollider>($"_Collider3");
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
                else if(side == "r")
                {
                    _targetRight = fingerBase.transform.up;
                    _targetForward = fingerBase.transform.right;
                }

                DebugDraw.DrawLine(fingerBasePosition, fingerTipPosition, Color.gray);
            }
            else if (TargetChooser.val.Contains("Foot"))
            {
                var side = TargetChooser.val.Contains("Left") ? "l" : "r";
                var foot = _femaleAtom.GetRigidBodyByName($"{side}Foot");
                var footBase = foot?.GetComponentByName<CapsuleCollider>($"_Collider6");

                _targetRight = footBase.transform.forward;
                _targetForward = -footBase.transform.up;

                if (side == "l")
                    _targetUp = footBase.transform.right;
                else if (side == "r")
                    _targetUp = -footBase.transform.right;

                _targetPosition = footBase.transform.position + _targetForward * footBase.radius;
            }
            else if (TargetChooser.val == "Feet")
            {
                var leftFoot = _femaleAtom.GetRigidBodyByName($"lFoot");
                var rightFoot = _femaleAtom.GetRigidBodyByName($"rFoot");
                var leftFootBase = leftFoot?.GetComponentByName<CapsuleCollider>($"_Collider6");
                var rightFootBase = rightFoot?.GetComponentByName<CapsuleCollider>($"_Collider6");

                var leftPosition = leftFootBase.transform.position - leftFootBase.transform.up * leftFootBase.radius;
                var rightPosition = rightFootBase.transform.position - rightFootBase.transform.up * rightFootBase.radius;

                _targetPosition = (leftPosition + rightPosition) / 2;
                _targetForward = ((leftFootBase.transform.forward + rightFootBase.transform.forward) / 2).normalized;
                _targetUp = Vector3.Cross((leftPosition - rightPosition).normalized, _targetForward).normalized;
                _targetRight = Vector3.Cross(_targetUp, _targetForward).normalized;

                DebugDraw.DrawCircle(_targetPosition, Quaternion.FromToRotation(Vector3.up, _targetUp), Color.white, (leftPosition - rightPosition).magnitude / 2);
            }
            else
            {
                return false;
            }

            DebugDraw.DrawTransform(TargetPosition, TargetUp, TargetRight, TargetForward, 0.15f);

            return true;
        }

        private void FindFemales()
        {
            var people = Controller.GetAtoms().Where(a => a.type == "Person");
            var femaleUids = people
                .Where(a => a.GetComponentInChildren<DAZCharacterSelector>().gender == DAZCharacterSelector.Gender.Female)
                .Select(a => a.uid)
                .ToList();

            var defaultFemale = femaleUids.FirstOrDefault(uid => uid == _femaleAtom?.uid) ?? femaleUids.FirstOrDefault() ?? "None";
            femaleUids.Insert(0, "None");

            FemaleChooser.choices = femaleUids;
            FemaleChooserCallback(defaultFemale);
        }

        protected void FemaleChooserCallback(string s)
        {
            _femaleAtom = Controller.GetAtomByUid(s);
            FemaleChooser.valNoCallback = _femaleAtom == null ? "None" : s;
        }

        protected override void RefreshButtonCallback() => FindFemales();
    }
}