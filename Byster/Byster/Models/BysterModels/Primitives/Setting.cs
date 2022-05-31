using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterModels.Primitives
{
    public abstract class Setting<TRegValue, TValue, TEnum> : INotifyPropertyChanged where TEnum : Enum
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public TRegValue RegistryValue { get; set; }
        public TValue Value { get; set; }
        public TEnum EnumValue { get; set; }
        public string Name { get; set; }

        public Setting(string _name, TEnum _enum, TValue _value = default(TValue), TRegValue _registryValue = default(TRegValue))
        {
            RegistryValue = _registryValue;
            EnumValue = _enum;
            Value = _value;
            Name = _name;
        }
    }
}
