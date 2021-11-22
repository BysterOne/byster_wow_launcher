using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using RestSharp;
using System.Runtime.CompilerServices;
using Byster.Models.BysterModels;
using Byster.Models.Services;
using Byster.Models.Utilities;
using Byster.Models.ViewModels;
using System.Windows;

namespace Byster.Views
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        
        public ObservableCollection<Visibility> PageVisibilities { get; set; } = new ObservableCollection<Visibility>()
        {
            Visibility.Visible,
            Visibility.Collapsed,
        };
        public ObservableCollection<Visibility> ControlVisibilities { get; set; } = new ObservableCollection<Visibility>()
        {
            Visibility.Visible,
            Visibility.Collapsed,
            Visibility.Collapsed,
        };


        private RestService restService;

        public ActiveRotationsService ActiveRotations { get; set; }
        public ShopService Shop { get; set; }
        public UserInfoService UserInfo { get; set; }
        public ActionService ActionService { get; set; }
        public SessionService SessionService { get; set; }

        private SessionViewModel selectedSession;
        public SessionViewModel SelectedSession
        {
            get { return selectedSession; }
            set
            {
                selectedSession = value;
                OnPropertyChanged("SelectedSession");
            }
        }

        public MainWindowViewModel(RestClient client, string sessionId)
        {

            restService = new RestService(client);
            UserInfo = new UserInfoService(restService)
            {
                SessionId = sessionId,
            };
            ActiveRotations = new ActiveRotationsService(restService)
            {
                SessionId = sessionId,
            };
            Shop = new ShopService(restService)
            {
                SessionId = sessionId,
            };
            ActionService = new ActionService(restService, updateAction)
            {
                SessionId = sessionId,
            };
            SessionService = new SessionService(App.Current.MainWindow.Dispatcher);
            updateAction();
        }

        private void selectPage(int index)
        {
            for (int i = 0; i < PageVisibilities.Count; i++)
            {
                PageVisibilities[i] = Visibility.Collapsed;
            }
            PageVisibilities[index] = Visibility.Visible;
        }

        private void selectControls(int index)
        {
            for (int i = 0; i < ControlVisibilities.Count; i++)
            {
                ControlVisibilities[i] = Visibility.Collapsed;
            }
            if(index > 0 && index < 3)
            ControlVisibilities[index] = Visibility.Visible;
        }

        public RelayCommand StartCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                     {
                         SessionService.StartInjecting(SelectedSession.WowApp.Process.Id);
                     });
            }
        }
        public RelayCommand UnselectSessionCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                    {
                        SelectedSession = null;
                    });
            }
        }

        public RelayCommand SelectPageCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                  {
                      int selectedIndex = Convert.ToInt32(obj as string);
                      selectPage(selectedIndex);
                  });
            }
        }
        private void syncData()
        {

        }

        private void updateAction()
        {
            UserInfo.UpdateData();
            Shop.UpdateData();
            ActiveRotations.UpdateData();
            syncData();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if(property == "SelectedSession")
            {
                ActiveRotations.FilterClass = SelectedSession?.SessionClass?.EnumWOWClass ?? WOWClasses.ANY;
                if(ActiveRotations.FilteredActiveRotations.Count > 0)
                {
                    if (SelectedSession == null)
                    { 
                        selectControls(3);
                        return;
                    }
                    if (SelectedSession.InjectInfo.InjectInfoStatusCode == InjectInfoStatusCode.INACTIVE)
                    {
                        selectControls(0);
                    }
                    else
                    {
                        selectControls(1);
                    }
                }
                else
                {
                    selectControls(2);
                }
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public void Execute(object parameter)
        {
            if(_execute != null)
                _execute(parameter);
        }
        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute(parameter);
            else
                return true;
        }

        public RelayCommand(Action<object> executeDel, Func<object, bool> canExecuteDel = null)
        {
            _execute = executeDel;
            _canExecute = canExecuteDel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value;}
        }
    }
}
