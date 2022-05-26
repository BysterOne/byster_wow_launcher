using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Byster.Models.BysterModels
{
    public class SandboxStatus
    {
        public string Name { get; set; }
        public int RegistryValue { get; set; }
        public static List<SandboxStatus> SandboxStatuses { get; set; } = new List<SandboxStatus>()
        {
            new SandboxStatus() { Name = "Production", RegistryValue = 0 },
            new SandboxStatus() { Name = "Sandbox", RegistryValue = 1 }
        };
        public static SandboxStatus ReadServerTypeFromReg()
        {
            int regValue = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Byster", "Sandbox", -1);
            if (regValue == -1)
            {
                regValue = 0;
            }
            else if (regValue == 0)
            {
                Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Byster", true).DeleteValue("Sandbox");
            }
            return SandboxStatuses.Where(_type => _type.RegistryValue == regValue).FirstOrDefault();
        }
        public static void WriteServerTypeToReg(SandboxStatus serverType)
        {
            if (serverType == null) return;
            if(serverType.RegistryValue == 0)
            {
                Registry.CurrentUser.OpenSubKey(@"Software\Byster", true).DeleteValue(@"Sandbox", false);
                return;
            }
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Byster", "Sandbox", serverType.RegistryValue);
        }
    }
}
