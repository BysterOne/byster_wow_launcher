using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Localizations.Tools
{
    public class LocalizationDataForViewModels
    {
        //MainWindow
        public LocalizationResource Main { get; set; }
        public LocalizationResource Shop { get; set; }
        public LocalizationResource NoRotations { get; set; }
        public LocalizationResource AllAvailableRotations { get; set; }
        public LocalizationResource LoginToPerson { get; set; }
        public LocalizationResource Start { get; set; }
        public LocalizationResource WaitLoadingEnd { get; set; }
        public LocalizationResource Classes { get; set; }
        public LocalizationResource Types { get; set; }
        public LocalizationResource ToBuy { get; set; }

        public LocalizationDataForViewModels()
        {
            Main = "Main";
            Shop = "Shop";
            NoRotations = "NoRotations";
            AllAvailableRotations = "AllAvailableRotations";
            LoginToPerson = "LoginToPerson";
            Start = "Start";
            WaitLoadingEnd = "WaitLoadingEnd";
            Classes = "Classes";
            Types = "Types";
            ToBuy = "ToBuy";
        }
    }
}
