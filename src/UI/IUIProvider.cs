namespace ToySerialController.UI
{
    public interface IUIProvider
    {
        void CreateUI(IUIBuilder builder);
        void DestroyUI(IUIBuilder builder);
    }
}
