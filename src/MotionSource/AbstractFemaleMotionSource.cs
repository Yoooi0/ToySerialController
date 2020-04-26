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
            var targets = new List<string> { "Vagina", "Anus", "Mouth", "Left Hand", "Right Hand" };
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
                var finger1 = _femaleAtom.GetRigidBodyByName(side + "Pinky1")?.transform;
                var finger3 = _femaleAtom.GetRigidBodyByName(side + "Pinky3")?.transform; // TODO: should be finger tip

                if (finger1 == null || finger3 == null)
                    return false;

                _targetUp = finger1.forward;
                _targetRight = finger1.right;
                _targetForward = -finger1.up;
                _targetPosition = (finger1.position + finger3.position) / 2 - finger1.up * 0.008f;

                DebugDraw.DrawLine(finger1.position, finger3.position, Color.gray);
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