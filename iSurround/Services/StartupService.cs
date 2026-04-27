using Microsoft.Win32;
using System.Windows.Forms;

namespace iSurround.Services
{
    public static class StartupService
    {
        private const string RegKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "iSurround";

        public static void SetAutoStart(bool enable)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RegKeyPath, true))
            {
                if (enable)
                    key?.SetValue(AppName, Application.ExecutablePath);
                else
                    key?.DeleteValue(AppName, false);
            }
        }

        public static bool GetAutoStart()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RegKeyPath))
            {
                return key?.GetValue(AppName) != null;
            }
        }
    }
}