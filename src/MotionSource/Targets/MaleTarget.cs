using System.Collections.Generic;

namespace ToySerialController.MotionSource
{
    public class MaleTarget : AbstractPersonTarget
    {
        protected override DAZCharacterSelector.Gender TargetGender => DAZCharacterSelector.Gender.Male;

        protected override IList<string> Targets { get; } = new List<string>
        {
            "Auto", "Anus", "Mouth", "Left Hand", "Right Hand", "Left Foot", "Right Foot", "Feet"
        };

        protected override string DefaultTarget => "Auto";

        public MaleTarget() : base()
        {
            RegisterAutoUpdater("Anus");
        }
    }
}