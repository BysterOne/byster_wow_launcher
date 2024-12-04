using Byster.Views.ModelsTemp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Byster.Models.ViewModels
{
    public class ClientImageSourceConverter : IValueConverter
    {
        public static BitmapImage IconTypeToImageSource(ClientIcon? value)
        {
            string source = "no-client-image.png";

            if (value != null)
            {
                switch (value)
                {
                    case ClientIcon.Wow: source = "WOW.png"; break;
                    case ClientIcon.Wizard: source = "Wizard.png"; break;
                    case ClientIcon.Warrior: source = "Warrior.png"; break;
                    case ClientIcon.Warlock: source = "Warlock.png"; break;
                }
            }

            return new BitmapImage(new Uri($"pack://application:,,,/Byster;component/Resources/Images/ClassesWOW/{source}"));
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IconTypeToImageSource((ClientIcon)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented.");
        }
    }
}
