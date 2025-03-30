using Launcher.Any.GlobalEnums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Settings
{
    public class AppSettings
    {     
        public static readonly string MainFolderPath = "local";
        private static readonly string SettingsFilePath = Path.Combine(MainFolderPath, "settings.json");
        private static AppSettings? _instance;

        #region Параметры
        public ELang Language { get; set; } = ELang.Ru;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;        
        #endregion

        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                    if (_instance == null) return new AppSettings();
                }
                return _instance;
            }
        }

        public static void Save()
        {
            if (!Directory.Exists(MainFolderPath)) Directory.CreateDirectory(MainFolderPath);

            var json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
            File.WriteAllText(SettingsFilePath, json);
        }

        private static AppSettings? Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonConvert.DeserializeObject<AppSettings>(json);
            }
            return new AppSettings();
        }
    }
}
