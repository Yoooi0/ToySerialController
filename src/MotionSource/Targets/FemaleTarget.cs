using ToySerialController.Utils;
using UnityEngine;
using Leap.Unity.Infix;
using Leap.Unity;

namespace ToySerialController.MotionSource
{
    public class FemaleTarget : AbstractPersonTarget
    {
        protected override DAZCharacterSelector.Gender TargetGender => DAZCharacterSelector.Gender.Female;

        protected override string DefaultTarget => "Vagina";

        public FemaleTarget()
        {
            RegisterTarget("Vagina", UpdateVaginaTarget);
            RegisterTarget("Pelvis", r => UpdateFreeControllerTarget("pelvisControl", r));
            RegisterTarget("Hips", r => UpdateFreeControllerTarget("hipControl", r));
            RegisterTarget("Chest", UpdateChestTarget);

            RegisterAutoTarget("Vagina");
        }

        private bool UpdateVaginaTarget(IMotionSourceReference reference)
        {
            var labiaTrigger = _personAtom.GetRigidBodyByName("LabiaTrigger");
            var vaginaTrigger = _personAtom.GetRigidBodyByName("VaginaTrigger");
            var positionOffsetCollider = _personAtom.GetComponentByName<CapsuleCollider>("_JointB");

            if (labiaTrigger == null || vaginaTrigger == null || positionOffsetCollider == null)
                return false;

            _position = labiaTrigger.transform.position;
            Up = (vaginaTrigger.transform.position - labiaTrigger.transform.position).normalized;
            Right = vaginaTrigger.transform.right;
            Forward = Vector3.Cross(Right, Up);

            var positionOffset = positionOffsetCollider.transform.position
                + positionOffsetCollider.transform.forward * positionOffsetCollider.radius
                - Up * 0.0025f;
            _position += Up * Vector3.Dot(positionOffset - _position, Up);

            return true;
        }

        private bool UpdateFreeControllerTarget(string id, IMotionSourceReference reference)
        {
            var control = _personAtom.GetStorableByID(id) as FreeControllerV3;
            if (control == null)
                return false;

            var followBody = control.followWhenOffRB;
            var labiaTrigger = _personAtom.GetRigidBodyByName("LabiaTrigger");
            var vaginaTrigger = _personAtom.GetRigidBodyByName("VaginaTrigger");
            var positionOffsetCollider = _personAtom.GetComponentByName<CapsuleCollider>("_JointB");

            if (followBody == null || labiaTrigger == null || vaginaTrigger == null || positionOffsetCollider == null)
                return false;

            var vaginaPosition = labiaTrigger.transform.position;
            var vaginaUp = (vaginaTrigger.transform.position - labiaTrigger.transform.position).normalized;
            var vaginaRight = vaginaTrigger.transform.right;
            var vaginaForward = Vector3.Cross(vaginaRight, vaginaUp);

            var positionOffset = positionOffsetCollider.transform.position
                + positionOffsetCollider.transform.forward * positionOffsetCollider.radius
                - vaginaUp * 0.0025f;
            vaginaPosition += vaginaUp * Vector3.Dot(positionOffset - vaginaPosition, vaginaUp);

            var controlToBody = control.control.position - followBody.position;
            var referenceToVagina = vaginaPosition - reference.Position;

            var controlToBodyRotation = Quaternion.Slerp(control.control.rotation, followBody.rotation, 0.5f).ToNormalized();
            var vaginaRotation = Quaternion.LookRotation(vaginaForward, vaginaUp);
            var rotation = Quaternion.Slerp(vaginaRotation, controlToBodyRotation, 0.125f).ToNormalized();

            Up = rotation.GetUp();
            Right = rotation.GetRight();
            Forward = rotation.GetForward();

            var maxOffsetDistance = Mathf.Min(0.05f, referenceToVagina.magnitude);
            var maxControlDistance = 0.25f;

            var controlToBodyUp = Vector3.Project(controlToBody, vaginaUp);
            var controlT = Mathf.Clamp01(controlToBodyUp.magnitude / maxControlDistance);
            var strength = controlT < 0.5 ? 2 * controlT * controlT : -1 + (4 - 2 * controlT) * controlT;
            var sign = -Mathf.Sign(Vector3.Dot(vaginaUp, controlToBodyUp));
            var offsetN = referenceToVagina.normalized;
            var offset = offsetN * sign * maxOffsetDistance * strength;

            _position = vaginaPosition - offset;
            return true;
        }

        private bool UpdateChestTarget(IMotionSourceReference reference)
        {
            var left = _personAtom.GetComponentByName<AutoCollider>("AutoColliderFemaleAutoColliderslNipple1")?.jointCollider as CapsuleCollider;
            var right = _personAtom.GetComponentByName<AutoCollider>("AutoColliderFemaleAutoCollidersrNipple1")?.jointCollider as CapsuleCollider;
            var chest = _personAtom.GetComponentByName<AutoCollider>("AutoColliderFemaleAutoColliderschest2c")?.jointCollider as CapsuleCollider;

            if (left == null || right == null || chest == null)
                return false;

            var leftPosition = left.transform.position;
            var rightPosition = right.transform.position;
            var chestPosition = chest.transform.position + Forward * chest.radius;

            Right = (rightPosition - leftPosition).normalized;
            Forward = chest.transform.forward;
            Up = Vector3.Cross(Forward, Right);

            _position = Vector3.Lerp(chestPosition, (leftPosition + rightPosition) / 2, 0.3f);

            return true;
        }
    }
}