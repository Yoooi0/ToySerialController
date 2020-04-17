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
        private Transform _penisOrigin;
        private Vector3 _planeNormal;

        private Atom _maleAtom;

        private JSONStorableStringChooser MaleChooser;

        private SuperController Controller => SuperController.singleton;

        public override Vector3 ReferencePosition => _penisOrigin.position;
        public override Vector3 ReferenceUp => _penisOrigin.up;
        public override Vector3 ReferenceRight => _penisOrigin.right;
        public override Vector3 ReferenceForward => _penisOrigin.forward;
        public override float ReferenceLength => _penisLength;
        public override Vector3 ReferencePlaneNormal => _planeNormal;

        public override void CreateUI(UIBuilder builder)
        {
            MaleChooser = builder.CreatePopup("MotionSource:Male", "Select Male", null, null, MaleChooserCallback);
            FindMales();

            base.CreateUI(builder);
        }

        public override void DestroyUI(UIBuilder builder)
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
            _penisOrigin = penisTransforms[0];
            for (int i = 0, j = 1; j < penisTransforms.Count; i = j++)
                _penisLength += Vector3.Distance(penisTransforms[i].position, penisTransforms[j].position);

            //TODO: could this be done better?
            var pelvisRight = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFR3Joint")?.transform;
            var pelvidLeft = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFL3Joint")?.transform;
            var pelvisMid = _maleAtom.GetComponentByName<Transform>("AutoColliderpelvisF1")?.GetComponentByName<Collider>("AutoColliderpelvisF4Joint")?.transform;

            if (pelvisRight == null || pelvidLeft == null || pelvisMid == null)
                _planeNormal = _penisOrigin.up;
            else
                _planeNormal = Vector3.Cross(pelvisMid.position - pelvidLeft.position, pelvisMid.position - pelvisRight.position).normalized;

            DebugDraw.Draw();
            DebugDraw.DrawSquare(_penisOrigin.position, _planeNormal, Color.white, 0.33f);
            DebugDraw.DrawTransform(_penisOrigin, 0.15f);
            DebugDraw.DrawLine(ReferencePosition, ReferencePosition + ReferenceUp * ReferenceLength, Color.white);
            DebugDraw.DrawLine(ReferencePosition, TargetPosition, Color.yellow);
            DebugDraw.DrawLine(TargetPosition, TargetPosition + TargetNormal, Color.blue);

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
