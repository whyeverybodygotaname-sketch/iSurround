using iSurround.Models;

namespace iSurround.Services
{
    public static class SettingsService
    {
        public static ProgramSettings Settings => Program.AppProgramSettings;

        public static void Save() => Settings.SaveSettings();

        public static bool MinimiseOnStart
        {
            get => Settings.MinimiseOnStart;
            set => Settings.MinimiseOnStart = value;
        }

        public static bool StartOnBootUp
        {
            get => Settings.StartOnBootUp;
            set => Settings.StartOnBootUp = value;
        }
    }
}