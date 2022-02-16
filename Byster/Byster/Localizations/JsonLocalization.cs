using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Localizations
{
    public class JsonLocalization
    {
        public string LocalizationLanguageCode { get; set; }
        public Dictionary<string, string> LocalizationAssociations { get; set; }
    }
}
