using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Byster.Localizations.Tools;
using Byster.Models.BysterModels.Primitives;

namespace Byster.Models.BysterModels
{
    public class LoadType : Setting<int, int, LoadTypeType>
    {
        public LoadType(string _name, LoadTypeType _enum, int _value = 0, int _registryValue = 0) : base(_name, _enum, _value, _registryValue) { }
    }

    public class LoadTypeAssociator : SettingAssociator<LoadType, int, int, LoadTypeType>
    {
        private static LoadTypeAssociator instance;
        public static new LoadTypeAssociator GetAssociator()
        {
            return instance ?? (instance = new LoadTypeAssociator());
        }
        public LoadTypeAssociator()
        {
            AllInstances = new List<LoadType>()
            {
                new LoadType(Localizator.GetLocalizationResourceByKey("MixedLoadtype").Value, LoadTypeType.MIXED, 2, 2),
                new LoadType(Localizator.GetLocalizationResourceByKey("ServerLoadtype").Value, LoadTypeType.SERVER, 3, 3),
            };
        }

    }

    public enum LoadTypeType
    {
        MIXED = 1,
        SERVER = 2,
    }
}
