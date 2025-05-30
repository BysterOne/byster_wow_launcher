using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterModels.Primitives
{
    public abstract class Setting<TEnum> : INotifyPropertyChanged where TEnum : Enum
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public object RegistryValue { get; }
        public object Value { get; }
        public TEnum EnumValue { get; }
        public string Name { get; }

        public Setting(string _name, TEnum _enum, object _value = default(object), object _registryValue = default(object))
        {
            RegistryValue = _registryValue;
            EnumValue = _enum;
            Value = _value;
            Name = _name;
        }
    }
}
