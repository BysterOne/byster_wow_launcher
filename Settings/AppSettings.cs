﻿using Launcher.Any.GlobalEnums;
using Launcher.Api.Models;
using Launcher.Cls.ModelConverters;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings.Enums;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace Launcher.Settings
{
    public class AppSettings
    {
        public static string RootFolder { get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"); }
        public static string CacheFolder { get => Path.Combine(RootFolder, "cache"); }
        public static string TempBin { get => Path.Combine(RootFolder, "tempBin"); }
        public static string SettingsFilePath { get => Path.Combine(RootFolder, "settings.json"); }

        public static string ProtectedFolder { get => Path.Combine(RootFolder, "default"); }
        public static string UnprotectedFolder { get => Path.Combine(RootFolder, "unpr"); }
        public static BitmapScalingMode GlobalBitmapScalingMode { get; set; } = BitmapScalingMode.HighQuality;

        private static AppSettings? _instance;

        #region События
        public delegate void LanguageChangedDelegate(ELang newLanguage);
        public static event LanguageChangedDelegate? LanguageChanged;
        #endregion

        #region Скрытые
        private ELang _lang = ELang.Ru;
        #endregion

        #region Параметры
        [JsonConverter(typeof(LangAsTextJsonConverter))]
        public ELang Language { get => _lang; set { _lang = value; LanguageChanged?.Invoke(value); } }
        public EServer Server { get; set; } = EServer.Prod;
        public string Branch { get; set; } = "master";

        [JsonConverter(typeof(BoolAsIntJsonConverter))]
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Console { get; set; } = false;

        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? WorkDirectory { get; set; } = null;
        public ObservableCollection<CServer> Servers { get; set; } = [];
        public List<CGitSyncData>? SyncData { get; set; }
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
            var json = JsonConvert.SerializeObject(Instance, GProp.JsonSeriSettings);
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
