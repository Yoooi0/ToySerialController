
namespace ToySerialController.MotionSource
{
    public class AssetTarget : AbstractAssetMotion, IMotionSourceTarget
    {
        public bool Update(IMotionSourceReference reference) => Update();
    }
}