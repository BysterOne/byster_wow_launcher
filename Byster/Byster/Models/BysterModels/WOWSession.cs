using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.Utilities;
using System.Runtime.CompilerServices;


namespace Byster.Models.BysterModels
{
    public class SessionWOW : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
                if(property == "WowApp")
                {
                    InjectInfo.ProcessId = (uint)WowApp.Process.Id;
                }
            }
        }

        private WoW wowApp;

        public WoW WowApp
        {
            get
            {
                return wowApp;
            }
            set
            {
                wowApp = value;
                OnPropertyChanged("WowApp");
            }
        }

        private string userName;

        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
                OnPropertyChanged("UserName");
            }
        }
        private string serverName;
        public string ServerName
        {
            get
            {
                return serverName;
            }
            set
            {
                serverName = value;
                OnPropertyChanged("ServerName");
            }
        }
        private ClassWOW sessionClass;
        public ClassWOW SessionClass
        {
            get
            {
                return sessionClass;
            }
            set
            {
                sessionClass = value;
                OnPropertyChanged("SessionClass");
            }
        }

        private InjectInfo injectInfo;
        public InjectInfo InjectInfo
        {
            get
            {
                return injectInfo;
            }
            set
            {
                injectInfo = value;
                OnPropertyChanged("InjectInfo");
            }
        }

        public SessionWOW()
        {
            InjectInfo = new InjectInfo();
        }

        public static WOWClasses ConverterOfClasses(Classes sessionClass)
        {
            Dictionary<Classes, WOWClasses> classDict = new Dictionary<Classes, WOWClasses>()
            {
                {Classes.WARRIOR, WOWClasses.Warrior },
                {Classes.WARLOCK, WOWClasses.Warlock },
                {Classes.SHAMAN, WOWClasses.Shaman },
                {Classes.ROGUE, WOWClasses.Robber },
                {Classes.PRIEST, WOWClasses.Priest },
                {Classes.PALADIN, WOWClasses.Paladin },
                {Classes.MAGE, WOWClasses.Wizard },
                {Classes.HUNTER, WOWClasses.Hunter },
                {Classes.DRUID, WOWClasses.Druid },
                {Classes.DEATHKNIGHT, WOWClasses.DeathKnight }
            };
            if(!classDict.ContainsKey(sessionClass))
            {
                return WOWClasses.ANY;
            }
            else
            {
                return classDict[sessionClass];
            }
        }

    }
}
