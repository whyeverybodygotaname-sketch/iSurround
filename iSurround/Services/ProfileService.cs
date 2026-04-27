using iSurroundShared;
using System;
using System.Linq;
using System.Windows.Forms;

namespace iSurround.Services
{
    public static class ProfileService
    {
        public static string MakeUniqueName(string baseName)
        {
            string candidate = baseName;
            int counter = 1;
            while (ProfileRepository.AllProfiles.Any(p => p.Name.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
            {
                candidate = $"{baseName} {++counter}";
            }
            return candidate;
        }

        public static bool TryAddProfile(ProfileItem profile, out string errorMessage)
        {
            errorMessage = null;
            if (ProfileRepository.AddProfile(profile))
                return true;

            errorMessage = "Failed to save profile. The profile data might be incomplete.";
            return false;
        }

        public static void UpdateProfileHotkey(string profileUuid, Keys hotkey)
        {
            var profile = ProfileRepository.AllProfiles.FirstOrDefault(p => p.UUID == profileUuid);
            if (profile != null)
            {
                profile.Hotkey = hotkey;
                ProfileRepository.SaveProfiles();
            }
        }
    }
}