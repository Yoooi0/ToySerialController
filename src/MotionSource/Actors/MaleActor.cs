using SimpleJSON;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class MaleActor : IMotionSourceActor
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
        private JSONStorableFloat PenisBaseOffset;

        private SuperController Controller => SuperController.singleton;

        public Vector3 ReferencePosition => _penisPosition;
        public Vector3 ReferenceUp => _penisUp;
        public Vector3 ReferenceRight => _penisRight;
        public Vector3 ReferenceForward => _penisForward;
        public float ReferenceLength => _penisLength;
        public float ReferenceRadius => _penisRadius;
        public Vector3 ReferencePlaneNormal => _planeNormal;

        public void CreateUI(IUIBuilder builder)
        {
            MaleChooser = builder.CreatePopup("MotionSource:Male", "Select Male", null, null, MaleChooserCallback);
            PenisBaseOffset = builder.CreateSlider("MotionSource:PenisBaseOffset", "Penis base offset", 0, -0.05f, 0.05f, true, true);

            FindMales();
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(MaleChooser);
            builder.Destroy(PenisBaseOffset);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(MaleChooser);
            config.Store(PenisBaseOffset);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(MaleChooser);
            config.Restore(PenisBaseOffset);

            FindMales(MaleChooser.val);
        }


        public bool Update() => UpdateMale();

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
            var gen1Position = gen1Transform.position - gen1Transform.up * (gen1Collider.height / 2 - gen1Collider.radius + PenisBaseOffset.val);
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

            return true;
        }

        private void FindMales(string defaultUid = null)
        {
            var people = Controller.GetAtoms().Where(a => a.enabled && a.type == "Person");
            var maleUids = people
                .Where(a => a.GetComponentInChildren<DAZCharacterSelector>()?.gender == DAZCharacterSelector.Gender.Male)
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

        public void RefreshButtonCallback()
        {
            FindMales(MaleChooser.val);
        }
    }
}
