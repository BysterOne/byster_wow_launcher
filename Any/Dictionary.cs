using Cls;
using Launcher.Any.GlobalEnums;
using Launcher.Settings;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace Launcher
{
    public static class Dictionary
    {
        public static List<JObject> Localizations = new List<JObject>();

        public static void Load()
        {
            var assm_ = Assembly.GetExecutingAssembly();

            var dir = "Launcher.Localizations";
            var filesNames = new List<string>() { "English.json" };
            Log.Add("Загрузка языковых файлов ...");
            foreach (var fileName in filesNames)
            {
                try
                {
                    using (Stream s = assm_.GetManifestResourceStream($"{dir}.{fileName}")!)
                    using (StreamReader stream = new StreamReader(s))
                    {
                        var d_ = JObject.Parse(stream.ReadToEnd());
                        if (d_ == null) Log.Add($"Ошибка загрузки {fileName}. Файл пустой");
                        Localizations.Add(d_);
                    }
                }
                catch (Exception ex) { Log.Add($"Ошибка загрузки {fileName}. Exception: {ex.Message}"); }
            }
            Log.Add($"Загрузка языковых файлов завершена. Загружено {Localizations.Count}/{filesNames.Count}");
        }

        public static string Translate(string key) => Translate(key, AppSettings.Instance.Language);

        public static string Translate(string key, ELang lang)
        {
            string code = "";
            switch (lang)
            {
                case ELang.Ru: return key;
                case ELang.En: code = "enUS"; break;
            }

            var haveloc_ = Localizations.FirstOrDefault(x => x["Language"] != null && x["Language"].ToString() == code);
            if (haveloc_ == null) return "";
            var dv_ = haveloc_["Associations"]![key];
            return dv_ != null ? dv_.ToString() : key;
        }

        public static string GetLanguageName(ELang lang)
        {
            return lang switch
            {
                ELang.Ru => "Русский",
                ELang.En => "English",
                _ => "Unknown"
            };
        }

        #region Колво ротаций
        public static string RotationsCount(int count)
        {
            #region Ru
            string Ru()
            {
                string[] forms = { "ротация", "ротации", "ротаций" };
                int idx = (count % 100 > 4 && count % 100 < 20)
                    ? 2
                    : new[] { 2, 0, 1, 1, 1, 2 }[Math.Min(count % 10, 5)];
                return forms[idx];
            }
            #endregion

            return AppSettings.Instance.Language switch
            {
                ELang.En => $"{count} rotations",
                _ => $"{count} {Ru()}"
            };
        }
        #endregion
        #region Длительность в днях
        public static string DaysCount(int count)
        {
            #region Ru
            string Ru()
            {
                string[] forms = { "день", "дня", "дней" };
                int idx = (count % 100 > 4 && count % 100 < 20)
                    ? 2
                    : new[] { 2, 0, 1, 1, 1, 2 }[Math.Min(count % 10, 5)];
                return forms[idx];
            }
            #endregion

            return AppSettings.Instance.Language switch
            {
                ELang.En => $"{count} day{(count == 1 ? "" : "s")}",
                _ => $"{count} {Ru()}"
            };
        }
        #endregion
        #region Длительность в часах
        public static string HoursCount(int count)
        {
            #region Ru
            string Ru()
            {
                var n = Math.Abs(count) % 100;
                var n1 = n % 10;

                if (n is >= 11 and <= 14) return "часов";
                return n1 switch
                {
                    1 => "час",
                    2 or 3 or 4 => "часа",
                    _ => "часов"
                };
            }
            #endregion

            return AppSettings.Instance.Language switch
            {
                ELang.En => $"{count} hour{(count == 1 ? "" : "s")}",
                _ => $"{count} {Ru()}"
            };
        }
        #endregion
    }
}
