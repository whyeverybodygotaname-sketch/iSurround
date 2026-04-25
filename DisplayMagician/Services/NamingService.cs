using DisplayMagicianShared;

namespace DisplayMagician.Services
{
    public static class NamingService
    {
        public static string GenerateAutoName()
        {
            var temp = new ProfileItem();
            temp.CreateProfileFromCurrentDisplaySettings(true);

            if (temp.NVIDIADisplayConfig.MosaicConfig.IsMosaicEnabled && temp.NVIDIADisplayConfig.MosaicConfig.MosaicGridCount > 0)
                return "NV Surround";

            if (temp.AMDDisplayConfig.SlsConfig.IsSlsEnabled)
                return "AMD Infinity Eye";

            int displayCount = 1;
            if (temp.WindowsDisplayConfig.DisplayConfigPaths != null)
                displayCount = temp.WindowsDisplayConfig.DisplayConfigPaths.Length;
            return $"{displayCount}xScreen";
        }
    }
}