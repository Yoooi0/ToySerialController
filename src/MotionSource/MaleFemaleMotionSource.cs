using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;
using DebugUtils;

namespace ToySerialController.MotionSource
{
    //TODO: support futa
    public class MaleFemaleMotionSource : AbstractFemaleMotionSource
    {
        private float _penisLength;
        private float _penisRadius;
        private Vector3 _penisUp;
        private Vector3 _penisRight;
        private Vector3 _penisForward;
        private Vector3 _planeNormal;
        private Vector3 _penisPosition;

        private Atom _maleAtom;

        private JSONStorableStringChooser MaleChooser;

        private SuperController Controller => SuperController.singleton;

        public override Vector3 ReferencePosition => _penisPosition;
        public override Vector3 ReferenceUp => _penisUp;
        public override Vector3 ReferenceRight => _penisRight;
        public override Vector3 ReferenceForward => _penisForward;
        public override float ReferenceLength => _penisLength;
        public override float ReferenceRadius => _penisRadius;
        public override Vector3 ReferencePlaneNormal => _planeNormal;

        public override void CreateUI(IUIBuilder builder)
        {
            MaleChooser = builder.CreatePopup("MotionSource:Male", "Select Male", null, null, MaleChooserCallback);

            base.CreateUI(builder);

            FindMales();
        }

        public override void DestroyUI(IUIBuilder builder)
        {
            base.DestroyUI(builder);
            builder.Destroy(MaleChooser);
        }

        public override void StoreConfig(JSONNode config)
        {
            base.StoreConfig(config);
            config.Store(MaleChooser);
        }

        public override void RestoreConfig(JSONNode config)
        {
            base.RestoreConfig(config);
            config.Restore(MaleChooser);

            FindMales(MaleChooser.val);
        }


        public override bool Update() => UpdateMale() && base.Update();

        private bool UpdateMale()
        {
            if (_maleAtom == null || !_maleAtom.on)
                return false;

            var gen1Collider = _maleAtom.GetComponentByName<CapsuleCollider>("AutoColliderGen1Hard");
            var gen2Collider = _maleAtom.GetComponentByName<CapsuleCollider>("AutoColliderGen2Hard");
            var gen3aCollider = _maleAtom.GetComponentByName<CapsuleCollider>("AutoColliderGen3aHard");
            var gen3bCollider = _maleAtom.GetComponentByName<CapsuleCollider>("AutoColliderGen3bHard");

            if (gen1Collider == null || gen2Collider == null || gen3aCollider == null || gen3bCollider == null)
                return false;

            var gen1Transform = gen1Collider.transform;
            var gen1Position = gen1Transform.position - gen1Transform.up * (gen1Collider.height / 2 - gen1Collider.radius);
            var gen2Position = gen2Collider.transform.position;
            var gen3aPosition = gen3aCollider.transform.position;
            var gen3bPosition = gen3bCollider.transform.position + gen3bCollider.transform.right * gen3bCollider.radius;

            _penisUp = gen1Transform.up;
            _penisRight = -gen1Transform.forward;
            _penisForward = gen1Transform.right;
            _penisPosition = gen1Position;
            _penisRadius = gen2Collider.radius;
            _penisLength = Vector3.Distance(gen1Position, gen2Position) + Vector3.Distance(gen2Position, gen3aPosition) + Vector3.Distance(gen3aPosition, gen3bPosition);

            var pelvisRight = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFR3Joint")?.transform;
            var pelvidLeft = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFL3Joint")?.transform;
            var pelvisMid = _maleAtom.GetComponentByName<Transform>("AutoColliderpelvisF1")?.GetComponentByName<Collider>("AutoColliderpelvisF4Joint")?.transform;

            if (pelvisRight == null || pelvidLeft == null || pelvisMid == null)
                _planeNormal = _penisUp;
            else
                _planeNormal = Vector3.Cross(pelvisMid.position - pelvidLeft.position, pelvisMid.position - pelvisRight.position).normalized;

            DebugDraw.DrawSquare(ReferencePosition, ReferencePlaneNormal, ReferenceRight, Color.white, 0.33f);
            DebugDraw.DrawTransform(ReferencePosition, ReferenceUp, ReferenceRight, ReferenceForward, 0.15f);
            DebugDraw.DrawRay(ReferencePosition, ReferenceUp, ReferenceLength, Color.white);
            DebugDraw.DrawLine(ReferencePosition, TargetPosition, Color.yellow);

            return true;
        }

        private void FindMales(string defaultUid = null)
        {
            var people = Controller.GetAtoms().Where(a => a.type == "Person");
            var maleUids = people
                .Where(a => a.GetComponentInChildren<DAZCharacterSelector>().gender == DAZCharacterSelector.Gender.Male)
                .Select(a => a.uid)
                .ToList();

            if (!maleUids.Contains(defaultUid))
                defaultUid = maleUids.FirstOrDefault(uid => uid == _maleAtom?.uid) ?? maleUids.FirstOrDefault() ?? "None";

            maleUids.Insert(0, "None");

            MaleChooser.choices = maleUids;
            MaleChooserCallback(defaultUid);
        }

        protected void MaleChooserCallback(string s)
        {
            _maleAtom = Controller.GetAtomByUid(s);
            MaleChooser.valNoCallback = _maleAtom == null ? "None" : s;
        }

        protected override void RefreshButtonCallback()
        {
            base.RefreshButtonCallback();
            FindMales(MaleChooser.val);
        }
    }
}
