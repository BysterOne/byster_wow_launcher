using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Localizations.Tools
{
    public class LocalizationResource : INotifyPropertyChanged
    {
        private string key;
        private string value;
        public string Key
        {
            get => key;
            set
            {
                key = value;
                OnPropertyChanged("Key");
            }
        }
        public string Value
        {
            get => value;
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        public static implicit operator LocalizationResource(string key)
        {
            return Localizator.GetLocalizationResourceByKey(key);
        }
        public static implicit operator string(LocalizationResource localizationResource)
        {
            return localizationResource.Value;
        }
        public override string ToString()
        {
            return this?.Value ?? ""; 
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
