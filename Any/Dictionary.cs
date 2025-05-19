using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Any.GlobalEnums;
using Launcher.Api;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.DictionaryAny;
using Launcher.Settings;
using Launcher.Settings.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Nodes;
using System.Windows;

namespace Launcher
{
    namespace DictionaryAny
    {
        public enum ELoad
        {
            FailLoadTranslations
        }
    }

    public static class Dictionary
    {
        private static LogBox Pref { get; set; } = new ("Dictionary");

        public static List<ULocDictionary> Localizations { get; set; } = [];

        public static List<string> Untranslatable { get; set; } = [];

        #region ReadFromResource
        private static T? ReadFromResource<T>(string path)
        {
            var uri = new Uri($"pack://application:,,,/{path}", UriKind.Absolute);
            var resource = Application.GetResourceStream(uri);

            if (resource is null) return default(T);

            using var reader = new StreamReader(resource.Stream);
            var jsonObject = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            if (jsonObject is null) return default(T);

            return jsonObject;
        }
        #endregion
        #region GetKeys
        private static List<string> GetUntranslatable()
        {
            var read = ReadFromResource<List<string>>("Localizations/untranslatable.json");
            return read is null ? [] : read;
        }
        #endregion
        #region GetKeys
        private static List<string> GetKeys()
        {
            var read = ReadFromResource<List<string>>("Localizations/keys.json");
            return read is null ? [] : read;
        }
        #endregion
        #region Load
        public static async Task<UResponse> Load()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось загрузить переводы";

            #region try
            try
            {
                var assm = Assembly.GetExecutingAssembly();
                var reqkeys = GetKeys();
                Untranslatable = GetUntranslatable();
                var reqLangs = new List<ELang>() { ELang.En, ELang.ZhCn };

                #region Загрузка локальных файлов
                foreach (var lang in reqLangs)
                {
                    var fileName = lang switch
                    {
                        ELang.En => "English.json",
                        ELang.ZhCn => "Chinese.json",
                        _ => null
                    };

                    if (fileName is null) continue;

                    var uri = new Uri($"pack://application:,,,/Localizations/{fileName}", UriKind.Absolute);
                    var resource = Application.GetResourceStream(uri);

                    if (resource is null)
                    {
                        _proc.Log($"Файл локализации '{fileName}' не найден");
                        continue;
                    }

                    using var reader = new StreamReader(resource.Stream);
                    var jsonObject = JsonConvert.DeserializeObject<LocalDictionary>(reader.ReadToEnd());
                    if (jsonObject is not null) CreateOrUpdateLocalization(lang, jsonObject.Translations);
                }
                #endregion
                #region Подгрузка недостоящих ключей
                foreach (var lang in reqLangs)
                {
                    var onlyThisLangKeys = new List<string>();

                    var ownKeys = Localizations.FirstOrDefault(x => x.Language == lang);
                    if (ownKeys is null)
                        onlyThisLangKeys.AddRange(reqkeys);
                    else
                        foreach (var u in reqkeys)
                            if (!ownKeys.Translations.ContainsKey(u))
                                onlyThisLangKeys.Add(u);

                    if (onlyThisLangKeys.Count is 0) continue;

                    var tryGetTransl = await CApi.Translate(lang, onlyThisLangKeys);
                    if (!tryGetTransl.IsSuccess)
                    {
                        throw new UExcept(ELoad.FailLoadTranslations, $"Не удалось получить перевод для <{lang}>", tryGetTransl.Error);
                    }

                    CreateOrUpdateLocalization(lang, tryGetTransl.Response.Translations);
                }
                #endregion

                return new() { IsSuccess = true };
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
                return new(ex.Error);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(GlobalErrors.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);
                return new(uerror);
            }
            #endregion
        }
        #endregion


        private static void CreateOrUpdateLocalization(ELang lang, Dictionary<string, string> values)
        {
            var locDic = Localizations.FirstOrDefault(x => x.Language == lang);
            if (locDic is not null)
            {
                foreach (var key in values)
                {
                    if (!locDic.Translations.ContainsKey(key.Key))
                    {
                        locDic.Translations.Add(key.Key, key.Value);
                    }
                }
            }

            Localizations.Add(new ()
            {
                Language = lang,
                Translations = values
            });
        }

        public static string Translate(string key) => Translate(key, AppSettings.Instance.Language);

        public static string Translate(string key, ELang lang)
        {
            var isUntranslatable = Untranslatable.Any(x => x.Equals(key, StringComparison.CurrentCultureIgnoreCase));
            if (isUntranslatable) return key;

            var dictionary = Localizations.FirstOrDefault(x => x.Language == lang);
            if (dictionary is null) return key;

            var hasValue = dictionary.Translations.TryGetValue(key, out string? translated);
            return hasValue ? translated! : key;
        }

        #region GetLanguageName
        public static string GetLanguageName(ELang lang)
        {
            return lang switch
            {
                ELang.Ru => "Русский",
                ELang.En => "English",
                ELang.ZhCn => "中文",
                _ => "Unknown"
            };
        }
        #endregion
        #region GetServerName
        public static string GetServerName(EServer server)
        {
            return server switch
            {
                EServer.Prod => "Production",
                EServer.Staging => "Staging",
                _ => "Unknown"
            };
        }
        #endregion

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
                ELang.ZhCn => $"{count} 次轮换",
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
                ELang.ZhCn => $"{count} 天",
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
                ELang.ZhCn => $"{count} 小时",
                _ => $"{count} {Ru()}"
            };
        }
        #endregion
    }
}
