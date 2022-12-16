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
using Byster.Models.BysterModels;

namespace Byster.Models.Services
{
    public class DeveloperRotationCore
    {
        private readonly string internalConfigurationFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BysterConfig\\launcherConfiguration.json");
        private readonly string rotationConfigurationFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BysterConfig\\rotations.json");

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

        private Semaphore semaphore = new Semaphore(3, 3);
        private Semaphore autoSyncSemaphore = new Semaphore(3, 3);
        private Thread startAutoSyncThread;

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
            StatusCode = DeveloperRotationStatusCodes.IDLE;
            InitializationCompleted?.Invoke();
        }

        private void startRotationSyncThreadTarget(IEnumerable<DeveloperRotation> developerRotations)
        {
            foreach (var developerRotation in developerRotations)
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

        public void UpdateData()
        {
            if (!IsReadyToSyncronization)
            {
                return;
            }
            stopAutoSyncOfRotations();
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
                DeveloperRotation rotation = DeveloperRotations.Where(rot => rot.gitUrl == devRotation.git_ssh_url).FirstOrDefault();
                if (rotation == null)
                {
                    rotation = new DeveloperRotation(devRotation.git_ssh_url, BaseDirectory, semaphore)
                    {
                        ClassOfRotation = Classes.Where(_c => _c.Value.ToLower() == devRotation.klass.ToLower()).FirstOrDefault(),
                        RoleTypeOfRotation = null,
                        RotationTypeOfRotation = Types.Where(_rt => _rt.Name.ToLower() == devRotation.type.ToLower()).FirstOrDefault(),
                        Name = devRotation.name,
                    };
                    rotation.IsEnabled = rotationDict.ContainsKey(rotation.TocPath) ? rotationDict[rotation.TocPath] : false;
                    rotation.CheckDirs();
                    DeveloperRotations.Add(rotation);
                    if (string.IsNullOrEmpty(rotation.TocPath))
                    {
                        rotationsToAutoSync.Add(rotation);
                    }
                }
                else
                {
                    if (rotation.Status == DeveloperRotationStatusCode.WAITING_SYNC)
                    {
                        rotation.StopWaitSync();
                    }
                    else if(rotation.Status != DeveloperRotationStatusCode.IDLE)
                    {
                        rotation.PropertyChanged += getPCEHToUpdatingSingleRotation(devRotation.klass, devRotation.type, devRotation.name);
                    }
                    else
                    {
                        rotation.ClassOfRotation = Classes.Where(_c => _c.Value.ToLower() == devRotation.klass.ToLower()).FirstOrDefault();
                        rotation.RotationTypeOfRotation = Types.Where(_rt => _rt.Name.ToLower() == devRotation.type.ToLower()).FirstOrDefault();
                        rotation.Name = devRotation.name;
                        if (string.IsNullOrEmpty(rotation.TocPath))
                        {
                            rotationsToAutoSync.Add(rotation);
                        }
                    }
                }
            }
            DeveloperRotations.RemoveAll(rotation => devRotations.Where(dr => dr.git_ssh_url == rotation.gitUrl).Count() == 0);
            DeveloperRotations = DeveloperRotations.OrderByDescending(_c => _c.IsEnabledChangingOfIsEnabled).OrderByDescending(_c => _c.IsEnabled).ToList();
            StatusCode = DeveloperRotationStatusCodes.IDLE;
            startAutoSyncOfRotations(rotationsToAutoSync);
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
            var devRotation = RestService.ExecuteAddRotationRequest(name, description, type, klass, spec, roletype);
            if (devRotation == null)
            {
                return false;
            }
            DeveloperRotation rotation = new DeveloperRotation(devRotation.git_ssh_url, BaseDirectory, semaphore)
            {
                ClassOfRotation = Classes.Where(_c => _c.Value.ToLower() == devRotation.klass.ToLower()).FirstOrDefault(),
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

        private void startAutoSyncOfRotations(IEnumerable<DeveloperRotation> rotationsToSync)
        {
            startAutoSyncThread = new Thread(() => { startRotationSyncThreadTarget(rotationsToSync); })
            {
                Name = "Auto Syncronization of Dev Rotations",
                IsBackground = true,
            };
            startAutoSyncThread.Start();
        }

        private void stopAutoSyncOfRotations()
        {
            startAutoSyncThread?.Abort();
        }

        public void Stop()
        {
            stopAutoSyncOfRotations();
        }

        private PropertyChangedEventHandler getPCEHToUpdatingSingleRotation(string klass, string type, string name)
        {
            PropertyChangedEventHandler pceh = null;
            pceh = (s, e) =>
            {
                var rotation = s as DeveloperRotation;
                rotation.PropertyChanged -= pceh;
                rotation.ClassOfRotation = Classes.Where(_c => _c.Value.ToLower() == klass.ToLower()).FirstOrDefault();
                rotation.RotationTypeOfRotation = Types.Where(_rt => _rt.Name.ToLower() == type.ToLower()).FirstOrDefault();
                rotation.Name = name;
            };
            return pceh;
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

        public void Dispose()
        {
            LogInfo("Developer Rotations Service", "Завершение работы сервиса...");
            core.Stop();
        }

        private bool addRotation(string name,
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
                        if (addRotation(EnteredName,
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
    }

    public class JsonConfiguration
    {
        public string baseDir { get; set; }
    }
}
