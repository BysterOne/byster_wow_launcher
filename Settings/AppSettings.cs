using Launcher.Any.GlobalEnums;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

namespace Launcher.Settings
{
    public class AppSettings
    {
        public static string RootFolder { get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "local"); }
        public static string CacheFolder { get => Path.Combine(RootFolder, "cache"); }
        public static string TempBin { get => Path.Combine(RootFolder, "tempBin"); }
        public static string SettingsFilePath { get => Path.Combine(RootFolder, "settings.json"); }        

        private static AppSettings? _instance;

        #region Параметры
        public ELang Language { get; set; } = ELang.Ru;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public ObservableCollection<CServer> Servers { get; set; } = [];
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
            if (!Directory.Exists(RootFolder)) Directory.CreateDirectory(RootFolder);

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
