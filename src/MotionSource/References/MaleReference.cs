using SimpleJSON;
using System.Linq;
using ToySerialController.UI;
using ToySerialController.Utils;
using UnityEngine;

namespace ToySerialController.MotionSource
{
    public class MaleReference : IMotionSourceReference
    {
        private Atom _maleAtom;
        private Vector3 _staticPlaneNormal, _customPlaneTangent;

        private JSONStorableStringChooser MaleChooser;
        private JSONStorableFloat PenisBaseOffset;
        private JSONStorableBool StaticPlaneNormalToggle;
        private JSONStorableFloat StaticPlaneNormalPitchSlider, StaticPlaneNormalYawSlider, StaticPlaneNormalRollSlider;
        private JSONStorableAction SetStaticPlaneNormalFromCurrentAction;
        private UIDynamicButton StaticPlaneNormalSetFromCurrentButton;

        private SuperController Controller => SuperController.singleton;

        public Vector3 Position { get;private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Forward { get; private set; }
        public float Length { get; private set; }
        public float Radius { get; private set; }
        public Vector3 PlaneNormal { get; private set; }
        public Vector3 PlaneTangent { get; private set; }

        public void CreateUI(IUIBuilder builder)
        {
            MaleChooser = builder.CreatePopup("MotionSource:Male", "Select Male", null, null, MaleChooserCallback);
            PenisBaseOffset = builder.CreateSlider("MotionSource:PenisBaseOffset", "Penis base offset", 0, -0.05f, 0.05f, true, true); 
            
            var group = new UIGroup(builder);
            StaticPlaneNormalToggle = builder.CreateToggle("MotionSource:StaticPlaneNormal:Enabled", "Custom plane normal", false, v =>
            {
                group.SetVisible(v);
                SetStaticPlaneNormalFromCurrent();
                UpdateStaticPlaneNormal();
            });

            StaticPlaneNormalSetFromCurrentButton = group.CreateButton("Set From Current", SetStaticPlaneNormalFromCurrent, new Color(0, 0.75f, 1f) * 0.8f, Color.white);
            StaticPlaneNormalPitchSlider = group.CreateSlider("MotionSource:StaticPlaneNormal:Pitch", "Pitch (\u00b0)", 0, 0, 360f, v => UpdateStaticPlaneNormal(), true, true);
            StaticPlaneNormalYawSlider = group.CreateSlider("MotionSource:StaticPlaneNormal:Yaw", "Yaw (\u00b0)", 0, 0f, 360f, v => UpdateStaticPlaneNormal(), true, true);
            StaticPlaneNormalRollSlider = group.CreateSlider("MotionSource:StaticPlaneNormal:Roll", "Roll (\u00b0)", 0, 0, 360f, v => UpdateStaticPlaneNormal(), true, true);
            group.SetVisible(false);

            SetStaticPlaneNormalFromCurrentAction = UIManager.CreateAction("Set StaticPlaneNormal From Current", SetStaticPlaneNormalFromCurrent);

            FindMales();
        }

        public void DestroyUI(IUIBuilder builder)
        {
            builder.Destroy(MaleChooser);
            builder.Destroy(PenisBaseOffset);

            builder.Destroy(StaticPlaneNormalToggle);
            builder.Destroy(StaticPlaneNormalSetFromCurrentButton);
            builder.Destroy(StaticPlaneNormalPitchSlider);
            builder.Destroy(StaticPlaneNormalYawSlider);
            builder.Destroy(StaticPlaneNormalRollSlider);

            UIManager.RemoveAction(SetStaticPlaneNormalFromCurrentAction);
        }

        public void StoreConfig(JSONNode config)
        {
            config.Store(MaleChooser);
            config.Store(PenisBaseOffset);

            config.Store(StaticPlaneNormalToggle);
            config.Store(StaticPlaneNormalPitchSlider);
            config.Store(StaticPlaneNormalYawSlider);
            config.Store(StaticPlaneNormalRollSlider);
        }

        public void RestoreConfig(JSONNode config)
        {
            config.Restore(MaleChooser);
            config.Restore(PenisBaseOffset);

            config.Restore(StaticPlaneNormalToggle);
            config.Restore(StaticPlaneNormalPitchSlider);
            config.Restore(StaticPlaneNormalYawSlider);
            config.Restore(StaticPlaneNormalRollSlider);

            FindMales(MaleChooser.val);
        }

        public bool Update()
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
            var gen1Position = gen1Transform.position - gen1Transform.up * (gen1Collider.height / 2 - gen1Collider.radius - PenisBaseOffset.val);
            var gen2Position = gen2Collider.transform.position;
            var gen3aPosition = gen3aCollider.transform.position;
            var gen3bPosition = gen3bCollider.transform.position + gen3bCollider.transform.right * gen3bCollider.radius;

            Up = gen1Transform.up;
            Right = -gen1Transform.forward;
            Forward = gen1Transform.right;
            Position = gen1Position;
            Radius = gen2Collider.radius;
            Length = Vector3.Distance(gen1Position, gen2Position) + Vector3.Distance(gen2Position, gen3aPosition) + Vector3.Distance(gen3aPosition, gen3bPosition);

            var pelvisRight = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFR3Joint")?.transform;
            var pelvidLeft = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFL3Joint")?.transform;
            var pelvisMid = _maleAtom.GetComponentByName<Transform>("AutoColliderpelvisF1")?.GetComponentByName<Collider>("AutoColliderpelvisF4Joint")?.transform;

            if (StaticPlaneNormalToggle.val)
            {
                PlaneNormal = _staticPlaneNormal;
                PlaneTangent = _customPlaneTangent;
            }
            else if (pelvisRight == null || pelvidLeft == null || pelvisMid == null)
            {
                PlaneNormal = Up;
                PlaneTangent = Right;
            }
            else
            {
                PlaneNormal = -Vector3.Cross(pelvisMid.position - pelvidLeft.position, pelvisMid.position - pelvisRight.position).normalized;
                PlaneTangent = Vector3.Cross(PlaneNormal, pelvisMid.position - (pelvidLeft.position + pelvisRight.position) / 2).normalized;
            }

            return true;
        }

        private void UpdateStaticPlaneNormal()
        {
            var planeRotation = Quaternion.Euler(StaticPlaneNormalPitchSlider.val, StaticPlaneNormalYawSlider.val, StaticPlaneNormalRollSlider.val);

            _staticPlaneNormal = planeRotation * Vector3.up;
            _customPlaneTangent = planeRotation * Vector3.right;
        }

        private void SetStaticPlaneNormalFromCurrent()
        {
            if (_maleAtom == null)
                return;

            var pelvisRight = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFR3Joint")?.transform;
            var pelvidLeft = _maleAtom.GetComponentByName<Collider>("AutoColliderpelvisFL3Joint")?.transform;
            var pelvisMid = _maleAtom.GetComponentByName<Transform>("AutoColliderpelvisF1")?.GetComponentByName<Collider>("AutoColliderpelvisF4Joint")?.transform;

            if (pelvisRight == null || pelvidLeft == null || pelvisMid == null)
                return;

            var normal = -Vector3.Cross(pelvisMid.position - pelvidLeft.position, pelvisMid.position - pelvisRight.position).normalized;
            var tangent = Vector3.Cross(normal, pelvisMid.position - (pelvidLeft.position + pelvisRight.position) / 2).normalized;
            var angles = Quaternion.LookRotation(Vector3.Cross(normal, tangent), normal).eulerAngles;

            StaticPlaneNormalPitchSlider.valNoCallback = angles.x;
            StaticPlaneNormalYawSlider.valNoCallback = angles.y;
            StaticPlaneNormalRollSlider.valNoCallback = angles.z;

            _staticPlaneNormal = normal;
            _customPlaneTangent = tangent;
        }

        private void FindMales(string defaultUid = null)
        {
            var people = Controller.GetAtoms().Where(a => a.type == "Person");

            var penisColliderNames = new[] { "AutoColliderGen1Hard", "AutoColliderGen2Hard", "AutoColliderGen3aHard", "AutoColliderGen3bHard" };
            var maleUids = people
                .Where(a => {
                    var gender = a.GetComponentInChildren<DAZCharacterSelector>()?.gender;
                    if (gender == null)
                        return false;
                    if (gender == DAZCharacterSelector.Gender.Male)
                        return true;

                    return a.GetComponentsInChildren<Collider>()
                            .Count(c => penisColliderNames.Contains(c.name)) == penisColliderNames.Length;
                })
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

        public void Refresh() => FindMales(MaleChooser.val);
    }
}
