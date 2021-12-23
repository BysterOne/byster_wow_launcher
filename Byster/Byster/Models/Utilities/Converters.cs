using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Byster.Models.Utilities
{
    public class WordEndAssociator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string startWord = " " + parameter.ToString();
            int count = int.Parse(value.ToString());
            count = count % 21;
            if (count == 0)
            {
                startWord += "ий";
            }
            else if (count == 1)
            {
                startWord += "ия";
            }
            else if (count >= 2 && count <= 4)
            {
                startWord += "ии";
            }
            else if (count >= 4 && count <= 20)
            {
                startWord += "ий";
            }
            return startWord;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
