using Byster.Localizations.Tools;
using Byster.Models.Services;
using Byster.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Byster.Models.Utilities.BysterLogger;

namespace Byster.Models.BysterModels
{
    public class DeveloperRotation : INotifyPropertyChanged
    {
        private static int enumeratorIds = 0;
        public int Id { get; private set; } = enumeratorIds++;

        private string rootPath;
        public string gitUrl { get; private set; }

        private Visibility isShowInCollection = Visibility.Visible;
        private DevClass classOfRotation;
        private DevRoleType roleTypeOfRotation;
        private DevSpecialization specializationOfRotation;
        private DevRotationType rotationTypeOfRotation;
        private string name;
        private bool isEnabled;

        private DeveloperRotationStatusCode status = DeveloperRotationStatusCode.IDLE;
        private Semaphore syncSemaphore;

        public DeveloperRotationStatusCode Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged("Status");
                OnPropertyChanged("StatusText");
                OnPropertyChanged("IdleVisibility");
                OnPropertyChanged("ActiveVisibility");
            }
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        public Visibility IsShowInCollection
        {
            get => isShowInCollection;
            set
            {
                isShowInCollection = value;
                OnPropertyChanged("IsShowInCollection");
            }
        }

        public DevClass ClassOfRotation
        {
            get => classOfRotation;
            set
            {
                classOfRotation = value;
                OnPropertyChanged("ClassOfRotation");
            }
        }
        public DevRoleType RoleTypeOfRotation
        {
            get => roleTypeOfRotation;
            set
            {
                roleTypeOfRotation = value;
                OnPropertyChanged("RoleTypeOfRotation");
            }
        }

        public DevRotationType RotationTypeOfRotation
        {
            get => rotationTypeOfRotation;
            set
            {
                rotationTypeOfRotation = value;
                OnPropertyChanged("RotationTypeOfRotation");
            }
        }
        public DevSpecialization SpecializationOfRotation
        {
            get => specializationOfRotation;
            set
            {
                specializationOfRotation = value;
                OnPropertyChanged("SpecializationOfRotation");
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public string Path
        {
            get
            {
                if (string.IsNullOrEmpty(rootPath) ||
                    string.IsNullOrEmpty(rotationTypeOfRotation?.Name ?? null) ||
                    string.IsNullOrEmpty(classOfRotation?.Value ?? null) ||
                    string.IsNullOrEmpty(Name)) return "";
                return $"{rootPath}\\{rotationTypeOfRotation.Name}\\{classOfRotation.Value}\\{name.ToLower().Replace(' ', '-')}";
            }
        }

        public string PathToClone
        {
            get
            {
                if (string.IsNullOrEmpty(rootPath) ||
                    string.IsNullOrEmpty(rotationTypeOfRotation?.Name ?? null) ||
                    string.IsNullOrEmpty(classOfRotation?.Value ?? null) ||
                    string.IsNullOrEmpty(Name)) return "";
                return $"{rootPath}\\{rotationTypeOfRotation.Name}\\{classOfRotation.Value}";
            }
        }

        public string TocPath
        {
            get
            {
                if (string.IsNullOrEmpty(rootPath) ||
                    string.IsNullOrEmpty(rotationTypeOfRotation?.Name ?? null) ||
                    string.IsNullOrEmpty(classOfRotation?.Value ?? null) ||
                    string.IsNullOrEmpty(Name)) return "";
                if (File.Exists($"{rootPath}\\{rotationTypeOfRotation.Name}\\{classOfRotation.Value}\\{name.ToLower().Replace(' ', '-')}\\{name}.toc"))
                    return $"{rootPath}\\{rotationTypeOfRotation.Name}\\{classOfRotation.Value}\\{name.ToLower().Replace(' ', '-')}\\{name}.toc";
                else if (Directory.Exists(Path + $"\\.git"))
                {
                    string[] names = DeveloperRotationCore.GetAllFilesWithSpecifiedExtension(Path, ".toc");
                    if (names.Length > 0)
                        return names[0];
                }
                return "";
            }
        }
        public bool IsEnabledChangingOfIsEnabled
        {
            get => string.IsNullOrEmpty(TocPath) ? false : File.Exists(TocPath);
        }
        public Visibility IdleVisibility
        {
            get => Status == DeveloperRotationStatusCode.IDLE ? Visibility.Visible : Visibility.Hidden;
        }
        public Visibility ActiveVisibility
        {
            get => Status != DeveloperRotationStatusCode.IDLE ? Visibility.Visible : Visibility.Hidden;
        }
        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    default:
                    case DeveloperRotationStatusCode.IDLE:
                        return "";
                    case DeveloperRotationStatusCode.PREPARING_TO_SYNC:
                        return Localizator.GetLocalizationResourceByKey("PreparingToSync").Value;
                    case DeveloperRotationStatusCode.WAITING_SYNC:
                        return Localizator.GetLocalizationResourceByKey("WaitingSync").Value;
                    case DeveloperRotationStatusCode.CHECKING_DIRS:
                        return Localizator.GetLocalizationResourceByKey("CheckingDirs").Value;
                    case DeveloperRotationStatusCode.GIT_STARTING:
                        return Localizator.GetLocalizationResourceByKey("GitStart").Value;
                    case DeveloperRotationStatusCode.CLONING:
                        return "Cloning";
                    case DeveloperRotationStatusCode.PULLING:
                        return "Pulling";
                    case DeveloperRotationStatusCode.ERROR_SYNC:
                        return Localizator.GetLocalizationResourceByKey("Error").Value;
                    case DeveloperRotationStatusCode.SUCCESS_SYNC:
                        return Localizator.GetLocalizationResourceByKey("Success").Value;

                }
            }
        }
        public DeveloperRotation(string gitUrl, string rootPath, Semaphore sem)
        {
            this.gitUrl = gitUrl;
            this.rootPath = rootPath;
            syncSemaphore = sem;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void CheckDirs()
        {
            DeveloperRotationStatusCode prevCode = Status;
            Status = DeveloperRotationStatusCode.CHECKING_DIRS;
            string path = $"{rootPath}\\{rotationTypeOfRotation?.Name}";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += $"\\{classOfRotation.Value}";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += $"\\{name.ToLower().Replace(' ', '-')}";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path += "\\";
            Status = prevCode;
        }

        public void WaitSync()
        {
            if (Status == DeveloperRotationStatusCode.IDLE)
            {
                Status = DeveloperRotationStatusCode.WAITING_SYNC;
            }
        }
        public void StopWaitSync()
        {
            if (Status == DeveloperRotationStatusCode.WAITING_SYNC)
            {
                Status = DeveloperRotationStatusCode.IDLE;
            }
        }
        public void Sync()
        {
            if (string.IsNullOrEmpty(Path)) return;
            Status = DeveloperRotationStatusCode.WAITING_SYNC;
            syncSemaphore?.WaitOne();
            try
            {
                Status = DeveloperRotationStatusCode.PREPARING_TO_SYNC;
                CheckDirs();
                Status = DeveloperRotationStatusCode.GIT_STARTING;
                if (File.Exists(TocPath))
                {
                    pullRepo();
                }
                else
                {
                    cloneRepo();
                }
            }
            finally
            {
                Thread.Sleep(5000);
                Status = DeveloperRotationStatusCode.IDLE;
            }
            syncSemaphore?.Release();
        }

        private void cloneRepo()
        {
            if (string.IsNullOrEmpty(PathToClone))
            {
                LogWarn("Developer Rotation Service", $"Ошибка создания репозитория", $"Отсутствует путь для клонирования репозитория");
                Status = DeveloperRotationStatusCode.ERROR_SYNC;
            }
            Status = DeveloperRotationStatusCode.CLONING;
            Process gitCloneProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = "git",
                WorkingDirectory = PathToClone,
                Arguments = $"clone --remote-submodules --recursive --branch=dev {gitUrl}",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            });
            gitCloneProcess.WaitForExit();
            if (gitCloneProcess.ExitCode != 0)
            {
                LogWarn("Developer Rotation Service", $"Ошибка создания репозитория {Path}", $"Код эавершения: {gitCloneProcess.ExitCode}");
                Status = DeveloperRotationStatusCode.ERROR_SYNC;
            }
            else
            {
                LogInfo("Developer Rotation Service", $"Репозиторй создан и синхронизирован успешно {Path}");
                Status = DeveloperRotationStatusCode.SUCCESS_SYNC;
                OnPropertyChanged("IsEnabledChangingIsEnabled");
            }
            
        }

        private void pullRepo()
        {
            Status = DeveloperRotationStatusCode.PULLING;
            Process gitCloneProcess = Process.Start(new ProcessStartInfo()
            {
                FileName = "git",
                WorkingDirectory = Path + $"\\{name}",
                Arguments = $"pull origin dev",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            });
            gitCloneProcess.WaitForExit();
            if (gitCloneProcess.ExitCode != 0)
            {
                LogWarn("Developer Rotation Service", $"Ошибка синхронизациии репозитория {Path}", $"Код эавершения: {gitCloneProcess.ExitCode}");
                Status = DeveloperRotationStatusCode.ERROR_SYNC;
            }
            else
            {
                LogInfo("Developer Rotation Service", $"Репозиторй синхронизирован успешно {Path}");
                Status = DeveloperRotationStatusCode.SUCCESS_SYNC;
                OnPropertyChanged("IsEnabledChangingIsEnabled");
            }
        }

        private RelayCommand syncCommand;
        public RelayCommand SyncCommand
        {
            get
            {
                return syncCommand ?? (syncCommand = new RelayCommand(() =>
                {
                    Task.Run(() => Sync());
                }));
            }
        }
        private RelayCommand openRepoCommand;
        public RelayCommand OpenRepoCommand
        {
            get
            {
                return openRepoCommand ?? (openRepoCommand = new RelayCommand(() =>
                {
                    if (string.IsNullOrEmpty(Path)) return;
                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = false,
                        CreateNoWindow = false,
                        FileName = "explorer.exe",
                        Arguments = Directory.Exists(Path) ? $"\"{Path}\"" : $"\"{PathToClone}\"",
                        WindowStyle = ProcessWindowStyle.Normal,
                    });
                }));
            }
        }
    }
    public enum DeveloperRotationStatusCode
    {
        IDLE = 0,
        WAITING_SYNC = 6,
        PREPARING_TO_SYNC = 5,
        CHECKING_DIRS = 1,
        GIT_STARTING = 2,
        PULLING = 3,
        CLONING = 4,
        ERROR_SYNC = 7,
        SUCCESS_SYNC = 8,
    }

    public class DevSpecialization
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public DevSpecialization(string val, string name)
        {
            Name = name;
            Value = val;
        }
    }
    public class DevRotationType
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public DevRotationType(int val, string name)
        {
            Name = name;
            Value = val;
        }
    }

    public class DevRoleType
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public DevRoleType(string val, string name)
        {
            Name = name;
            Value = val;
        }
    }

    public class DevClass
    {
        public string Value { get; set; }
        public string Name { get; set; }
        public DevClass(string val, string name)
        {
            Name = name;
            Value = val;
        }
    }
}
