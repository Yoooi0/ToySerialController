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
    public class MaleFemaleMotionSource : AbstractFemaleMotionSource
    {
        private float _penisLength;
        private float _penisRadius;
        private Transform _penisOrigin;
        private Vector3 _planeNormal;
        private Vector3 _referencePosition;

        private Atom _maleAtom;

        private JSONStorableStringChooser MaleChooser;

        private SuperController Controller => SuperController.singleton;

        public override Vector3 ReferencePosition => _referencePosition;
        public override Vector3 ReferenceUp => _penisOrigin.up;
        public override Vector3 ReferenceRight => -_penisOrigin.forward;
        public override Vector3 ReferenceForward => _penisOrigin.right;
        public override float ReferenceLength => _penisLength;
        public override float ReferenceRadius => _penisRadius;
        public override Vector3 ReferencePlaneNormal => _planeNormal;

        public override void CreateUI(IUIBuilder builder)
        {
            MaleChooser = builder.CreatePopup("MotionSource:Male", "Select Male", null, null, MaleChooserCallback);
            FindMales();

            base.CreateUI(builder);
        }

        public override void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(MaleChooser);
            base.DestroyUI(builder);
        }

        public override bool Update() => UpdateMale() && base.Update();

        private bool UpdateMale()
        {
            if (_maleAtom == null)
                return false;

            var penisColliders = new List<string> {
                "AutoColliderGen1Hard",
                "AutoColliderGen2Hard",
                "AutoColliderGen3aHard",
                "AutoColliderGen3bHard"
            };

            var penisTransforms = _maleAtom
                .GetComponentsInChildren<Collider>()
                .Where(c => penisColliders.Contains(c.name))
                .OrderBy(c => c.name, StringComparer.OrdinalIgnoreCase)
                .Select(c => c.transform)
                .ToList();

            if (penisTransforms.Count != 4)
                return false;

            _penisLength = 0.0f;
            for (int i = 0, j = 1; j < penisTransforms.Count; i = j++)
                _penisLength += Vector3.Distance(penisTransforms[i].position, penisTransforms[j].position);

            _penisOrigin = penisTransforms[0];

            var positionOffset = -_penisOrigin.up * (penisTransforms[1].position - penisTransforms[0].position).magnitude * 0.15f;
            _referencePosition = _penisOrigin.position + positionOffset;
            _penisLength += positionOffset.magnitude;
            _penisLength += penisTransforms[3].GetComponent<CapsuleCollider>().radius;
            _penisRadius = penisTransforms[1].GetComponent<CapsuleCollider>().radius;

            var pelvisRight = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFR3Joint")?.transform;
            var pelvidLeft = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFL3Joint")?.transform;
            var pelvisMid = _maleAtom.GetComponentByName<Transform>("AutoColliderpelvisF1")?.GetComponentByName<Collider>("AutoColliderpelvisF4Joint")?.transform;

            if (pelvisRight == null || pelvidLeft == null || pelvisMid == null)
                _planeNormal = _penisOrigin.up;
            else
                _planeNormal = Vector3.Cross(pelvisMid.position - pelvidLeft.position, pelvisMid.position - pelvisRight.position).normalized;

            DebugDraw.DrawSquare(ReferencePosition, ReferencePlaneNormal, Color.white, 0.33f);
            DebugDraw.DrawTransform(ReferencePosition, ReferenceUp, ReferenceRight, ReferenceForward, 0.15f);
            DebugDraw.DrawRay(ReferencePosition, ReferenceUp, ReferenceLength, Color.white);
            DebugDraw.DrawLine(ReferencePosition, TargetPosition, Color.yellow);

            return true;
        }

        private void FindMales()
        {
            var people = Controller.GetAtoms().Where(a => a.type == "Person");
            var maleUids = people
                .Where(a => a.GetComponentInChildren<DAZCharacterSelector>().gender == DAZCharacterSelector.Gender.Male)
                .Select(a => a.uid)
                .ToList();

            var defaultMale = maleUids.FirstOrDefault(uid => uid == _maleAtom?.uid) ?? maleUids.FirstOrDefault() ?? "None";
            maleUids.Insert(0, "None");

            MaleChooser.choices = maleUids;
            MaleChooserCallback(defaultMale);
        }

        public override void StoreConfig(JSONNode config)
        {
            config.Store(MaleChooser);
            base.StoreConfig(config);
        }

        public override void RestoreConfig(JSONNode config)
        {
            config.Restore(MaleChooser);
            base.RestoreConfig(config);
        }

        protected void MaleChooserCallback(string s)
        {
            _maleAtom = Controller.GetAtomByUid(s);
            MaleChooser.valNoCallback = _maleAtom == null ? "None" : s;
        }

        protected override void RefreshButtonCallback()
        {
            base.RefreshButtonCallback();
            FindMales();
        }
    }
}
