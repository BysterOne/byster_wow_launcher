using Launcher.Any.GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Any
{
    public static class DictionaryExtensions
    {
        public static ELang? GetKeyOrNull(this Dictionary<ELang, string> dict, string value)
        {
            foreach (var pair in dict)
            {
                if (pair.Value == value)
                    return pair.Key;
            }
            return null;
        }
    }

}
