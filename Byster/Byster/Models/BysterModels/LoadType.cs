using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Byster.Localizations.Tools;

namespace Byster.Models.BysterModels
{
    public class LoadType
    {
        public int Value { get; set; }
        public string Name { get; set; }

        public static LoadType[] AllLoadTypes { get; set; } = new LoadType[] {
            //new LoadType(){ Name=Localizator.GetLocalizationResourceByKey("LocalLoadtype"), Value=1},
            new LoadType(){ Name=Localizator.GetLocalizationResourceByKey("MixedLoadtype"), Value=2},
            new LoadType(){ Name=Localizator.GetLocalizationResourceByKey("ServerLoadtype"), Value=3},
        };
    }
}
