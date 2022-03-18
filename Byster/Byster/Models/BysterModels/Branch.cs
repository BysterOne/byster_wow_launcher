using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Byster.Localizations.Tools;

namespace Byster.Models.BysterModels
{
    public class Branch : INotifyPropertyChanged
    {
        public string Name { get; set; }
        private BranchType branchType;
        public BranchType BranchType
        {
            get { return branchType; }
            set
            {
                branchType = value;
                OnPropertyChanged("BranchType");
                switch (branchType)
                {
                    case BranchType.TEST:
                        Name = Localizator.GetLocalizationResourceByKey("TesterBranch");
                        break;
                    case BranchType.DEVELOPER:
                        Name = Localizator.GetLocalizationResourceByKey("DeveloperBranch");
                        break;
                    default:
                    case BranchType.MASTER:
                        Name = Localizator.GetLocalizationResourceByKey("MasterBranch");
                        break;
                }
                OnPropertyChanged("Name");
            }
        }
        public Branch(BranchType type)
        {
            BranchType = type;
            switch (BranchType)
            {
                case BranchType.TEST:
                    Name = Localizator.GetLocalizationResourceByKey("TesterBranch");
                    break;
                case BranchType.DEVELOPER:
                    Name = Localizator.GetLocalizationResourceByKey("DeveloperBranch");
                    break;
                default:
                case BranchType.MASTER:
                    Name = Localizator.GetLocalizationResourceByKey("MasterBranch");
                    break;
            }
        }
        public static Branch[] AllBranches { get; set; } =
            new Branch[]
            {
                new Branch(BranchType.MASTER),
                new Branch(BranchType.TEST),
                new Branch(BranchType.DEVELOPER),
            };

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
    public enum BranchType
    {
        DEVELOPER = 3,
        TEST      = 2,
        MASTER    = 1,
        UNKNOWN   = -1,
    }
}
