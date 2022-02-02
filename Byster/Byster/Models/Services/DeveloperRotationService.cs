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

namespace Byster.Models.Services
{

    public class DeveloperRotation : INotifyPropertyChanged
    {
        private Visibility isShowInCollection = Visibility.Visible;
        private string path;
        private string displayedPath;
        private bool isEnabled;

        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged("Path");
            }
        }
        public string DisplayedPath
        {
            get => displayedPath;
            private set
            {
                displayedPath = value;
                OnPropertyChanged("DisplayedPath");
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if(property == "Path")
            {
                if (Path != null)
                {
                    if (Path.Split('\\').Length > 3)
                    {
                        string str = "";
                        string[] parts = Path.Split('\\');
                        for (int i = parts.Length - 1; i > parts.Length - 4; i--)
                        {
                            str = "\\" + parts[i] + str;
                        }
                        str = ".." + str;
                        DisplayedPath = str;
                    }
                    else
                    {
                        DisplayedPath = Path;
                    }
                }
            }
        }
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
        public event Action<int, int, List<string>> SynchronizationErrorDetected;

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
                switch(StatusCode)
                {
                    case DeveloperRotationStatusCodes.IDLE:
                    default:
                        return "";
                    case DeveloperRotationStatusCodes.SYNCRONIZATION:
                        return "Синхронизация репозиториев";
                    case DeveloperRotationStatusCodes.INITIALIZATION:
                        return "Инициализация";
                    case DeveloperRotationStatusCodes.CHECKING:
                        return "Проверка обновлений";
                }
            }
        }

        public void Initialize()
        {
            InitializationStarted?.Invoke();
            StatusCode = DeveloperRotationStatusCodes.INITIALIZATION;
            readConfFile();
            while(!IsReadyToSyncronization) { }
            DeveloperRotations = new List<DeveloperRotation>();
            Task.Run(() => UpdateData());
            InitializationCompleted?.Invoke(); 
            StatusCode = DeveloperRotationStatusCodes.IDLE;
        }

        Semaphore semaphore = new Semaphore(3, 3);

        public void UpdateData()
        {
            if (!IsReadyToSyncronization)
            {
                return;
            }
            DeveloperRotations.Clear();
            var errors = new List<string>();
            int counterRepositories = 0;
            int counterErrorRepositories = 0;
            int counterTrigger = 0;
            StatusCode = DeveloperRotationStatusCodes.CHECKING;
            
            if (!File.Exists(rotationConfigurationFilePath)) File.Create(rotationConfigurationFilePath).Close();
            string rawRotationsConf = File.ReadAllText(rotationConfigurationFilePath);
            Dictionary<string, bool> devRotationsdict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(rawRotationsConf);
            if (devRotationsdict != null)
            {
                foreach(var key in devRotationsdict.Keys)
                {
                    DeveloperRotations.Add(new DeveloperRotation()
                    {
                        IsEnabled = devRotationsdict[key],
                        Path = key,
                    });
                }
            }
            StatusCode = DeveloperRotationStatusCodes.SYNCRONIZATION;
            SyncronizationStarted?.Invoke();
            var devRotations = RestService.ExecuteDeveloperRotationRequest();
            foreach (var devRotation in devRotations)
            {
                if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type)) Directory.CreateDirectory(BaseDirectory + "\\" + devRotation.type);
                if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass)) Directory.CreateDirectory(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass);
                if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name)) Directory.CreateDirectory(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name);
                semaphore.WaitOne();
                if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name + "\\.git"))
                {
                    string path = BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name;

                    counterTrigger++;
                    
                    Task.Run(() =>
                    {
                        Log("Синхронизация репозитория - создание репозитория", path);
                        counterRepositories++;
                        Process gitCloneProcess = Process.Start(new ProcessStartInfo()
                        {
                            FileName = "git",
                            WorkingDirectory = path,
                            Arguments = $"clone --remote-submodules --recursive --branch=dev {devRotation.git_ssh_url}",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden,
                        });
                        gitCloneProcess.WaitForExit();
                        if (gitCloneProcess.ExitCode != 0)
                        {
                            counterErrorRepositories++;
                            Log("Ошибка синхронизациии репозитория", $"Код эавершения: {gitCloneProcess.ExitCode}");
                            errors.Add($"Ошибка синхронизации репозитория {path}");
                        }
                        counterTrigger--;
                        
                        string[] pathes = getAllFilesWithSpecifiedExtension(path, ".toc");
                        foreach (string filePath in pathes)
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            if (fileInfo.Extension == ".toc")
                            {
                                if (DeveloperRotations.Find(item => item.Path == filePath) != null) continue;
                                DeveloperRotations.Add(new DeveloperRotation()
                                {
                                    Path = filePath,
                                    IsEnabled = false,
                                });
                            }
                        }
                        Log("Синхронизация репозитория завершена", path);
                        semaphore.Release();
                    });
                }
                else
                {
                    string path = BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name;

                    counterTrigger++;
                    Task.Run(() =>
                    {
                        Log("Синхронизация репозитория", path);
                        counterRepositories++;
                        Process gitCloneProcess = Process.Start(new ProcessStartInfo()
                        {
                            FileName = "git",
                            WorkingDirectory = path,
                            Arguments = $"pull origin dev",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WindowStyle = ProcessWindowStyle.Hidden,
                        });
                        gitCloneProcess.WaitForExit();
                        if (gitCloneProcess.ExitCode != 0)
                        {
                            counterErrorRepositories++;
                            Log("Ошибка синхронизациии репозитория", $"Код эавершения: {gitCloneProcess.ExitCode}");
                            errors.Add($"Ошибка синхронизации репозитория {path}");
                        }
                        counterTrigger--;

                        string[] pathes = getAllFilesWithSpecifiedExtension(path, ".toc");
                        foreach (string filePath in pathes)
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            if (fileInfo.Extension == ".toc")
                            {
                                if(DeveloperRotations.Find(item => item.Path == filePath) != null) continue;
                                DeveloperRotations.Add(new DeveloperRotation()
                                {
                                    Path = filePath,
                                    IsEnabled = false,
                                });
                            }
                        }
                        Log("Синхронизация репозитория завершена", path);
                        semaphore.Release();
                    });
                }
            }
            while (counterTrigger != 0)
            {
                Thread.Sleep(1);
            }
            SaveRotations();
            StatusCode = DeveloperRotationStatusCodes.IDLE;
            SyncronizationCompleted?.Invoke();
            if (errors.Count > 0)
            {
                SynchronizationErrorDetected?.Invoke(counterRepositories, counterErrorRepositories, errors);
            }
        }

        private string[] getAllFilesWithSpecifiedExtension(string dirPath, string extension)
        {
            List<string> files = new List<string>();
            foreach(var filePath in Directory.GetFiles(dirPath))
            {
                var info = new FileInfo(filePath);
                if (info.Extension == extension) files.Add(filePath);
            }
            foreach(var directoryPath in Directory.GetDirectories(dirPath))
            {
                files = files.Union(getAllFilesWithSpecifiedExtension(directoryPath, extension)).ToList();
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
            if(!string.IsNullOrEmpty(BaseDirectory))
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
            if(devRotation == null)
            {
                return false;
            }
            if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type)) Directory.CreateDirectory(BaseDirectory + "\\" + devRotation.type);
            if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass)) Directory.CreateDirectory(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass);
            if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name)) Directory.CreateDirectory(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name);
                semaphore.WaitOne();
            string path = BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name;               
            Task.Run(() =>
            {
                Log("Синхронизация репозитория - создание репозитория - создание новой ротации", path);
                Process gitCloneProcess = Process.Start(new ProcessStartInfo()
                {
                    FileName = "git",
                    WorkingDirectory = path,
                    Arguments = $"clone --remote-submodules --recursive --branch=dev {devRotation.git_ssh_url}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                });
                gitCloneProcess.WaitForExit();
                if (gitCloneProcess.ExitCode != 0)
                {
                    Log("Ошибка синхронизациии репозитория", $"Код эавершения: {gitCloneProcess.ExitCode}");
                }           
                string[] pathes = getAllFilesWithSpecifiedExtension(path, ".toc");
                foreach (string filePath in pathes)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Extension == ".toc")
                    {
                        if (DeveloperRotations.Find(item => item.Path == filePath) != null) continue;
                        DeveloperRotations.Add(new DeveloperRotation()
                        {
                            Path = filePath,
                            IsEnabled = false,
                        });
                    }
                }
                Log("Синхронизация репозитория завершена", path);
                SaveRotations();
                semaphore.Release();
            });
            return true;
        }

        public void SaveRotations()
        {
            Dictionary<string, bool> devRotationsDict = new Dictionary<string, bool>();
            foreach (var item in DeveloperRotations)
            {
                devRotationsDict.Add(item.Path, item.IsEnabled);
            }
            File.WriteAllText(rotationConfigurationFilePath, JsonConvert.SerializeObject(devRotationsDict));
        }

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
                Log("Конфигурация не найдена. Установка нового пути для сохранения");
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.ShowNewFolderButton = true;
                dialog.Description = "Выберите директорию для сохранения ротация для разработчиков";
                DialogResult dialogResult;
                do
                {
                    dialogResult = dialog.ShowDialog();
                }
                while (dialogResult != DialogResult.OK);
                core.ChangeBaseDirectory(dialog.SelectedPath);
            };
            core.StatusCodeChanged += () =>
            {
                IsReadyForUsing = core.StatusCode == DeveloperRotationStatusCodes.IDLE ? Visibility.Collapsed : Visibility.Visible;
                OnPropertyChanged("StatusCodeText");
            };
            core.Initialize();
        }

        public void UpdateData()
        {
            core.UpdateData();
        }

        public bool AddRotation(string name,
                                string description,
                                int type,
                                string klass,
                                string spec,
                                string roletype)
        {
            return core.AddRotation(name,
                            description,
                            type,
                            klass,
                            spec,
                            roletype);
        }

        public List<DevClass> Classes { get; set; } = new List<DevClass>()
        {
            new DevClass("ANY", "Все классы"),
            new DevClass("DEATHKNIGHT", "Рыцарь смерти"),
            new DevClass("DRUID", "Друид"),
            new DevClass("HUNTER", "Охотник"),
            new DevClass("MAGE", "Маг"),
            new DevClass("PALADIN", "Паладин"),
            new DevClass("PRIEST", "Жрец"),
            new DevClass("ROGUE", "Разбойник"),
            new DevClass("SHAMAN", "Шаман"),
            new DevClass ("WARLOCK", "Чернокнижник"),
            new DevClass("WARRIOR", "Воин"),
        };

        public List<DevRotationType> Types { get; set; } = new List<DevRotationType>()
        {
            new DevRotationType(-1,"Bot"),
            new DevRotationType(0, "PvE"),
            new DevRotationType(1, "PvP"),
            new DevRotationType(2, "Utility"),
            new DevRotationType(3, "Common"),
            new DevRotationType(4, "Core Module"),
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
            new DevRoleType("DPS", "ДПС"),
            new DevRoleType("HEAL", "Хилер"),
            new DevRoleType("TANK", "Танк"),
        };

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
                        if(AddRotation(EnteredName,
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


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if(property == "SelectedType" || property == "SelectedRoletype" || property == "SelectedClass" || property == "SelectedSpecialization" ||
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
        }

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
