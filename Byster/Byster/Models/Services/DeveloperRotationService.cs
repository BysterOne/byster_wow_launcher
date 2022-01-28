using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace Byster.Models.Services
{
    public class DeveloperRotationService : IService 
    {
        private readonly string configurationFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig\\rotations.json";
        private readonly string internalConfigurationFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BysterConfig\\developerConfig.json";


        public string DeveloperRotationsFolderPath { get; private set; }
        public Dispatcher Dispatcher { get; set; }
        public bool IsInitialized { get; set; } = false;
        public string SessionId { get; set; } = "";

        public bool Trigger { get; set; } = false;

        private Thread internalThread;

        public RestService RestService { get; set; }
        public void Initialize(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            IsInitialized = true;
            internalThread = new Thread(threadFunction);
            internalThread.Name = "Developer Rotation Service Thread";
            internalThread.IsBackground = true;
            internalThread.Start();
        }

        public void UpdateData()
        {

        }

        public void ChangeConfigurationFilePath(string path)
        {

        }

        public string ReadConfigurationFile()
        {

        }

        private void threadFunction()
        {

        }
    }
}
