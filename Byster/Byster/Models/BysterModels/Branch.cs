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
    public class Branch : Setting<string, string, BranchType>
    {
        public Branch(string _name, BranchType _enum, string _value = null, string _registryValue = null) : base(_name, _enum, _value, _registryValue) { }
    }

    public class BranchAssociator : SettingAssociator<Branch, string, string, BranchType>
    {
        private static BranchAssociator instance;
        public static new BranchAssociator GetAssociator()
        {
            return instance ?? (instance = new BranchAssociator());
        }
        public BranchAssociator() : base()
        {
            AllInstances = new List<Branch>()
            {
                new Branch(Localizator.GetLocalizationResourceByKey("TesterBranch"), BranchType.TEST, "test", "test"),
                new Branch(Localizator.GetLocalizationResourceByKey("DeveloperBranch"), BranchType.TEST, "dev", "dev"),
                new Branch(Localizator.GetLocalizationResourceByKey("MasterBranch"), BranchType.TEST, "master", "master"),
            };
        }
    }
    public enum BranchType
    {
        DEVELOPER = 3,
        TEST =      2,
        MASTER =    1,
        UNKNOWN =   -1,
    }
}
