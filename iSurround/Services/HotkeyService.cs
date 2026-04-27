using iSurroundShared;
using System.Windows.Forms;

namespace iSurround.Services
{
    public static class HotkeyService
    {
        /// <summary>
        /// 检查热键是否与其他配置文件冲突（如果冲突则弹窗并返回 true）
        /// 注意：此方法保持与原 IsHotkeyConflict 完全相同的弹窗行为
        /// </summary>
        public static bool IsHotkeyConflict(ProfileItem currentProfile, Keys hotkey)
        {
            if (hotkey == Keys.None) return false;

            foreach (var profile in ProfileRepository.AllProfiles)
            {
                if (profile == currentProfile) continue;
                if (profile.Hotkey == hotkey)
                {
                    MessageBox.Show(
                        $"热键 {new KeysConverter().ConvertToString(hotkey)} 已经被配置文件 “{profile.Name}” 使用。\n请选择其他组合键。",
                        "热键冲突",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return true;
                }
            }
            return false;
        }
    }
}