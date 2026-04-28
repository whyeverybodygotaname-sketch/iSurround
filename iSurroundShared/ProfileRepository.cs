using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using iSurroundShared.AMD;
using iSurroundShared.NVIDIA;
using iSurroundShared.Windows;

namespace iSurroundShared
{
    public enum ApplyProfileResult { Successful, Cancelled, Error }

    public struct ProfileFile
    {
        public string ProfileFileVersion;
        public DateTime LastUpdated;
        public List<ProfileItem> Profiles;
    }

    public static class ProfileRepository
    {
        private static List<ProfileItem> _allProfiles = new List<ProfileItem>();
        private static bool _profilesLoaded = false;
        private static ProfileItem _currentProfile;
        private static List<string> _connectedDisplayIdentifiers = new List<string>();

        public static string AppDataPath = Path.Combine(Application.StartupPath, "iSurroundData");
        //public static string AppIconPath = Path.Combine(AppDataPath, "Icons");//不再使用
        //public static string AppiSurroundIconFilename = Path.Combine(AppIconPath, "iSurround.ico");//不再使用
        private static readonly string AppProfileStoragePath = SharedConfig.ProfilesPath;
        private static readonly string _profileStorageJsonFullFileName = Path.Combine(AppProfileStoragePath, "DisplayProfiles.json");

        static ProfileRepository()
        {
            if (!Directory.Exists(AppProfileStoragePath))
                Directory.CreateDirectory(AppProfileStoragePath);
        }

        public static List<ProfileItem> AllProfiles
        {
            get
            {
                if (!_profilesLoaded) LoadProfiles();
                return _allProfiles;
            }
        }

        public static ProfileItem CurrentProfile
        {
            get
            {
                if (_currentProfile == null) UpdateActiveProfile();
                return _currentProfile;
            }
            set => _currentProfile = value;
        }

        // 补充 ProfileItem.cs 中使用的属性
        public static List<string> ConnectedDisplayIdentifiers
        {
            get => _connectedDisplayIdentifiers;
            set => _connectedDisplayIdentifiers = value;
        }

        public static bool ProfilesLoaded => _profilesLoaded;

        public static List<string> GetCurrentDisplayIdentifiers() => GetAllConnectedDisplayIdentifiers();

        public static bool AddProfile(ProfileItem profile)
        {
            if (profile == null) return false;
            _allProfiles.Add(profile);
            //SaveProfileIconToCache(profile);//不再生成图标缓存
            SaveProfiles();
            IsPossibleRefresh();
            return true;
        }

        public static bool RemoveProfile(ProfileItem profile)
        {
            if (profile == null) return false;
            bool removed = _allProfiles.RemoveAll(p => p.UUID == profile.UUID) > 0;
            if (removed)
            {
                SaveProfiles();
                IsPossibleRefresh();
                UpdateActiveProfile();
                //try { if (File.Exists(profile.SavedProfileIconCacheFilename)) File.Delete(profile.SavedProfileIconCacheFilename); } catch { }//不再使用
            }
            return removed;
        }

        public static ProfileItem GetProfile(string nameOrId)
        {
            if (string.IsNullOrEmpty(nameOrId)) return null;
            return _allProfiles.FirstOrDefault(p => p.UUID == nameOrId || p.Name == nameOrId);
        }

        public static void UpdateActiveProfile(bool fastScan = true)
        {
            var current = new ProfileItem();
            current.CreateProfileFromCurrentDisplaySettings(fastScan);
            var matched = _allProfiles.FirstOrDefault(p => p.Equals(current));
            _currentProfile = matched ?? current;
        }

        private static bool LoadProfiles()
        {
            _profilesLoaded = false;
            _allProfiles.Clear();

            if (!File.Exists(_profileStorageJsonFullFileName))
                return true;

            try
            {
                string json = File.ReadAllText(_profileStorageJsonFullFileName, Encoding.Unicode);
                if (string.IsNullOrWhiteSpace(json)) return true;

                var profileFile = JsonConvert.DeserializeObject<ProfileFile>(json);
                _allProfiles = profileFile.Profiles ?? new List<ProfileItem>();

                // 修复 Windows 显示配置的适配器 ID
                foreach (var prof in _allProfiles)
                {
                    var winCfg = prof.WindowsDisplayConfig;
                    WinLibrary.GetLibrary().PatchWindowsDisplayConfig(ref winCfg);
                }
                _allProfiles.Sort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load profiles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            _profilesLoaded = true;
            IsPossibleRefresh();
            return true;
        }

        public static void CopyCurrentLayoutToProfile(ProfileItem profile)
        {
            profile.CreateProfileFromCurrentDisplaySettings(false);
            SaveProfiles();
        }

        public static bool SaveProfiles()
        {
            if (!Directory.Exists(AppProfileStoragePath))
                Directory.CreateDirectory(AppProfileStoragePath);

            _allProfiles.Sort();
            try
            {
                var profileFile = new ProfileFile
                {
                    ProfileFileVersion = "3",
                    LastUpdated = DateTime.Now,
                    Profiles = _allProfiles
                };
                string json = JsonConvert.SerializeObject(profileFile, Formatting.Indented);
                File.WriteAllText(_profileStorageJsonFullFileName, json, Encoding.Unicode);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save profiles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        //不再需要图标缓存的保留
        //private static void SaveProfileIconToCache(ProfileItem profile)
        //{
        //    profile.SavedProfileIconCacheFilename = Path.Combine(AppProfileStoragePath, $"profile-{profile.UUID}.ico");
        //    try
        //    {
        //        var multiIcon = profile.ProfileIcon.ToIcon();
        //        multiIcon.Save(profile.SavedProfileIconCacheFilename, MultiIconFormat.ICO);
        //    }
        //    catch
        //    {
        //        File.Copy(AppiSurroundIconFilename, profile.SavedProfileIconCacheFilename, true);
        //    }
        //}

        public static void IsPossibleRefresh()
        {
            if (!_profilesLoaded || _allProfiles.Count == 0) return;
            _connectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers();
            foreach (var p in _allProfiles) p.RefreshPossbility();
        }

        public static List<string> GetAllConnectedDisplayIdentifiers()
        {
            var ids = new List<string>();
            try
            {
                if (NVIDIALibrary.GetLibrary().IsInstalled) ids.AddRange(NVIDIALibrary.GetLibrary().GetAllConnectedDisplayIdentifiers());
                if (AMDLibrary.GetLibrary().IsInstalled) ids.AddRange(AMDLibrary.GetLibrary().GetAllConnectedDisplayIdentifiers());
                ids.AddRange(WinLibrary.GetLibrary().GetAllConnectedDisplayIdentifiers());
                ids.Sort();
            }
            catch { }
            return ids;
        }

        public static ApplyProfileResult ApplyProfile(ProfileItem profile)
        {
            if (profile == null) return ApplyProfileResult.Error;
            try
            {
                if (!profile.SetActive()) return ApplyProfileResult.Error;
                // 成功应用后，将当前活动配置设为仓库中的对应实例（确保引用正确）
                var repoProfile = _allProfiles.FirstOrDefault(p => p.UUID == profile.UUID);
                _currentProfile = repoProfile ?? profile;
                return ApplyProfileResult.Successful;
            }
            catch { return ApplyProfileResult.Error; }
            finally
            {
                Thread.Sleep(500);
            }
        }
    }
}