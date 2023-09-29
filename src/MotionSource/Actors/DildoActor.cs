using SimpleJSON;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class DildoActor : IMotionSourceActor
    {
        private Atom _dildoAtom;

        private float _dildoLength;
        private float _dildoRadius;
        private Vector3 _dildoPosition;
        private Vector3 _dildoUp, _dildoRight, _dildoForward, _dildoPlaneNormal;

        private JSONStorableStringChooser DildoChooser;

        private SuperController Controller => SuperController.singleton;

        public Vector3 ReferencePosition => _dildoPosition;
        public Vector3 ReferenceUp => _dildoUp;
        public Vector3 ReferenceRight => _dildoRight;
        public Vector3 ReferenceForward => _dildoForward;
        public float ReferenceLength => _dildoLength;
        public float ReferenceRadius => _dildoRadius;
        public Vector3 ReferencePlaneNormal => _dildoPlaneNormal;

        public void CreateUI(IUIBuilder builder)
        {
            DildoChooser = builder.CreatePopup("MotionSource:Dildo", "Select Dildo", null, null, DildoChooserCallback);

            FindDildos();
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(DildoChooser);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(DildoChooser);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(DildoChooser);

            FindDildos(DildoChooser.val);
        }

        public bool Update()
        {
            if (_dildoAtom == null || !_dildoAtom.on)
                return false;

            var baseCollider = _dildoAtom.GetComponentByName<Transform>("b1").GetComponentByName<CapsuleCollider>("_Collider1");
            var midCollider = _dildoAtom.GetComponentByName<Transform>("b2").GetComponentByName<CapsuleCollider>("_Collider2");
            var tipCollider = _dildoAtom.GetComponentByName<Transform>("b3").GetComponentByName<CapsuleCollider>("_Collider2");

            if (baseCollider == null || midCollider == null || tipCollider == null)
                return false;

            var basePosition = baseCollider.transform.position - baseCollider.transform.up * baseCollider.radius / 2;
            var midPosition = midCollider.transform.position;
            var tipPosition = tipCollider.transform.position + tipCollider.transform.up * tipCollider.height;

            _dildoPosition = basePosition;
            _dildoLength = Vector3.Distance(basePosition, midPosition) + Vector3.Distance(midPosition, tipPosition);
            _dildoRadius = midCollider.radius;

            _dildoUp = (tipPosition - midPosition).normalized;
            _dildoRight = -baseCollider.transform.right;
            _dildoForward = Vector3.Cross(_dildoUp, _dildoRight);
            _dildoPlaneNormal = baseCollider.transform.up;

            return true;
        }

        private void FindDildos(string defaultUid = null)
        {
            var dildoUids = Controller.GetAtoms()
                .Where(a => a.type == "Dildo")
                .Select(a => a.uid)
                .ToList();

            if (!dildoUids.Contains(defaultUid))
                defaultUid = dildoUids.FirstOrDefault(uid => uid == _dildoAtom?.uid) ?? dildoUids.FirstOrDefault() ?? "None";

            dildoUids.Insert(0, "None");

            DildoChooser.choices = dildoUids;
            DildoChooserCallback(defaultUid);
        }

        protected void DildoChooserCallback(string s)
        {
            _dildoAtom = Controller.GetAtomByUid(s);
            DildoChooser.valNoCallback = _dildoAtom == null ? "None" : s;
        }

        public void RefreshButtonCallback()
        {
            FindDildos(DildoChooser.val);
        }
    }
}
