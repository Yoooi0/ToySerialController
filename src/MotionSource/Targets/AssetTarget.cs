
namespace ToySerialController.MotionSource
{
    public class AssetTarget : AbstractAssetBase, IMotionSourceTarget
    {
        public bool Update(IMotionSourceReference reference) => Update();
    }
}