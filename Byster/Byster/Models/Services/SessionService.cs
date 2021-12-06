using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Byster.Models.Utilities;
using Byster.Models.BysterModels;
using Byster.Models.ViewModels;
using System.Windows;
using System.Windows.Threading;
using RestSharp;

namespace Byster.Models.Services
{
    public class SessionService : INotifyPropertyChanged, IDisposable
    {
        public Dispatcher Dispatcher { get; set; }
        private WoWSearcher searcher;

        public ObservableCollection<SessionViewModel> Sessions { get; set; }

        public SessionService(RestClient client,Dispatcher dispatcher)
        {
            Injector.Rest = client;
            Dispatcher = dispatcher;
            Injector.Init();
            searcher = new WoWSearcher("World of Warcraft");
            searcher.OnWowChanged += searcherWowChanged;
            searcher.OnWowFounded += searcherWowFound;
            searcher.OnWowClosed += searcherWowClosed;
            Sessions = new ObservableCollection<SessionViewModel>();
        }

        private bool searcherWowClosed(WoW p)
        {
            Dispatcher.Invoke(() =>
            {
                var sessionToRemove = Sessions.First((session) => session.WowApp.Process.Id == p.Process.Id);
                if (sessionToRemove == null) return;
                Sessions.Remove(sessionToRemove);
            });
            return true;
        }

        private bool searcherWowFound(WoW p)
        {
            Dispatcher.Invoke(() =>
            {
                var sessionToAdd = new SessionViewModel()
                {
                    WowApp = p,
                    UserName = p.Name,
                    ServerName = p.RealmServer,
                    SessionClass = new ClassWOW(SessionWOW.ConverterOfClasses(p.Class)),
                };
                Sessions.Add(sessionToAdd);
            });
            return true;
        }

        private bool searcherWowChanged(WoW p)
        {
            Dispatcher.Invoke(() =>
            {
                var sessionToChange = Sessions.First((session) => session.WowApp.Process.Id == p.Process.Id);
                if (sessionToChange == null) return;
                sessionToChange.WowApp = p;
                sessionToChange.SessionClass = new ClassWOW(SessionWOW.ConverterOfClasses(p.Class));
                sessionToChange.ServerName = p.RealmServer;
                sessionToChange.UserName = p.Name;
            });
            return true;
        }

        public void StartInjecting(int pid)
        {
            var injectingSession = Sessions.First((session) => session.WowApp.Process.Id == pid);
            Injector.AddProcessToInject(injectingSession.InjectInfo);
        }

        public void Dispose()
        {
            searcher.Dispose();
            Injector.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
