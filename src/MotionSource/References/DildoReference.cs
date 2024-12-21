using SimpleJSON;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class DildoReference : IMotionSourceReference
    {
        private Atom _dildoAtom;

        private JSONStorableStringChooser DildoChooser;
        private JSONStorableFloat DildoBaseOffset;

        private SuperController Controller => SuperController.singleton;

        public Vector3 Position { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Forward { get; private set; }
        public float Length { get; private set; }
        public float Radius { get; private set; }
        public Vector3 PlaneNormal { get; private set; }
        public Vector3 PlaneTangent { get; private set; }

        public void CreateUI(IUIBuilder builder)
        {
            DildoChooser = builder.CreatePopup("MotionSource:Dildo", "Select Dildo", null, null, DildoChooserCallback);
            DildoBaseOffset = builder.CreateSlider("MotionSource:DildoBaseOffset", "Dildo base offset", 0, -0.05f, 0.05f, true, true);

            FindDildos();
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(DildoChooser);
            builder.Destroy(DildoBaseOffset);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(DildoChooser);
            config.Store(DildoBaseOffset);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(DildoChooser);
            config.Restore(DildoBaseOffset);

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

            var basePosition = baseCollider.transform.position - baseCollider.transform.up * (baseCollider.radius / 2 - DildoBaseOffset.val);
            var midPosition = midCollider.transform.position;
            var tipPosition = tipCollider.transform.position + tipCollider.transform.up * tipCollider.height;

            Position = basePosition;
            Length = Vector3.Distance(basePosition, midPosition) + Vector3.Distance(midPosition, tipPosition);
            Radius = midCollider.radius;

            Up = (tipPosition - midPosition).normalized;
            Right = -baseCollider.transform.right;
            Forward = Vector3.Cross(Up, Right);
            PlaneNormal = _dildoAtom.GetComponentByName<Transform>("object").forward;
            PlaneTangent = _dildoAtom.GetComponentByName<Transform>("object").right;

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

        public void Refresh() => FindDildos(DildoChooser.val);
    }
}
