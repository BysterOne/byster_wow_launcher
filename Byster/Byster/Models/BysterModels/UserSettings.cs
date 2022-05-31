using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterModels
{
    public abstract class BysterUserSettingEditor<T> : INotifyPropertyChanged
    {
        protected T value;
        public T Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        protected BysterUserSettingEditor() {}
    }

    public class BysterConsoleEditor : BysterUserSettingEditor<bool>
    {
        private static BysterConsoleEditor instance;
        protected static BysterConsoleEditor CreateInstance()
        {
            var regValue = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Byster", "Console", -1);
            if (regValue == -1) Registry.SetValue(@"HKEY_CURRENT_USER\Software\Byster", "Console", (regValue = 0));
            var instance = new BysterConsoleEditor()
            {
                value = regValue > 0,
            };
            instance.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == "Value")
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Byster", "Console", instance.Value ? 1 : 0);
                }
            };
            return instance;
        }
        public static BysterConsoleEditor GetInstance()
        {
            return instance ?? (instance = CreateInstance());
        }
    }
    public class BysterLoadTypeEditor : BysterUserSettingEditor<int>
    {
        private static BysterLoadTypeEditor instance;
        protected static BysterLoadTypeEditor CreateInstance()
        {
            var regValue = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Byster", "LoadType", -1);
            if (regValue == -1) Registry.SetValue(@"HKEY_CURRENT_USER\Software\Byster", "LoadType", (regValue = 3));
            var instance = new BysterLoadTypeEditor()
            {
                value = regValue,
            };
            instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Value")
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Byster", "LoadType", instance.Value);
                }
            };
            return instance;
        }
        public static BysterLoadTypeEditor GetInstance()
        {
            return instance ?? (instance = CreateInstance());
        }
    }
    public class BysterBranchEditor : BysterUserSettingEditor<string>
    {
        private static BysterBranchEditor instance;
        protected static BysterBranchEditor CreateInstance()
        {
            var regValue = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Byster", "Branch", "undefined");
            if (regValue == "undefined") Registry.SetValue(@"HKEY_CURRENT_USER\Software\Byster", "Branch", (regValue = "master"));
            var instance = new BysterBranchEditor()
            {
                value = regValue,
            };
            instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Value")
                {
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\Byster", "Branch", instance.Value);
                }
            };
            return instance;
        }
        public static BysterBranchEditor GetInstance()
        {
            return instance ?? (instance = CreateInstance());
        }
    }
    public class BysterSandboxEditor : BysterUserSettingEditor<int>
    {
        private static BysterSandboxEditor instance;
        protected static BysterSandboxEditor CreateInstance()
        {
            var regValue = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Byster", "Sandbox", -1);
            if (regValue == 0) Registry.CurrentUser.OpenSubKey(@"Software\Byster", true).DeleteValue("Sandbox", false);
            if (regValue == -1) regValue = 0;
            var instance = new BysterSandboxEditor()
            {
                value = regValue,
            };
            instance.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Value")
                {
                    if (instance.Value == 1)
                    {
                        Registry.SetValue(@"HKEY_CURRENT_USER\Software\Byster", "Sandbox", 1);
                    }
                    else
                    {
                        Registry.CurrentUser.OpenSubKey(@"Software\Byster", true).DeleteValue("Sandbox", false);
                    }
                }
            };
            return instance;
        }
        public static BysterSandboxEditor GetInstance()
        {
            return instance ?? (instance = CreateInstance());
        }
    }
}
