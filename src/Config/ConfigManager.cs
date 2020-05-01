using MVR.FileManagementSecure;
using SimpleJSON;
using System.Linq;
using System.Runtime.InteropServices;

namespace ToySerialController.Config
{
    public static class ConfigManager
    {
        private static SuperController Controller => SuperController.singleton;

        private static bool CheckPath(ref string path)
        {
            if (path == string.Empty)
                return false;

            var parts = path.Split('.');
            if (parts.Length == 1)
                path += ".json";

            if (path?.EndsWith(".json") == false || parts.Last() == string.Empty) {
                SuperController.LogError($"Invalid config file path! \"{path}\"");
                return false;
            }

            return true;
        }

        public static void SaveConfig(string path, IConfigProvider provider)
        {
            if (!CheckPath(ref path))
                return;

            var config = new JSONClass();
            provider.StoreConfig(config);
            Controller.SaveJSON(config, path);

            SuperController.LogMessage($"Saved config! \"{path}\"");
        }

        public static void LoadConfig(string path, IConfigProvider provider)
        {
            if (!CheckPath(ref path))
                return;

            var config = Controller.LoadJSON(path);
            provider.RestoreConfig(config);

            SuperController.LogMessage($"Loaded config! \"{path}\"");
        }

        private static void OpenDialog(string defaultPath, string filter, bool textEntry, uFileBrowser.FileBrowserCallback callback)
        {
            if(!FileManagerSecure.DirectoryExists(defaultPath))
                MakeSureDirectoryPathExists(defaultPath);

            Controller.mediaFileBrowserUI.fileRemovePrefix = null;
            Controller.mediaFileBrowserUI.hideExtension = false;
            Controller.mediaFileBrowserUI.keepOpen = false;
            Controller.mediaFileBrowserUI.showDirs = true;
            Controller.mediaFileBrowserUI.shortCuts = null;
            Controller.mediaFileBrowserUI.browseVarFilesAsDirectories = true;

            Controller.mediaFileBrowserUI.fileFormat = filter;
            Controller.mediaFileBrowserUI.defaultPath = defaultPath;
            Controller.mediaFileBrowserUI.SetTextEntry(textEntry);
            Controller.mediaFileBrowserUI.Show(callback);
        }

        public static void OpenSaveDialog(uFileBrowser.FileBrowserCallback callback) => OpenDialog(Plugin.PluginDir, "json", true, callback);
        public static void OpenLoadDialog(uFileBrowser.FileBrowserCallback callback) => OpenDialog(Plugin.PluginDir, "json", false, callback);

        [DllImport("dbgHelp", SetLastError = true)]
        private static extern bool MakeSureDirectoryPathExists(string dirPath);
    }
}
