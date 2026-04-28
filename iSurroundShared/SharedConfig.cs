using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace iSurroundShared
{
    public static class SharedConfig
    {
        public static string AppDataPath { get; private set; }
        public static string LogPath => Path.Combine(AppDataPath, "Logs");

        public static void Initialize(string dataPath)
        {
            AppDataPath = dataPath;
            Directory.CreateDirectory(AppDataPath);
            Directory.CreateDirectory(LogPath);
            ConfigureNLog();
        }

        private static void ConfigureNLog()
        {
            var config = new LoggingConfiguration();
            
            var fileTarget = new FileTarget("logfile")
            {
                FileName = Path.Combine(LogPath, "iSurround ${shortdate}.log"),
                Layout = "${longdate} | ${level:uppercase=true:padding=-5} | ${logger} | ${message} ${exception:format=tostring}",
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 7,
                ConcurrentWrites = true
            };
            
            config.AddTarget(fileTarget);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);
            
            LogManager.Configuration = config;
        }

        public static string ProfilesPath => Path.Combine(AppDataPath, "Profiles");
        public static string SettingsFilePath => Path.Combine(AppDataPath, "Settings.json");
    }
}