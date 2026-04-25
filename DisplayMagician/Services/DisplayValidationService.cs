using DisplayMagicianShared;

namespace DisplayMagician.Services
{
    public static class DisplayValidationService
    {
        public static bool IsValidForSave(ProfileItem profile, out bool isSurroundOrEyefinity)
        {
            bool isSurround = profile.NVIDIADisplayConfig.MosaicConfig.IsMosaicEnabled && profile.NVIDIADisplayConfig.MosaicConfig.MosaicGridCount > 0;
            bool isEyefinity = profile.AMDDisplayConfig.SlsConfig.IsSlsEnabled;
            isSurroundOrEyefinity = isSurround || isEyefinity;

            if (isSurroundOrEyefinity)
                return true; // Surround/Eyefinity 模式跳过 IsValid 检查

            return profile.IsValid();
        }
    }
}