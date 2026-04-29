using Newtonsoft.Json;
using System.IO;
using System.Text;
using iSurroundShared;

namespace iSurround.Models
{
    public class ProgramSettings
    {
        private static readonly string SettingsFilePath = iSurroundShared.SharedConfig.SettingsFilePath;

        public bool MinimiseOnStart { get; set; } = false;
        public bool StartOnBootUp { get; set; } = false;

        public static ProgramSettings LoadSettings()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsFilePath, Encoding.Unicode);
                    return JsonConvert.DeserializeObject<ProgramSettings>(json) ?? new ProgramSettings();
                }
                catch
                {
                    return new ProgramSettings();
                }
            }
            return new ProgramSettings();
        }

        public void SaveSettings()
        {
            try
            {
                string dir = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json, Encoding.Unicode);
            }
            catch { }
        }
    }
}