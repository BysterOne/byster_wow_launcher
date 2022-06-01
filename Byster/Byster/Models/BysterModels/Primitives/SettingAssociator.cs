using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterModels.Primitives
{
    public abstract class SettingAssociator<TSetting, TEnum> where TSetting : Setting<TEnum> where TEnum : Enum
    { 
        public List<TSetting> AllInstances { get; set; }
        public TSetting GetInstanceByRegistryValue(object _registryValue)
        {
            return AllInstances.Where(_ins => _ins.RegistryValue.Equals(_registryValue)).FirstOrDefault();
        }

        public TSetting GetInstanceByValue(object _value)
        {
            return AllInstances.Where(_ins => _ins.Value.Equals(_value)).FirstOrDefault();
        }
        public TSetting GetInstanceByName(string _name)
        {
            return AllInstances.Where(_ins => _ins.Name.Equals(_name)).FirstOrDefault();
        }
        public TSetting GetInstanceByEnumValue(TEnum _enumValue)
        {
            return AllInstances.Where(_ins => _ins.EnumValue.Equals(_enumValue)).FirstOrDefault();
        }
    }
}
