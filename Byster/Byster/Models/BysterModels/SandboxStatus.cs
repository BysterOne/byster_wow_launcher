using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Byster.Models.BysterModels.Primitives;

namespace Byster.Models.BysterModels
{
    public class SandboxStatus : Setting<int, int, SandboxType>
    {
        public SandboxStatus(string _name, SandboxType _enum, int _value = 0, int _registryValue = 0) : base(_name, _enum, _value, _registryValue) { }
    }

    public class SandboxStatusAssociator : SettingAssociator<SandboxStatus, int, int, SandboxType>
    {
        private static SandboxStatusAssociator instance;
        public new static SandboxStatusAssociator GetAssociator()
        {
            return instance ?? (instance = new SandboxStatusAssociator());
        }
        public SandboxStatusAssociator() : base()
        {
            AllInstances = new List<SandboxStatus>()
            {
                new SandboxStatus("Production", SandboxType.PRODUCTION, 0, 0),
                new SandboxStatus("Sandbox", SandboxType.SANDBOX, 1, 1),
            };
        }
    }

    public enum SandboxType
    {
        PRODUCTION = 0,
        SANDBOX = 1,
    }
}
