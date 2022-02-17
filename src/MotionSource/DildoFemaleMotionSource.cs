using SimpleJSON;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;
using DebugUtils;

namespace ToySerialController.MotionSource
{
    public class DildoFemaleMotionSource : AbstractFemaleMotionSource
    {
        private Atom _dildoAtom;

        private float _dildoLength;
        private float _dildoRadius;
        private Vector3 _dildoPosition;
        private Vector3 _dildoUp, _dildoRight, _dildoForward, _dildoPlaneNormal;

        private JSONStorableStringChooser DildoChooser;

        private SuperController Controller => SuperController.singleton;

        public override Vector3 ReferencePosition => _dildoPosition;
        public override Vector3 ReferenceUp => _dildoUp;
        public override Vector3 ReferenceRight => _dildoRight;
        public override Vector3 ReferenceForward => _dildoForward;
        public override float ReferenceLength => _dildoLength;
        public override float ReferenceRadius => _dildoRadius;
        public override Vector3 ReferencePlaneNormal => _dildoPlaneNormal;

        public override void CreateUI(IUIBuilder builder)
        {
            DildoChooser = builder.CreatePopup("MotionSource:Dildo", "Select Dildo", null, null, DildoChooserCallback);

            base.CreateUI(builder);

            FindDildos();
        }

        public override void DestroyUI(IUIBuilder builder)
        {
            base.DestroyUI(builder);
            builder.Destroy(DildoChooser);
        }

        public override void StoreConfig(JSONNode config)
        {
            base.StoreConfig(config);
            config.Store(DildoChooser);
        }

        public override void RestoreConfig(JSONNode config)
        {
            base.RestoreConfig(config);
            config.Restore(DildoChooser);

            FindDildos(DildoChooser.val);
        }

        public override bool Update() => UpdateDildo() && base.Update();

        private bool UpdateDildo()
        {
            if (_dildoAtom == null)
                return false;

            var baseCollider = _dildoAtom.GetComponentByName<Transform>("b1").GetComponentByName<CapsuleCollider>("_Collider1");
            var midCollider = _dildoAtom.GetComponentByName<Transform>("b2").GetComponentByName<CapsuleCollider>("_Collider2");
            var tipCollider = _dildoAtom.GetComponentByName<Transform>("b3").GetComponentByName<CapsuleCollider>("_Collider2");

            if (baseCollider == null || midCollider == null || tipCollider == null)
                return false;

            var basePosition = baseCollider.transform.position - baseCollider.transform.up * baseCollider.radius;
            var midPosition = midCollider.transform.position;
            var tipPosition = tipCollider.transform.position + tipCollider.transform.up * tipCollider.height;

            _dildoPosition = basePosition;
            _dildoLength = Vector3.Distance(basePosition, midPosition) + Vector3.Distance(midPosition, tipPosition);
            _dildoRadius = midCollider.radius;

            _dildoUp = (tipPosition - midPosition).normalized;
            _dildoRight = -baseCollider.transform.right;
            _dildoForward = Vector3.Cross(_dildoUp, _dildoRight);
            _dildoPlaneNormal = baseCollider.transform.up;

            DebugDraw.DrawTransform(ReferencePosition, ReferenceUp, ReferenceRight, ReferenceForward, 0.15f);
            DebugDraw.DrawRay(ReferencePosition, ReferenceUp, ReferenceLength, Color.white);
            DebugDraw.DrawLine(ReferencePosition, TargetPosition, Color.yellow);

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

        protected override void RefreshButtonCallback()
        {
            base.RefreshButtonCallback();
            FindDildos(DildoChooser.val);
        }
    }
}
