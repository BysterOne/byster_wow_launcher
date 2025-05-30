using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Byster.Localizations.Tools;

namespace Byster.Models.Utilities
{
    public class WordEndAssociator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string startWord;
            startWord = " " + Localizator.GetLocalizationResourceByKey(parameter.ToString());
            int count = int.Parse(value.ToString());
            count = count % 21;
            if (count == 0)
            {
                startWord += Localizator.GetLocalizationResourceByKey("WordEnd1");
            }
            else if (count == 1)
            {
                startWord += Localizator.GetLocalizationResourceByKey("WordEnd2");
            }
            else if (count >= 2 && count <= 4)
            {
                startWord += Localizator.GetLocalizationResourceByKey("WordEnd3");
            }
            else if (count >= 4 && count <= 20)
            {
                startWord += Localizator.GetLocalizationResourceByKey("WordEnd1");
            }
            return startWord;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
