namespace ToySerialController.UI
{
    public interface IUIProvider
    {
        void CreateUI(UIBuilder builder);
        void DestroyUI(UIBuilder builder);
    }
}
