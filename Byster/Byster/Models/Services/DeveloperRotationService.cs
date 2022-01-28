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
using static Byster.Models.Utilities.BysterLogger;

namespace Byster.Models.Services
{
    public class DeveloperRotationCore
    {
        private readonly string internalConfigurationFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig\\launcherConfiguration.json";
        private readonly string rotationConfigurationFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig\\rotations.json";

        private int repeatDelay = 30000;
        public string BaseDirectory { get; set; }

        public event Action EmptyConfigurationRead;
        public event Action InitializationStarted;
        public event Action InitializationCompleted;
        public event Action SyncronizationStarted;
        public event Action SyncronizationCompleted;
        public event Action<int, List<string>> SynchronizationErrorDetected;


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

        private Thread internalThread;

        public void Initialize()
        {
            InitializationStarted?.Invoke();
            StatusCode = DeveloperRotationStatusCodes.INITIALIZATION;
            readConfFile();
            internalThread = new Thread(threadFunction);
            internalThread.Name = "Internal Thread Of Developer Rotation Core";
            internalThread.IsBackground = true;
            InitializationCompleted?.Invoke();
            StatusCode = DeveloperRotationStatusCodes.IDLE;
            internalThread.Start();
        }

        private void threadFunction()
        {
            while(true)
            {
                var errors = new List<string>();
                int counterTrigger = 0;
                StatusCode = DeveloperRotationStatusCodes.CHECKING;
                var devRotations = RestService.ExecuteDeveloperRotationRequest();
                foreach(var devRotation in devRotations)
                {
                    if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type)) Directory.CreateDirectory(BaseDirectory + "\\" + devRotation.type);
                    if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass)) Directory.CreateDirectory(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass);
                    if (!Directory.Exists(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name)) Directory.CreateDirectory(BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name);
                    StatusCode = DeveloperRotationStatusCodes.SYNCRONIZATION;
                    Task.Run(() =>
                        {
                            Process gitCloneProcess = Process.Start(new ProcessStartInfo()
                            {
                                FileName = "git",
                                WorkingDirectory = BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name + "\\",
                                Arguments = $"clone --remote-submodules --recursive --branch=dev {devRotation.git_ssh_url}",
                                CreateNoWindow = true,
                            });
                            gitCloneProcess.WaitForExit();
                            MessageBox.Show(gitCloneProcess.ExitCode.ToString());
                        });
                    Log("Синхронизация репозитория: ", BaseDirectory + "\\" + devRotation.type + "\\" + devRotation.klass + "\\" + devRotation.name + "\\");    
                }
                while(counterTrigger != 0)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(repeatDelay);
            }
        }

        private void readConfFile()
        {
            if (!File.Exists(internalConfigurationFilePath)) EmptyConfigurationRead?.Invoke();
            var rawConfStr = File.ReadAllText(internalConfigurationFilePath);
            if (string.IsNullOrEmpty(rawConfStr)) EmptyConfigurationRead?.Invoke();
            var configuration = JsonConvert.DeserializeObject<JsonConfiguration>(rawConfStr);
            if (string.IsNullOrEmpty(configuration?.baseDir ?? null)) EmptyConfigurationRead?.Invoke();
            BaseDirectory = configuration.baseDir;
        }

        public void ChangeBaseDirectory(string newBaseDir)
        {
            BaseDirectory = newBaseDir;
            var newConfStr = JsonConvert.SerializeObject(new JsonConfiguration()
            {
                baseDir = BaseDirectory,
            });
            File.WriteAllText(internalConfigurationFilePath, newConfStr);
        }
    }

    public enum DeveloperRotationStatusCodes
    {
        IDLE = 0,
        SYNCRONIZATION = 1,
        INITIALIZATION = 2,
        CHECKING = 3,
    }
    //public class DeveloperRotationService : IService 
    //{
        
    //}

    public class JsonConfiguration
    {
        public string baseDir { get; set; }
    }
}
