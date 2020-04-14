using SimpleJSON;

namespace ToySerialController.Config
{
    public interface IConfigProvider
    {
        void StoreConfig(JSONNode config);
        void RestoreConfig(JSONNode config);
    }
}
