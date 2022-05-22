using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using Byster.Views;
using System.Diagnostics;
using System.Windows.Forms;
using Byster.Models.ViewModels;
using System.ComponentModel;
using static Byster.Models.Utilities.BysterLogger;
using Byster.Localizations.Tools;

namespace Byster.Models.Services
{

    public class DeveloperRotation : INotifyPropertyChanged
    {
        private static int enumeratorIds = 0;
        public int Id { get; private set; } = enumeratorIds++;

        private string rootPath;
        public string gitUrl { get; private set;  }

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
        public bool IsEnabledChangingIsEnabled
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
            if(Status == DeveloperRotationStatusCode.IDLE)
            {
                Status = DeveloperRotationStatusCode.WAITING_SYNC;
            }
        }
        public void StopWaitSync()
        {
            if(Status == DeveloperRotationStatusCode.WAITING_SYNC)
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
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        LogInfo("Developer Rotation Service", $"Репозиторй синхронизирован успешно {Path}");
                        Status = DeveloperRotationStatusCode.SUCCESS_SYNC;
                        Thread.Sleep(5000);
                        OnPropertyChanged("IsEnabledChangingIsEnabled");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(PathToClone))
                    {
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
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            LogInfo("Developer Rotation Service", $"Репозиторй создан и синхронизирован успешно {Path}");
                            Status = DeveloperRotationStatusCode.SUCCESS_SYNC;
                            Thread.Sleep(5000);
                            OnPropertyChanged("IsEnabledChangingIsEnabled");
                        }
                    }
                    else
                    {
                        LogWarn("Developer Rotation Service", $"Ошибка создания репозитория", $"Отсутствует путь для клонирования репозитория");
                        Status = DeveloperRotationStatusCode.ERROR_SYNC;
                        Thread.Sleep(5000);
                    }
                }
            }
            finally
            {
                Status = DeveloperRotationStatusCode.IDLE;
            }
            syncSemaphore?.Release();
        }

        public bool checkGitUrl(string url)
        {
            return url == gitUrl;
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

    public class DeveloperRotationCore
    {
        private readonly string internalConfigurationFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig\\launcherConfiguration.json";
        private readonly string rotationConfigurationFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig\\rotations.json";

        public string BaseDirectory { get; set; }
        public bool IsReadyToSyncronization { get; set; } = false;

        public event Action EmptyConfigurationRead;
        public event Action InitializationStarted;
        public event Action InitializationCompleted;
        public event Action SyncronizationStarted;
        public event Action SyncronizationCompleted;

        public event Action StatusCodeChanged;

        public List<DeveloperRotation> DeveloperRotations { get; set; }

        public RestService RestService { get; set; }
        private DeveloperRotationStatusCodes statusCode = DeveloperRotationStatusCodes.IDLE;
        public DeveloperRotationStatusCodes StatusCode
        {
            get { return statusCode; }
            set
            {
                statusCode = value;
                StatusCodeChanged?.Invoke();
            }
        }

        public string StatusCodeText
        {
            get
            {
                switch (StatusCode)
                {
                    case DeveloperRotationStatusCodes.IDLE:
                    default:
                        return "";
                    case DeveloperRotationStatusCodes.SYNCRONIZATION:
                        return Localizator.GetLocalizationResourceByKey("RepoSynchronization");
                    case DeveloperRotationStatusCodes.INITIALIZATION:
                        return Localizator.GetLocalizationResourceByKey("Initialization");
                    case DeveloperRotationStatusCodes.CHECKING:
                        return Localizator.GetLocalizationResourceByKey("UpdateChecking");
                }
            }
        }

        public void Initialize()
        {
            InitializationStarted?.Invoke();
            StatusCode = DeveloperRotationStatusCodes.INITIALIZATION;
            readConfFile();
            while (!IsReadyToSyncronization) { }
            DeveloperRotations = new List<DeveloperRotation>();
            Task.Run(() => UpdateData());
            InitializationCompleted?.Invoke();
            StatusCode = DeveloperRotationStatusCodes.IDLE;
        }


        private Semaphore autoSyncSemaphore = new Semaphore(3, 3);
        private Thread startAutoSyncThread;
        private void startRotationSync(IEnumerable<DeveloperRotation> developerRotations)
        {
            foreach(var developerRotation in developerRotations)
            {
                developerRotation.WaitSync();
            }
            foreach (var developerRotation in developerRotations)
            {
                autoSyncSemaphore.WaitOne();
                Task.Run(() =>
                {
                    developerRotation.Sync();
                    autoSyncSemaphore.Release();
                });
            }
        }
        private Semaphore semaphore = new Semaphore(3, 3);

        public void UpdateData()
        {
            if (!IsReadyToSyncronization)
            {
                return;
            }
            startAutoSyncThread?.Abort();
            Dictionary<string, bool> rotationDict;
            if (File.Exists(rotationConfigurationFilePath))
                rotationDict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(File.ReadAllText(rotationConfigurationFilePath));
            else
                rotationDict = new Dictionary<string, bool>();
            StatusCode = DeveloperRotationStatusCodes.CHECKING;

            StatusCode = DeveloperRotationStatusCodes.SYNCRONIZATION;
            SyncronizationStarted?.Invoke();
            List<DeveloperRotation> rotationsToAutoSync = new List<DeveloperRotation>();
            var devRotations = RestService.ExecuteDeveloperRotationRequest();
            foreach (var devRotation in devRotations)
            {
                if(DeveloperRotations.Where(rot => rot.gitUrl == devRotation.git_ssh_url).Count() == 0)
                {
                    DeveloperRotation rotation = new DeveloperRotation(devRotation.git_ssh_url, BaseDirectory, semaphore)
                    {
                        ClassOfRotation = Classes.Where(_c => _c.Value.ToLower() == devRotation.klass.ToLower()).FirstOrDefault(),
                        RoleTypeOfRotation = null,
                        RotationTypeOfRotation = Types.Where(_rt => _rt.Name.ToLower() == devRotation.type.ToLower()).FirstOrDefault(),
                        Name = devRotation.name,
                    };
                    rotation.IsEnabled = rotationDict.ContainsKey(rotation.TocPath) ? rotationDict[rotation.TocPath] : false;
                    rotation.CheckDirs();
                    DeveloperRotations.Add(rotation);
                    if(string.IsNullOrEmpty(rotation.TocPath))
                    {
                        rotationsToAutoSync.Add(rotation);
                    }
                }
                else
                {
                    DeveloperRotation rotation = DeveloperRotations.Where(rot => rot.gitUrl == devRotation.git_ssh_url).FirstOrDefault();
                    rotation.StopWaitSync();
                    rotation.ClassOfRotation = Classes.Where(_c => _c.Value.ToLower() == devRotation.klass.ToLower()).FirstOrDefault();
                    rotation.RoleTypeOfRotation = null;
                    rotation.RotationTypeOfRotation = Types.Where(_rt => _rt.Name.ToLower() == devRotation.type.ToLower()).FirstOrDefault();
                    rotation.Name = devRotation.name;
                    if (string.IsNullOrEmpty(rotation.TocPath))
                    {
                        rotationsToAutoSync.Add(rotation);
                    }
                }
            }
            DeveloperRotations.RemoveAll(rotation => devRotations.Where(dr => dr.git_ssh_url == rotation.gitUrl).Count() == 0);
            DeveloperRotations = DeveloperRotations.OrderByDescending(_c => _c.IsEnabledChangingIsEnabled).OrderByDescending(_c => _c.IsEnabled).ToList();
            StatusCode = DeveloperRotationStatusCodes.IDLE;
            startAutoSyncThread = new Thread(() => { startRotationSync(rotationsToAutoSync); })
            {
                Name = "Auto Sync Of Dev Rotations",
                IsBackground = true,
            };
            startAutoSyncThread.Start();
            SyncronizationCompleted?.Invoke();
        }

        public static string[] GetAllFilesWithSpecifiedExtension(string dirPath, string extension)
        {
            List<string> files = new List<string>();
            foreach (var filePath in Directory.GetFiles(dirPath))
            {
                var info = new FileInfo(filePath);
                if (info.Extension == extension) files.Add(filePath);
            }
            foreach (var directoryPath in Directory.GetDirectories(dirPath))
            {
                files = files.Union(GetAllFilesWithSpecifiedExtension(directoryPath, extension)).ToList();
            }
            return files.ToArray();
        }
        private void readConfFile()
        {
            if (!File.Exists(internalConfigurationFilePath))
            {
                EmptyConfigurationRead?.Invoke();
                return;
            }
            var rawConfStr = File.ReadAllText(internalConfigurationFilePath);
            if (string.IsNullOrEmpty(rawConfStr))
            {
                EmptyConfigurationRead?.Invoke();
                return;
            }
            var configuration = JsonConvert.DeserializeObject<JsonConfiguration>(rawConfStr);
            if (string.IsNullOrEmpty(configuration?.baseDir ?? null))
            {
                EmptyConfigurationRead?.Invoke();
                return;
            }
            BaseDirectory = configuration.baseDir;
            IsReadyToSyncronization = true;
        }

        public void ChangeBaseDirectory(string newBaseDir)
        {
            if (!string.IsNullOrEmpty(BaseDirectory))
            {
                Directory.Delete(BaseDirectory, true);
            }
            BaseDirectory = newBaseDir;
            var newConfStr = JsonConvert.SerializeObject(new JsonConfiguration()
            {
                baseDir = BaseDirectory,
            });
            File.WriteAllText(internalConfigurationFilePath, newConfStr);
            IsReadyToSyncronization = true;
        }

        public bool AddRotation(string name,
                                string description,
                                int type,
                                string klass,
                                string spec,
                                string roletype)
        {
            var devRotation = RestService.ExecuteAddRtationRequest(name, description, type, klass, spec, roletype);
            if (devRotation == null)
            {
                return false;
            }
            DeveloperRotation rotation = new DeveloperRotation(devRotation.git_ssh_url, BaseDirectory, semaphore)
            {
                ClassOfRotation = Classes.Where(_c => _c.Value.ToLower() == devRotation.klass).FirstOrDefault(),
                RotationTypeOfRotation = Types.Where(_rt => _rt.Name.ToLower() == devRotation.type.ToLower()).FirstOrDefault(),
                Name = devRotation.name,
                RoleTypeOfRotation = null,
            };
            rotation.IsEnabled = false;
            rotation.CheckDirs();
            DeveloperRotations.Add(rotation);
            return true;
        }

        public void SaveRotations()
        {
            Dictionary<string, bool> devRotationsDict = new Dictionary<string, bool>();
            foreach (var item in DeveloperRotations)
            {
                if (string.IsNullOrEmpty(item.TocPath)) continue;
                devRotationsDict.Add(item.TocPath, item.IsEnabled);
            }
            File.WriteAllText(rotationConfigurationFilePath, JsonConvert.SerializeObject(devRotationsDict));
        }

        public List<DevClass> Classes { get; set; } = new List<DevClass>()
        {
            new DevClass("ANY", Localizator.GetLocalizationResourceByKey("AllClasses")),
            new DevClass("DEATHKNIGHT", Localizator.GetLocalizationResourceByKey("DeathKnightClass")),
            new DevClass("DRUID", Localizator.GetLocalizationResourceByKey("DruidClass")),
            new DevClass("HUNTER", Localizator.GetLocalizationResourceByKey("HunterClass")),
            new DevClass("MAGE", Localizator.GetLocalizationResourceByKey("WizardClass")),
            new DevClass("PALADIN", Localizator.GetLocalizationResourceByKey("PaladinClass")),
            new DevClass("PRIEST", Localizator.GetLocalizationResourceByKey("PriestClass")),
            new DevClass("ROGUE", Localizator.GetLocalizationResourceByKey("RobberClass")),
            new DevClass("SHAMAN", Localizator.GetLocalizationResourceByKey("ShamanClass")),
            new DevClass ("WARLOCK", Localizator.GetLocalizationResourceByKey("WarlockClass")),
            new DevClass("WARRIOR", Localizator.GetLocalizationResourceByKey("WarriorClass")),
        };

        public List<DevRotationType> Types { get; set; } = new List<DevRotationType>()
        {
            new DevRotationType(-1,"Bot"),
            new DevRotationType(0, "PvE"),
            new DevRotationType(1, "PvP"),
            new DevRotationType(2, "Utility"),
            new DevRotationType(3, "Common"),
            new DevRotationType(4, "Core Module"),
            new DevRotationType(-3, "Core Lua"),
        };

        public List<DevSpecialization> Specializations { get; set; } = new List<DevSpecialization>()
        {
            new DevSpecialization("ARMS", "Arms Warrior"),
            new DevSpecialization("FURY", "Fury Warrior"),
            new DevSpecialization("PROTOWAR", "Proto Warrior"),
            // PALADIN
            new DevSpecialization("HOLYPAL", "Holy Paladin"),
            new DevSpecialization("RETRIBUTION", "Ret Paladin"),
            new DevSpecialization("PROTOPAL", "Proto Paladin"),
            // HUNTER
            new DevSpecialization("BM", "Beast Mastery Hunter"),
            new DevSpecialization("MM", "Marksmanship Hunter"),
            new DevSpecialization("SURVIVABILITY", "Survivability Hunter"),
            // ROGUE
            new DevSpecialization("MUTILATION", "Mutilation Rogue"),
            new DevSpecialization ("COMBAT", "Combat Rogue"),
            new DevSpecialization("SUBTLETY", "Subtlety Rogue"),
            // PRIEST
            new DevSpecialization("DISCIPLINE", "Discipline Priest"),
            new DevSpecialization("HOLYPRIEST", "Holy Priest"),
            new DevSpecialization("SHADOW", "Shadow Priest"),
            // DEATHKNIGHT
            new DevSpecialization("BLOOD", "Blood Death Knight"),
            new DevSpecialization("FROST", "Frost Death Knight"),
            new DevSpecialization("UNHOLY", "Unholy Death Knight"),
            // SHAMAN
            new DevSpecialization("ENHANCEMENT", "Enhancement Shaman"),
            new DevSpecialization("ELEMENTAL", "Elemental Shaman"),
            new DevSpecialization("RESTORSHAMAN", "Resto Shaman"),
            // MAGE
            new DevSpecialization("ARCANE", "Arcane Mage"),
            new DevSpecialization("FIRE", "Fire Mage"),
            new DevSpecialization("FROSTMAGE", "Frost Mage"),
            // WARLOCK
            new DevSpecialization("AFFLICTION", "Affli Warlock"),
            new DevSpecialization("DEMONOLOGY", "Demon Warlock"),
            new DevSpecialization("DESTRUCTION", "Destro Warlock"),
            // DRUID
            new DevSpecialization("MOONKIN", "Moonkin Druid"),
            new DevSpecialization("FERAL", "Feral Druid"),
            new DevSpecialization("RESTORDRUID", "Resto Druid"),
        };

        public List<DevRoleType> Roletypes { get; set; } = new List<DevRoleType>()
        {
            new DevRoleType("DPS", "DPS"),
            new DevRoleType("HEAL", "Heal"),
            new DevRoleType("TANK", "Tank"),
        };

    }

    public enum DeveloperRotationStatusCodes
    {
        IDLE = 0,
        SYNCRONIZATION = 1,
        INITIALIZATION = 2,
        CHECKING = 3,
    }



    public class DeveloperRotationService : IService, INotifyPropertyChanged
    {

        public Action CloseDel { get; set; }
        public RestService RestService { get; set; }
        public bool IsInitialized { get; set; }
        public Dispatcher Dispatcher { get; set; }
        public string SessionId { get; set; }

        private DeveloperRotationCore core;

        public Func<bool> PreAddRotationAction { get; set; }
        public Action AddRotationSuccess { get; set; }
        public Action AddRotationFail { get; set; }

        private string searchRequest;

        public string SearchRequest
        {
            get => searchRequest;
            set
            {
                searchRequest = value;
                OnPropertyChanged("SearchRequest");
            }
        }

        public string StatusCodeText
        {
            get
            {
                return core.StatusCodeText;
            }
        }

        public List<DeveloperRotation> DeveloperRotations
        {
            get
            {
                return core.DeveloperRotations;
            }
            set
            {
                core.DeveloperRotations = value;
            }
            
        }
        public void Initialize(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            core = new DeveloperRotationCore()
            {
                RestService = this.RestService,
            };
            core.EmptyConfigurationRead += () =>
            {
                LogInfo("Developer Rotation Service", "Конфигурация не найдена. Установка нового пути для сохранения");
                Dispatcher.Invoke(() =>
                {
                    FolderBrowserDialog dialog = new FolderBrowserDialog();
                    dialog.ShowNewFolderButton = true;
                    dialog.Description = Localizator.GetLocalizationResourceByKey("SelectRepoForDevRotations").Value;
                    DialogResult dialogResult = DialogResult.None;
                    do
                    {
                        dialogResult = dialog.ShowDialog();
                    }
                    while (dialogResult != DialogResult.OK);
                    core.ChangeBaseDirectory(dialog.SelectedPath);
                });
            };
            core.StatusCodeChanged += () =>
            {
                IsReadyForUsing = core.StatusCode == DeveloperRotationStatusCodes.IDLE ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged("StatusCodeText");
            };
            core.Initialize();
            IsInitialized = true;
        }

        public void UpdateData()
        {
            if (!IsInitialized) return;
            SyncronizationStarted?.Invoke();
            core.SaveRotations();
            core.UpdateData();
            OnPropertyChanged("DeveloperRotations");
            SyncronizationCompleted?.Invoke();
        }

        public bool AddRotation(string name,
                                string description,
                                int type,
                                string klass,
                                string spec,
                                string roletype)
        {
            if (!IsInitialized) return false;
            return core.AddRotation(name,
                            description,
                            type,
                            klass,
                            spec,
                            roletype);
        }



        private DevClass selectedClass;
        private DevRotationType selectedType;
        private DevSpecialization selectedSpecialization;
        private DevRoleType selectedRoletype;
        private string enteredDescription;
        private string enteredName;

        public DevClass SelectedClass
        {
            get => selectedClass;
            set
            {
                selectedClass = value;
                OnPropertyChanged("SelectedClass");
            }
        }
        public DevRotationType SelectedType
        {
            get => selectedType;
            set
            {
                selectedType = value;
                OnPropertyChanged("SelectedType");
            }
        }
        public DevSpecialization SelectedSpecialization
        {
            get => selectedSpecialization;
            set
            {
                selectedSpecialization = value;
                OnPropertyChanged("SelectedSpecialization");
            }
        }
        public DevRoleType SelectedRoletype
        {
            get => selectedRoletype;
            set
            {
                selectedRoletype = value;
                OnPropertyChanged("SelectedRoletype");
            }
        }
        public string EnteredDescription
        {
            get => enteredDescription;
            set
            {
                enteredDescription = value;
                OnPropertyChanged("Entereddescription");
            }
        }

        public string EnteredName
        {
            get => enteredName;
            set
            {
                enteredName = value;
                OnPropertyChanged("EnteredName");
            }
        }

        private bool isSpecChoiceEnabled = false;
        private bool isReadyToAddRotation = false;
        public bool IsSpecChoiceEnabled
        {
            get => isSpecChoiceEnabled;
            set
            {
                isSpecChoiceEnabled = value;
                OnPropertyChanged("IsSpecChoiceEnabled");
            }
        }
        public bool IsReadyToAddRotation
        {
            get => isReadyToAddRotation;
            set
            {
                isReadyToAddRotation = value;
                OnPropertyChanged("IsReadyToAddRotation");
            }
        }

        private Visibility isReadyForUsing = Visibility.Visible;
        public Visibility IsReadyForUsing
        {
            get => isReadyForUsing;
            set
            {
                isReadyForUsing = value;
                OnPropertyChanged("IsReadyForUsing");
            }
        }

        private RelayCommand addCommand;
        private RelayCommand saveCommand;
        private RelayCommand searchCommand;
        private RelayCommand syncCommand;
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ?? (addCommand = new RelayCommand(() =>
                {
                    var result = PreAddRotationAction?.Invoke() ?? false;
                    if (!result) return;
                    Task.Run(() =>
                    {
                        if (AddRotation(EnteredName,
                                EnteredDescription,
                                SelectedType.Value,
                                SelectedClass.Value,
                                SelectedSpecialization.Value,
                                SelectedRoletype.Value))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                AddRotationSuccess?.Invoke();
                            });
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                AddRotationFail?.Invoke();
                            });
                        }
                    });
                }));
            }
        }

        public RelayCommand SaveCommand
        {
            get
            {
                return saveCommand ?? (saveCommand = new RelayCommand(() =>
                {
                    core.SaveRotations();
                    CloseDel?.Invoke();
                }));
            }
        }

        public RelayCommand SearchCommand
        {
            get
            {
                return searchCommand ?? (searchCommand = new RelayCommand(() =>
                {
                    if (SearchRequest == null) return;
                    string strToSearch = SearchRequest.ToLower();
                    foreach (var rot in DeveloperRotations)
                    {
                        if (string.IsNullOrEmpty(strToSearch) || rot.Path.ToLower().Contains(strToSearch))
                        {
                            rot.IsShowInCollection = Visibility.Visible;
                        }
                        else
                        {
                            rot.IsShowInCollection = Visibility.Collapsed;
                        }
                    }

                }));
            }
        }

        public RelayCommand SyncCommand
        {
            get
            {
                return syncCommand ?? (syncCommand = new RelayCommand(() =>
                {
                    Task.Run(() => UpdateData());
                }));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if (property == "SelectedType" || property == "SelectedRoletype" || property == "SelectedClass" || property == "SelectedSpecialization" ||
                property == "EnteredName" || property == "EnteredDescription")
            {
                if (SelectedType != null && (SelectedType.Name.ToLower() == "pvp" || SelectedType.Name.ToLower() == "pve"))
                {
                    IsSpecChoiceEnabled = true;
                }
                else
                {
                    isSpecChoiceEnabled = false;
                }

                if (SelectedClass != null && SelectedType != null && SelectedRoletype != null && ((isSpecChoiceEnabled && selectedSpecialization != null) || !isSpecChoiceEnabled) && !string.IsNullOrEmpty(EnteredName) && !string.IsNullOrEmpty(EnteredDescription))
                {
                    IsReadyToAddRotation = true;
                }
                else
                {
                    IsReadyToAddRotation = false;
                }
            }
            if (property == "SearchRequest")
            {
                if (SearchRequest == null) return;
                string strToSearch = SearchRequest.ToLower();
                foreach (var rot in DeveloperRotations)
                {
                    if (string.IsNullOrEmpty(strToSearch) || rot.Path.ToLower().Contains(strToSearch))
                    {
                        rot.IsShowInCollection = Visibility.Visible;
                    }
                    else
                    {
                        rot.IsShowInCollection = Visibility.Collapsed;
                    }
                }
            }
    
        }

    

        public List<DevClass> Classes
        {
            get => core.Classes;
        }

        public List<DevRotationType> Types
        {
            get => core.Types;
        }

        public List<DevSpecialization> Specializations
        {
            get => core.Specializations;
        }

        public List<DevRoleType> Roletypes
        {
            get => core.Roletypes;
        }

        public event Action EmptyConfigurationRead;
        public event Action InitializationStarted;
        public event Action InitializationCompleted;
        public event Action SyncronizationStarted;
        public event Action SyncronizationCompleted;


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
    public class JsonConfiguration
    {
        public string baseDir { get; set; }
    }
}
