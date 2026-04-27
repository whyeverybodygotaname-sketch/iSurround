using System.IO;

namespace iSurroundShared
{
    public static class SharedConfig
    {
        public static string AppDataPath { get; private set; }

        public static void Initialize(string dataPath)
        {
            AppDataPath = dataPath;
        }

        public static string ProfilesPath => Path.Combine(AppDataPath, "Profiles");
        public static string SettingsFilePath => Path.Combine(AppDataPath, "Settings.json");
    }
}