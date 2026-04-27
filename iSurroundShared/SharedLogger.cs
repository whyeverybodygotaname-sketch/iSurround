using System;
using System.IO;

namespace iSurroundShared
{
    public class SharedLogger
    {

        internal static string AppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "iSurround");
        internal static string AppLogPath = Path.Combine(AppDataPath, $"Logs");
        internal static string AppSettingsFile = Path.Combine(AppDataPath, $"Settings_1.0.json");
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Provides a way of passing the NLog Logger instance to the iSurround.Shared library so we log to a single log file.
        /// </summary>
        /// <param name="parentLogger"></param>
        public SharedLogger(NLog.Logger parentLogger)
        {
            SharedLogger.logger = parentLogger;
        }
    }
}
