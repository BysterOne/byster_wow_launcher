using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace Byster.Localizations.Tools
{
    public static class Localizator
    {
        public static LocalizationInfo LoadedLocalizationInfo { get; set; }
        public static List<LocalizationResource> LocalizedResources { get; set; } = new List<LocalizationResource>();

        private static string baseDirOfLocalizations = "pack://application:,,,/Localizations/LocalizationData/";
        private static Dictionary<string, string> localizationKeyAndFileAssociations = new Dictionary<string, string>()
        {
            {"ruRU", baseDirOfLocalizations + "Russian.json" },
        };
        static Localizator()
        {
            //string osLang = CultureInfo.CurrentUICulture.IetfLanguageTag + "-" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            LoadLocalization(LocalizationInfo.GetLocalizationInfoByLanguageCode("ruRU"));
        }

        private static string getResourceOfLocalization(string langCode)
        {
            if(localizationKeyAndFileAssociations.ContainsKey(langCode)) return localizationKeyAndFileAssociations[langCode];
            return null;
        }

        private static string readResource(string resourcePath)
        {
            Stream stream = Application.GetResourceStream(new Uri(resourcePath)).Stream;
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[128];
            while(stream.Position < stream.Length)
            {
                int len = stream.Read(buffer, 0, buffer.Length);
                builder.Append(Encoding.UTF8.GetString(buffer, 0, len));
            }
            return builder.ToString();
        }

        public static void LoadLocalization(LocalizationInfo info)
        {
            if (info == null) return;
            var file = getResourceOfLocalization(info.LanguageCode);
            if(file == null) return;
            var jsonLocalization = JsonConvert.DeserializeObject<JsonLocalization>(readResource(file));
            if(jsonLocalization == null) return;
            LoadedLocalizationInfo = info;
            foreach (var key in jsonLocalization.LocalizationAssociations.Keys)
            {
                if (LocalizedResources.Where(resource => resource.Key == key).FirstOrDefault() == null)
                {
                    LocalizedResources.Add(new LocalizationResource()
                    {
                        Key = key,
                        Value = jsonLocalization.LocalizationAssociations[key],
                    });
                }
                else
                {
                    LocalizedResources.Where(resource => resource.Key == key).First().Value = jsonLocalization.LocalizationAssociations[key];
                }   
            }
        }

        public static LocalizationResource GetLocalizationResourceByKey(string key)
        {
            var resource = LocalizedResources.Where(_resource => _resource.Key == key).FirstOrDefault();
            if(resource == null)
            {
                LocalizationResource resourceToAdd = new LocalizationResource()
                {
                    Key = key,
                    Value = "",
                };
                LocalizedResources.Add(resourceToAdd);
                return resourceToAdd;
            }
            else
            {
                return resource;
            }
        }
    }

    public class LocalizationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Localizator.GetLocalizationResourceByKey((string)parameter).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class LocalizationInfo
    {
        public string Language { get; set; }
        public string LanguageCode { get; set; }
        public int Id { get; set; }

        public static List<LocalizationInfo> AllLocalizationInfos = new List<LocalizationInfo>()
        {
            new LocalizationInfo()
            {
                Language = "Русский",
                LanguageCode = "ruRU",
                Id = 0,
            },
            new LocalizationInfo()
            {
                Language = "English",
                LanguageCode = "enUS",
                Id = 1,
            }
        };

        public static LocalizationInfo GetLocalizationInfoByLanguage(string lang)
        {
            return AllLocalizationInfos.Where(localization => localization.Language == lang).FirstOrDefault();
        }
        public static LocalizationInfo GetLocalizationInfoByLanguageCode(string langCode)
        {
            return AllLocalizationInfos.Where(localization => localization.LanguageCode == langCode).FirstOrDefault();
        }

        public static LocalizationInfo GetLocalizationInfoById(int id)
        {
            return AllLocalizationInfos.Where(localization => localization.Id == id).FirstOrDefault();
        }
    }

}
