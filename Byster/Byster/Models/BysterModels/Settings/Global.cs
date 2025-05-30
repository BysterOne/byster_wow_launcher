using Byster.Views.ModelsTemp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterModels.Settings
{
    public class Settings
    {
        #region Путь
        private static string FilePath { get; set; } = Path.Combine("settings.json");
        #endregion

        #region Свойства
        public ObservableCollection<ClientModel> Clients { get; set; } = new ObservableCollection<ClientModel>();
        #endregion

        #region Свойство для доступа
        private static Settings _instance;
        public static Settings I
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }
        #endregion

        #region Функции
        #region Load
        public static Settings Load()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                var converted = JsonConvert.DeserializeObject<Settings>(json);
                if (converted != null) return converted;
            }
            return new Settings();
        }
        #endregion
        #region Save
        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
        #endregion
        #endregion
    }
}
