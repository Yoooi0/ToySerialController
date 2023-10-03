namespace ToySerialController.MotionSource
{
    public class MaleTarget : AbstractPersonTarget
    {
        protected override DAZCharacterSelector.Gender TargetGender => DAZCharacterSelector.Gender.Male;

        protected override string DefaultTarget => "Auto";

        public MaleTarget()
        {
            RegisterAutoTarget("Anus");
        }
    }
}