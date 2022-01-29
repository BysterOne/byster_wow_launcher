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

        public Dictionary<string, bool> DeveloperRotations { get; set; }

        public RestService RestService { get; set; }
        public DeveloperRotationStatusCodes StatusCode { get; private set; } = DeveloperRotationStatusCodes.IDLE;

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
            var errors = new List<string>();
            int counterRepositories = 0;
            int counterErrorRepositories = 0;
            int counterTrigger = 0;
            StatusCode = DeveloperRotationStatusCodes.CHECKING;

            if (!File.Exists(rotationConfigurationFilePath)) File.Create(rotationConfigurationFilePath).Close();
            string rawRotationsConf = File.ReadAllText(rotationConfigurationFilePath);
            DeveloperRotations = JsonConvert.DeserializeObject<Dictionary<string, bool>>(rawRotationsConf);
            if (DeveloperRotations == null) DeveloperRotations = new Dictionary<string, bool>();
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
                        });
                        gitCloneProcess.WaitForExit();
                        if (gitCloneProcess.ExitCode != 0)
                        {
                            counterErrorRepositories++;
                            Log("Ошибка синхронизациии репозитория", $"Код эавершения: {gitCloneProcess.ExitCode}");
                            errors.Add($"Ошибка синхронизации репозитория {path}");
                        }
                        counterTrigger--;
                        
                        string[] pathes = Directory.GetFiles(path);
                        foreach (string filePath in pathes)
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            if (fileInfo.Extension == ".toc")
                            {
                                if (DeveloperRotations.ContainsKey(filePath)) continue;
                                DeveloperRotations.Add(filePath, false);
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
                        });
                        gitCloneProcess.WaitForExit();
                        if (gitCloneProcess.ExitCode != 0)
                        {
                            counterErrorRepositories++;
                            Log("Ошибка синхронизациии репозитория", $"Код эавершения: {gitCloneProcess.ExitCode}");
                            errors.Add($"Ошибка синхронизации репозитория {path}");
                        }
                        counterTrigger--;

                        string[] pathes = Directory.GetFiles(path);
                        foreach (string filePath in pathes)
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            if (fileInfo.Extension == ".toc")
                            {
                                if (DeveloperRotations.ContainsKey(filePath)) continue;
                                DeveloperRotations.Add(filePath, false);
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
            StatusCode = DeveloperRotationStatusCodes.IDLE;
            SyncronizationCompleted?.Invoke();
            if (errors.Count > 0)
            {
                SynchronizationErrorDetected?.Invoke(counterRepositories, counterErrorRepositories, errors);
            }
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

        public void AddRotation(string name,
                                string description,
                                int type,
                                string klass,
                                string spec,
                                string roletype)
        {
            var devRotation = RestService.ExecuteAddRtationRequest(name, description, type, klass, spec, roletype);
            
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
                });
                gitCloneProcess.WaitForExit();
                if (gitCloneProcess.ExitCode != 0)
                {
                    Log("Ошибка синхронизациии репозитория", $"Код эавершения: {gitCloneProcess.ExitCode}");
                }           
                string[] pathes = Directory.GetFiles(path);
                foreach (string filePath in pathes)
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Extension == ".toc")
                    {
                        if (DeveloperRotations.ContainsKey(filePath)) continue;
                        DeveloperRotations.Add(filePath, false);
                    }
                }
                Log("Синхронизация репозитория завершена", path);
                semaphore.Release();
            });
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


        public void Initialize(Dispatcher dispatcher)
        {
            core = new DeveloperRotationCore()
            {
                RestService = this.RestService,
            };
            core.EmptyConfigurationRead += () =>
            {
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
            
            core.Initialize();
        }

        public void UpdateData()
        {
            core.UpdateData();
        }

        public void AddRotation(string name,
                                string description,
                                int type,
                                string klass,
                                string spec,
                                string roletype)
        {
            core.AddRotation(name,
                            description,
                            type,
                            klass,
                            spec,
                            roletype);
        }

        public List<(string, string)> Classes = new List<(string, string)>()
        {
            ("ANY", "Все классы"),
            ("DEATHKNIGHT", "Рыцарь смерти"),
            ("DRUID", "Друид"),
            ("HUNTER", "Охотник"),
            ("MAGE", "Маг"),
            ("PALADIN", "Паладин"),
            ("PRIEST", "Жрец"),
            ("ROGUE", "Разбойник"),
            ("SHAMAN", "Шаман"),
            ("WARLOCK", "Чернокнижник"),
            ("WARRIOR", "Воин"),
        };

        public List<(int, string)> Types = new List<(int, string)>()
        {
            (-1,"Bot"),
            (0, "PvE"),
            (1, "PvP"),
            (2, "Utility"),
            (3, "Common"),
            (4, "Core Module"),
        };

        public List<(string, string)> Specializations = new List<(string, string)>()
        {
            ("ARMS", "Arms Warrior"),
            ("FURY", "Fury Warrior"),
            ("PROTOWAR", "Proto Warrior"),
            // PALADIN
            ("HOLYPAL", "Holy Paladin"),
            ("RETRIBUTION", "Ret Paladin"),
            ("PROTOPAL", "Proto Paladin"),
            // HUNTER
            ("BM", "Beast Mastery Hunter"),
            ("MM", "Marksmanship Hunter"),
            ("SURVIVABILITY", "Survivability Hunter"),
            // ROGUE
            ("MUTILATION", "Mutilation Rogue"),
            ("COMBAT", "Combat Rogue"),
            ("SUBTLETY", "Subtlety Rogue"),
            // PRIEST
            ("DISCIPLINE", "Discipline Priest"),
            ("HOLYPRIEST", "Holy Priest"),
            ("SHADOW", "Shadow Priest"),
            // DEATHKNIGHT
            ("BLOOD", "Blood Death Knight"),
            ("FROST", "Frost Death Knight"),
            ("UNHOLY", "Unholy Death Knight"),
            // SHAMAN
            ("ENHANCEMENT", "Enhancement Shaman"),
            ("ELEMENTAL", "Elemental Shaman"),
            ("RESTORSHAMAN", "Resto Shaman"),
            // MAGE
            ("ARCANE", "Arcane Mage"),
            ("FIRE", "Fire Mage"),
            ("FROSTMAGE", "Frost Mage"),
            // WARLOCK
            ("AFFLICTION", "Affli Warlock"),
            ("DEMONOLOGY", "Demon Warlock"),
            ("DESTRUCTION", "Destro Warlock"),
            // DRUID
            ("MOONKIN", "Moonkin Druid"),
            ("FERAL", "Feral Druid"),
            ("RESTORDRUID", "Resto Druid"),
        };
        
        public List<(string, string)> Roletypes = new List<(string, string)>()
        {
            ("DPS", "ДПС"),
            ("HEAL", "Хилер"),
            ("TANK", "Танк"),
        };

        private (string, string) selectedClass;
        private (int, string) selectedType;
        private (string, string) selectedSpecialization;
        private (string, string) selectedRoletype;
        private string enteredDescription;

        public (string, string) SelectedClass
        {
            get => selectedClass;
            set
            {
                selectedClass = value;
                OnPropertyChanged("SelectedClass");
            }
        }
        public (int, string) SelectedType
        {
            get => selectedType;
            set
            {
                selectedType = value;
                OnPropertyChanged("SelectedType");
            }
        }
        public (string ,string) SelectedSpecialization
        {
            get => selectedSpecialization;
            set
            {
                selectedSpecialization = value;
                OnPropertyChanged("SelectedSpecialization");
            }
        }
        public (string, string) SelectedRoletype
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

        private bool isSpecChoiceEnabled = false;
        public bool IsSpecChoiceEnabled
        {
            get => isSpecChoiceEnabled;
            set
            {
                isSpecChoiceEnabled = value;
                OnPropertyChanged("IsSpecChoiceEnabled");
            }
        }

        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ?? (addCommand = new RelayCommand(() =>
                {
                    AddRotation(SelectedClass.Item1,
                                EnteredDescription,
                                SelectedType.Item1,
                                SelectedClass.Item1,
                                SelectedSpecialization.Item1,
                                SelectedRoletype.Item1);
                }));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }

    public class JsonConfiguration
    {
        public string baseDir { get; set; }
    }
}
