using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Byster.Models.BysterModels;
using Microsoft.Win32;
using System.IO;

namespace Byster.Models.Services
{
    public class UserInfoService : INotifyPropertyChanged, IService
    {
        public bool IsInitialized { get; set; } = false;
        public Dispatcher Dispatcher { get; set; }
        public string SessionId { get; set; }
        public RestService RestService { get; set; }
        
        private string username;
        private string password;
        private string referalCode;
        private int bonusBalance;
        private string branch;
        private int console;
        private BranchType userType;
        private int loadType;

        public string Username
        {
            get { return username; }
            set
        
            {
                username = value;
                OnPropertyChanged("Username");
            }
        }
        public string Password
        { 
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged("Password");
            }
        }
        public int BonusBalance
        {
            get { return bonusBalance;}
            set
            {
                bonusBalance = value;
                OnPropertyChanged("BonusBalance");
            }
        }
        public string ReferalCode
        {
            get { return referalCode; }
            set
            {
                referalCode = value;
                OnPropertyChanged("ReferalCode");
            }
        }

        public string Branch
        {
            get { return branch; }
            set
            {
                branch = value;
                OnPropertyChanged("Branch");
            }
        }

        public int Console
        {
            get
            {
                return console;
            }
            set
            {
                console = value;
                OnPropertyChanged("Console");
            }
        }
        public BranchType UserType
        {
            get { return userType; }
            set
            {
                userType = value;
                OnPropertyChanged("UserType");
                OnPropertyChanged("UserImage");
            }
        }

        public int LoadType
        {
            get { return loadType; }
            set
            {
                loadType = value;
                OnPropertyChanged("LoadType");
            }
        }

        public string UserImage
        {
            get
            {
                string root = "/Resources/Images/UserLogos/";
                switch(UserType)
                {
                    case BranchType.DEVELOPER:
                        return root + "DEV.png";
                    case BranchType.TEST:
                        return root + "TEST.png";
                    default:
                    case BranchType.MASTER:
                        return root + "MASTER.png";
                }
            }
        }

        public List<Branch> BranchChoices { get; set; } = Byster.Models.BysterModels.Branch.AllBranches.ToList();
        public List<LoadType> LoadTypes { get; set; } = Byster.Models.BysterModels.LoadType.AllLoadTypes.ToList();

        public void UpdateRemoteData()
        {
            (string _usernane, string _referalcode, int _bonuses) = RestService.GetUserInfo();
            if (string.IsNullOrEmpty(_usernane) &&
               string.IsNullOrEmpty(_referalcode))
            {
                return;
            }
            (Username, ReferalCode, BonusBalance) = (_usernane, _referalcode, _bonuses);
            UserType = RestService.GetUserType();
        }

        public void UpdateData()
        {
            UpdateRemoteData();
            Console = Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Console", -1));
            if (Console == -1) Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Console", (Console = 0));
            Branch = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Branch", "undefined") as string;
            if(Branch == "undefined") Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Branch", (Branch = "master"));
            LoadType = Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "LoadType", -1));
            if (LoadType == -1) Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Console", (LoadType = 1));
            Password = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", "undefined") as string;
            if (UserType == BranchType.TEST)
            {
                BranchChoices.Remove(BranchChoices.FirstOrDefault(branch => branch.BranchType == BranchType.DEVELOPER));
            }
        }
        public void SetBranch(Branch branch)
        {
            if (branch == null) return;

            Branch = branch.BranchType == BranchType.DEVELOPER ? "dev" :
                     branch.BranchType == BranchType.TEST ? "test" :
                     branch.BranchType == BranchType.MASTER ? "master" :
                                                            "master";
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Branch", Branch);
        }

        public void SetConsole(bool isConsoleEnabled)
        {
            Console = isConsoleEnabled ? 1 : 0;
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Console", Console);
        }

        public void SetLoadType(LoadType loadType)
        {
            LoadType = loadType.Value;
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "LoadType", LoadType);
        }

        public UserInfoService(RestService service)
        {
            RestService = service;
        }

        public void Initialize(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            IsInitialized = true;
            UpdateData();
        }

        public bool ChangePasssword(string newPassswordHash)
        {
            bool result = RestService.ExecuteChangePasswordRequest(newPassswordHash);
            if(result)
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", newPassswordHash);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool LinkEmail(string email)
        {
            bool res = RestService.ExecuteLinkEmailRequest(email);
            return res;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
