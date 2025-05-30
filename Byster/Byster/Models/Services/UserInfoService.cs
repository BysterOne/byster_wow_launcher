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
using static Byster.Models.Utilities.BysterLogger;
using Byster.Models.Utilities;
using System.Web.UI;
using System.Runtime.InteropServices;

namespace Byster.Models.Services
{
    public class UserInfoService : INotifyPropertyChanged, IService
    {
        public bool IsInitialized { get; set; } = false;
        public Dispatcher Dispatcher { get; set; }
        public string SessionId { get; set; }
        public RestService RestService { get; set; }
        //Registry values
        private string username;
        private string referalCode;
        private int bonusBalance;
        private BranchType userType;
        private string currency;
        private bool encryption;
        public SandboxStatus SandboxStatus
        {
            get => SandboxStatusAssociator.GetAssociator().GetInstanceByRegistryValue(RegistryEditor.GetEditor("Sandbox").RegistryValue);
            set
            {
                RegistryEditor.GetEditor("Sandbox").RegistryValue = value?.RegistryValue ?? 0;
                OnPropertyChanged("SandboxStatus");
            }
        }
        public string Username
        {
            get { return username; }
            set

            {
                username = value;
                OnPropertyChanged("Username");
            }
        }
        public int BonusBalance
        {
            get { return bonusBalance; }
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

        public Branch Branch
        {
            get { return BranchAssociator.GetAssociator().GetInstanceByRegistryValue(RegistryEditor.GetEditor("Branch").RegistryValue); }
            set
            {
                RegistryEditor.GetEditor("Branch").RegistryValue = value?.RegistryValue ?? "master";
                OnPropertyChanged("Branch");
            }
        }

        public bool Console
        {
            get => (int)RegistryEditor.GetEditor("Console").RegistryValue == 1;
            set
            {
                RegistryEditor.GetEditor("Console").RegistryValue = value ? 1 : 0;
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

        public LoadType LoadType
        {
            get => LoadTypeAssociator.GetAssociator().GetInstanceByRegistryValue(RegistryEditor.GetEditor("LoadType").RegistryValue);
            set
            {
                RegistryEditor.GetEditor("LoadType").RegistryValue = value?.RegistryValue ?? 3;
                OnPropertyChanged("LoadType");
            }
        }

        public string UserImage
        {
            get
            {
                string root = "/Resources/Images/UserLogos/";
                switch (UserType)
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

        public string Currency
        {
            get { return currency; }
            set
            {
                currency = value;
                OnPropertyChanged("Currency");
            }
        }

        public bool Encryption
        {
            get { return encryption; }
            set
            {
                encryption = value;
                OnPropertyChanged("Encryption");
            }
        }

        public bool EncryptionWithAutoUpdate
        {
            get { return encryption; }
            set
            {
                encryption = value;
                IsEncryptinChangeAllowed = false;
                Task.Run(() =>
                {
                    SetEncryption(Encryption);
                    IsEncryptinChangeAllowed = true;
                });
                OnPropertyChanged("EncryptionWithAutoUpdate");
            }
        }

        private bool isEncryptinChangeAllowed = true;
        public bool IsEncryptinChangeAllowed
        {
            get { return isEncryptinChangeAllowed; }
            set
            {
                isEncryptinChangeAllowed = value;
                OnPropertyChanged("IsEncryptinChangeAllowed");
            }
        }


        public List<Branch> BranchChoices { get => BranchAssociator.GetAssociator().AllInstances.Where(_branch => _branch.EnumValue <= UserType).ToList(); }
        public List<LoadType> LoadTypes { get => LoadTypeAssociator.GetAssociator().AllInstances; }
        public List<SandboxStatus> SandboxStatuses { get => SandboxStatusAssociator.GetAssociator().AllInstances; }

        static UserInfoService()
        {
        }

        public void UpdateRemoteData()
        {
            (string _usernane, string _referalcode, int _bonuses, string _currency, bool? _encryption) = RestService.GetUserInfo();
            if (string.IsNullOrEmpty(_usernane) ||
               string.IsNullOrEmpty(_referalcode))
            {
                return;
            }
            (Username, ReferalCode, BonusBalance, Currency, Encryption) = (_usernane, _referalcode, _bonuses, _currency, _encryption ?? false);
            UserType = RestService.GetUserType();
        }

        public void UpdateData()
        {
            UpdateRemoteData();
        }

        public void UpdateEncryption()
        {
            Encryption = RestService.GetEncryptStatus() ?? false;
        }

        public void SetEncryption(bool newStatus)
        {
            RestService.SetEncryptStatus(newStatus);
            Encryption = newStatus;
        }

        public void SetBranch(Branch branch)
        {
            Branch = branch;
        }

        public void SetConsole(bool isConsoleEnabled)
        {
            Console = isConsoleEnabled;
        }

        public void SetLoadType(LoadType loadType)
        {
            LoadType = loadType;
        }
        public void SetSandboxStatus(SandboxStatus sandbox)
        {
            if (SandboxStatus.EnumValue != SandboxType.PRODUCTION)
                SandboxStatus = sandbox;
        }
        public UserInfoService(RestService service)
        {
            LogInfo("User Info Service", "Создание сервиса");
            RestService = service;
            PropertyChanged += (o, e) =>
            {
                if(e.PropertyName == "UserType")
                {
                    ValidateUserData();
                }
            };
        }

        public void ValidateUserData()
        {
            switch (UserType)
            {
                case BranchType.UNKNOWN:
                    LogWarn("User Info Service", "UserType изменил значение на UNKNOWN");
                    SetBranch(BranchAssociator.GetAssociator().GetInstanceByEnumValue(BranchType.MASTER));
                    SetLoadType(LoadTypeAssociator.GetAssociator().GetInstanceByEnumValue(LoadTypeType.SERVER));
                    SetConsole(false);
                    SetSandboxStatus(SandboxStatusAssociator.GetAssociator().GetInstanceByEnumValue(SandboxType.PRODUCTION));
                    break;
                case BranchType.MASTER:
                    SetBranch(BranchAssociator.GetAssociator().GetInstanceByEnumValue(BranchType.MASTER));
                    SetLoadType(LoadTypeAssociator.GetAssociator().GetInstanceByEnumValue(LoadTypeType.SERVER));
                    SetConsole(false);
                    SetSandboxStatus(SandboxStatusAssociator.GetAssociator().GetInstanceByEnumValue(SandboxType.PRODUCTION));
                    break;
                case BranchType.TEST:
                    if(Branch.EnumValue > BranchType.TEST) SetBranch(BranchAssociator.GetAssociator().GetInstanceByEnumValue(BranchType.TEST));
                    SetLoadType(LoadTypeAssociator.GetAssociator().GetInstanceByEnumValue(LoadTypeType.SERVER));
                    break;
                case BranchType.DEVELOPER:
                    break;
            }
        }

        public void Initialize(Dispatcher dispatcher)
        {
            LogInfo("User Info Service", "Запуск сервиса");
            Dispatcher = dispatcher;
            UpdateData();
            Injector.PreInjection += () =>
            {
                ValidateUserData();
            };
            RegistryEditor.AddEditor(new RegistrySettingEditorCreateConfig("Удаление здесь любого ключа приведёт к бану", "")).RegistryValue = "Deleting here any key leads to BAN";
            IsInitialized = true;
            LogInfo("User Info Service", "Запуск сервиса завершён");
        }

        public void Dispose()
        {
            LogInfo("User Info Service", "Завершение работы сервиса...");
        }

        public bool ChangePasssword(string newPassswordHash)
        {
            bool result = RestService.ExecuteChangePasswordRequest(newPassswordHash);
            if (result)
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
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public async Task<bool> ClearCacheAsync()
        {
            bool res = await RestService.ExecuteAsyncClearCacheRequest();
            return res;
        }
        public void ResetConfig()
        {
            string configDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BysterConfig");
            int configIndex = getLastOldConfigIndex(configDirectoryPath);
            string currectConfigDirPath = Path.Combine(configDirectoryPath, "configs");
            if (!Directory.Exists(currectConfigDirPath)) return;
            string createdConfigDirPath = Path.Combine(configDirectoryPath, $"config.old.{configIndex}");
            Directory.CreateDirectory(Path.Combine(configDirectoryPath, createdConfigDirPath));
            foreach(var source in Directory.EnumerateFiles(currectConfigDirPath))
            {
                Directory.Move(source, createdConfigDirPath);
            }
            foreach (var source in Directory.EnumerateDirectories(currectConfigDirPath))
            {
                Directory.Move(source, createdConfigDirPath);
            }
            Directory.Delete(Path.Combine(configDirectoryPath, "configs"));
            LogInfo("UserInfo Service", "Конфиги сброшены");
        }

        private int getLastOldConfigIndex(string configDirPath)
        {
            Directory.GetDirectories(configDirPath);
            for (int i = 0; i < 1000; i++)
            {
                if(!Directory.Exists(Path.Combine(configDirPath, $"config.old.{i}")))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
