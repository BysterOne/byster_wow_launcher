using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using static Byster.Models.Utilities.BysterLogger;

namespace Byster.Models.BysterModels
{
    public class RegistrySettingEditor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        
        private readonly string registryRootSubkey = @"Software\Byster";

        public object defaultValue;

        public OnTriggerValueAction triggerAction;
        public IEnumerable<object> triggerValues;

        public string RegistryValueName { get; }
        private object registryValue;
        public object RegistryValue
        {
            get => registryValue;
            set
            {
                registryValue = value;
                OnPropertyChanged("RegistryValue");
            }
        }

        private bool checkTrigger()
        {
            return triggerValues?.Contains(RegistryValue) ?? false;
        }

        private void onTriggered()
        {
            switch (triggerAction)
            {
                case OnTriggerValueAction.Default:
                    Registry.CurrentUser.OpenSubKey(registryRootSubkey, true).SetValue(RegistryValueName, RegistryValue);
                    break;
                case OnTriggerValueAction.DeleteValue:
                    Registry.CurrentUser.OpenSubKey(registryRootSubkey, true).DeleteValue(RegistryValueName, false);
                    break;
            }
            
        }

        private void pushValueToRegistry()
        {
            if (!checkRegistryAccessConfig()) return;
            if(checkTrigger())
            {
                onTriggered();
            }
            else
            {
                Registry.CurrentUser.OpenSubKey(registryRootSubkey, true).SetValue(RegistryValueName, RegistryValue);
            }

        }
        private void pullValueFromRegistry()
        {
            if (!checkRegistryAccessConfig()) return;
            RegistryValue = Registry.CurrentUser.OpenSubKey(registryRootSubkey).GetValue(RegistryValueName, defaultValue);
            if (checkTrigger()) onTriggered();
        }
        private bool checkRegistryAccessConfig()
        {
            if (string.IsNullOrWhiteSpace(RegistryValueName)) return false;
            return true;
        }
        public RegistrySettingEditor(IRegistrySettingEditorConfig config)
        {
            this.RegistryValueName = config.RegistryValueName;
            this.defaultValue = config.DefaultValue;
            this.triggerAction = config.OnTriggerValueAction;
            this.triggerValues = config.TriggerValues;
            if (!checkRegistryAccessConfig()) throw new Exception($"Attempt to create an instance of {nameof(RegistrySettingEditor)} with incorrect params");
            pullValueFromRegistry();
            PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == nameof(RegistryValue))
                {
                    pushValueToRegistry();
                }
            };
        }
    }

    public interface IRegistrySettingEditorConfig
    {
        string RegistryValueName { get; set; }
        object DefaultValue { get; set; }
        OnTriggerValueAction OnTriggerValueAction { get; set; }
        IEnumerable<object> TriggerValues { get; set; }
    }

    public struct RegistrySettingEditorCreateConfig : IRegistrySettingEditorConfig
    {
        public string RegistryValueName { get; set; }
        public object DefaultValue { get; set; }
        public OnTriggerValueAction OnTriggerValueAction { get; set; }
        public IEnumerable<object> TriggerValues { get; set; }
        public RegistrySettingEditorCreateConfig(string registryValueName, object defaultValue = null, OnTriggerValueAction triggerAction = OnTriggerValueAction.Default, IEnumerable<object> triggerValues = null)
        {
            this.RegistryValueName = registryValueName;
            this.DefaultValue = defaultValue;
            this.OnTriggerValueAction = triggerAction;
            this.TriggerValues = triggerValues;
        }
    }

    public static class RegistryEditor
    {
        private static List<RegistrySettingEditor> allSettingEditors;
        public static IEnumerable<RegistrySettingEditor> AllSettingEditors => allSettingEditors;

        private readonly static RegistrySettingEditorCreateConfig[] presetConfigs =
        {
            new RegistrySettingEditorCreateConfig("Console", 0),
            new RegistrySettingEditorCreateConfig("LoadType", 3),
            new RegistrySettingEditorCreateConfig("Branch", "master"),
            new RegistrySettingEditorCreateConfig("Sandbox", 0, OnTriggerValueAction.DeleteValue, new List<object>(){0})
        };
        
        static RegistryEditor()
        {
            var presetEditors = new List<RegistrySettingEditor>();
            foreach(var preset in presetConfigs)
            {
                var editor = new RegistrySettingEditor(preset);
                presetEditors.Add(editor);   
            }
            allSettingEditors = presetEditors;
        }

        public static RegistrySettingEditor GetEditor(string registryValueName)
        {
            return allSettingEditors.Where(_editor => _editor.RegistryValueName == registryValueName).FirstOrDefault();
        }

        public static RegistrySettingEditor AddEditor(IRegistrySettingEditorConfig config)
        {
            if (config == null) return null;
            if (allSettingEditors.Any(_editor => _editor.RegistryValueName == config.RegistryValueName)) return null;
            try
            {
                var editor = new RegistrySettingEditor(config);
                allSettingEditors.Add(editor);
                return editor;
            }
            catch
            {
                LogWarn("Редактор реестра", "Ошибка создания редактора");
                return null;
            }
        }
    }

    public enum OnTriggerValueAction
    {
        Default = 0,
        DeleteValue = 1,
    }
}
