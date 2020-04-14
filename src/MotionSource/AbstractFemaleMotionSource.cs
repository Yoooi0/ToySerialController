using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public abstract class AbstractFemaleMotionSource : AbstractRefreshableMotionSource
    {
        private Atom _femaleAtom;
        private Vector3 _targetPosition;
        private Vector3 _targetNormal;

        private JSONStorableStringChooser FemaleChooser;
        private JSONStorableStringChooser TargetChooser;

        private SuperController Controller => SuperController.singleton;

        public override Vector3 TargetPosition => _targetPosition;
        public override Vector3 TargetNormal => _targetNormal;

        public override void CreateUI(UIBuilder builder)
        {
            var targets = new List<string> { "Vagina", "Anus", "Mouth", "Left Hand", "Right Hand" };
            var defaultTarget = targets.First();

            FemaleChooser = builder.CreatePopup("MotionSource:Female", "Select Female", null, null, FemaleChooserCallback);
            TargetChooser = builder.CreateScrollablePopup("MotionSource:FemaleTarget", "Select Target Point", targets, defaultTarget, null);

            FindFemales();

            base.CreateUI(builder);
        }

        public override void DestroyUI(UIBuilder builder)
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

                if (labiaTrigger == null || vaginaTrigger == null)
                    return false;

                _targetPosition = labiaTrigger.position;
                _targetNormal = (labiaTrigger.position - vaginaTrigger.position).normalized;

                // transform to correct labia position
                var positionOffset = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJointtopblock0")?.transform;
                if (positionOffset != null)
                    _targetPosition += TargetNormal * Vector3.Dot(positionOffset.position - TargetPosition, TargetNormal);
            }
            else if (TargetChooser.val == "Anus")
            {
                var bottom = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJointan9")?.transform;
                var top0 = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJointantopsides0")?.transform;
                var top1 = _femaleAtom.GetComponentByName<Collider>("PhysicsMeshJointantopsides1")?.transform;

                if (bottom == null || top0 == null || top1 == null)
                    return false;

                _targetNormal = Vector3.Cross(top0.position - bottom.position, top1.position - bottom.position).normalized;
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
                _targetNormal = (center - mouthTrigger.position).normalized;
                _targetPosition = center - TargetNormal * Vector3.Distance(center, mouthTrigger.position) * 0.2f;

                DebugDraw.DrawCircle(TargetPosition, TargetNormal, Color.cyan, 0.03f);
                DebugDraw.DrawLine(mouthTrigger.position, mouthTrigger.position + mouthTrigger.forward, Color.red);
            }
            else if (TargetChooser.val.Contains("Hand"))
            {
                var side = TargetChooser.val.Contains("Left") ? "l" : "r";
                var hand = _femaleAtom.GetRigidBodyByName(side + "Hand")?.transform;
                var finger = _femaleAtom.GetRigidBodyByName(side + "Pinky1")?.transform;

                if (hand == null || finger == null)
                    return false;

                _targetNormal = hand.forward;
                _targetPosition = finger.position - hand.up * 0.03f;
            }
            else
            {
                return false;
            }

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