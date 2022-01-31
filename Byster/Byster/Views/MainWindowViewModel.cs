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
using System.Windows.Threading;

namespace Byster.Views
{

    public static class BysterWindowExtensions
    {
        public static MainWindowViewModel Model { get; set; }
        public static bool ShowModalDialog(this Window window)
        {
            Model.IsModalWindowOpened = Visibility.Visible;
            bool result = window.ShowDialog() ?? false;
            Model.IsModalWindowOpened = Visibility.Collapsed;
            return result;
        }
    }
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
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


        internal RestService restService;

        private string statusText;
        public string StatusText
        {
            get { return statusText; }
            set
            {
                statusText = value;
                OnPropertyChanged("StatusText");
            }
        }

        public event Action UpdateDataStarted;
        public event Action UpdateDataCompleted;

        public event Action InitializationStarted;
        public event Action InitializationCompleted;
        public ActiveRotationsService ActiveRotations { get; set; }
        public ShopService Shop { get; set; }
        public UserInfoService UserInfo { get; set; }
        public ActionService ActionService { get; set; }
        public SessionService SessionService { get; set; }

        private SessionViewModel selectedSession;

        public DeveloperRotationService DeveloperRotations { get; set; }
        public SessionViewModel SelectedSession
        {
            get { return selectedSession; }
            set
            {
                if (selectedSession != null)
                {
                    selectedSession.PropertyChanged -= (o, e) =>
                    {
                        OnPropertyChanged("SelectedSession");
                        OnPropertyChanged("IsSessionDefined");
                        OnPropertyChanged("IsSessionUndefined");
                        OnPropertyChanged("IsWorldUnloaded");
                    };
                }
                selectedSession = value;
                if (selectedSession != null)
                    selectedSession.PropertyChanged += (o, e) =>
                    {
                        OnPropertyChanged("SelectedSession");
                        OnPropertyChanged("IsSessionDefined");
                        OnPropertyChanged("IsSessionUndefined");
                        OnPropertyChanged("IsWorldUnloaded");
                    };
                OnPropertyChanged("SelectedSession");
                OnPropertyChanged("IsSessionDefined");
                OnPropertyChanged("IsSessionUndefined");
                OnPropertyChanged("IsWorldUnloaded");
            }
        }

        public Visibility IsSessionDefined
        {
            get
            {
                if(SelectedSession == null || !SelectedSession.WowApp.WorldLoaded)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }
        public Visibility IsSessionUndefined
        {
            get
            {
                if(SelectedSession == null)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }
        public Visibility IsWorldUnloaded
        {
            get
            {
                if(SelectedSession?.WowApp?.WorldLoaded ?? true)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        private ShopProductInfoViewModel selectedShopProductInfo;
        public ShopProductInfoViewModel SelectedProduct
        {
            get { return selectedShopProductInfo; }
            set
            {
                selectedShopProductInfo = value;
                OnPropertyChanged("SelectedProduct");
            }
        }

        private Visibility isModalWindowOpened = Visibility.Collapsed;
        public Visibility IsModalWindowOpened
        {
            get { return isModalWindowOpened; }
            set
            {
                isModalWindowOpened = value;
                OnPropertyChanged("IsModalWindowOpened");
            }
        }

        private Visibility isMediaOpened = Visibility.Collapsed;
        public Visibility IsMediaOpened
        {
            get { return isMediaOpened; }
            set
            {
                isMediaOpened = value;
                OnPropertyChanged("IsMediaOpened");
            }
        }


        private string sourceOfMediaToOpen = "";
        public string SourceOfMediaToOpen
        {
            get { return sourceOfMediaToOpen; }
            set
            {
                sourceOfMediaToOpen = value;
                OnPropertyChanged("SourceOfMediaToOpen");
            }
        }

        public string LastError
        {
            get
            {
                return restService.LastError;
            }
        }

        public MainWindowViewModel(RestClient client, string sessionId)
        {
            BysterWindowExtensions.Model = this;
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
                PreTestElementAction = () =>
                {
                    DialogWindow dialogWindow = new DialogWindow("Подтверждение", "Вы собираетесь взять Тест ротации на 2 часа. Тест выдается только 1 раз на 1 ротацию. Вы уверены, что хотите взять его сейчас?");
                    bool result = false;
                    result = dialogWindow.ShowModalDialog();
                    return result;
                },
                TestElementSuccessAction = () =>
                {
                    InfoWindow infoWindow = new InfoWindow("Успех", "Тестовая версия продукта успешно получена");
                    infoWindow.ShowModalDialog();
                },
                TestElementFailAction = () =>
                {
                    InfoWindow infoWindow = new InfoWindow("Ошибка", $"Произошла ошибка при получении тестовой версии продукта\n{restService.LastError}");
                    infoWindow.ShowModalDialog();
                },
                CloseElementAction = () =>
                {
                    SelectedProduct = null;
                },
                PreBuyCartAction = () =>
                {
                    PaymentSystemSelectorWindow selectorWindow = new PaymentSystemSelectorWindow("Выбор платёжной системы", "Выберите платёжную систему для оплаты", Shop.GetAllPaymentSystemsList());
                    bool res = false;
                    res = selectorWindow.ShowModalDialog();
                    if (res)
                    {
                        return selectorWindow.SystemId;
                    }
                    return -1;
                },
                BuyCartSuccessAction = (string str) =>
                {
                    LinkPresenterWindow infoWindow = new LinkPresenterWindow("", str);
                    infoWindow.ShowModalDialog();
                    Shop.ClearCart();
                },
                BuyCartFailAction = () =>
                {
                    InfoWindow infoWindow = new InfoWindow("Ошибка", $"Произошла ошибка при покупке товара, попробуйте позже...\n{restService.LastError}");
                    infoWindow.ShowModalDialog();
                },
                PreBuyCartByBonusesAction = () =>
                {
                    DialogWindow dialogWindow = new DialogWindow("Подтверждение", "Вы собираетесь оплатить всю корзину бонусами, Вы уверены?");
                    return dialogWindow.ShowModalDialog();
                },
                BuyCartByBonusesSuccessAction = () =>
                {
                    InfoWindow infoWindow = new InfoWindow("Успех", "Оплата бонусами прошла успешно");
                    infoWindow.ShowModalDialog();
                    Shop.ClearCart();
                },
                BuyCartByBonusesFailAction = () =>
                {
                    InfoWindow infoWindow = new InfoWindow("Ошибка", $"Произошла ошибка при оплате бонусами\n{restService.LastError}");
                    infoWindow.ShowModalDialog();
                },
                SessionId = sessionId,
            };
            ActionService = new ActionService(restService, UpdateData)
            {
                SessionId = sessionId,
            };
            SessionService = new SessionService(App.Rest)
            {
                FirstWowFoundAction = (session) =>
                {
                    SelectedSession = session as SessionViewModel;
                },
            };
            ActiveRotations.AllActiveRotations.CollectionChanged += (o, e) =>
            {
                checkRotations();
            };
            UserInfo.PropertyChanged += (obj, args) =>
            {
                if (args.PropertyName == "Branch")
                {
                    Injector.Branch = UserInfo.Branch;
                };
            };
            MediaControl.OpenAction = (uri) =>
            {
                SourceOfMediaToOpen = uri;
                IsMediaOpened = Visibility.Visible;
            };
            DeveloperRotations = new DeveloperRotationService()
            {
                RestService = restService,
            };
        }

        public void Initialize(Dispatcher dispatcher)
        {
            StatusText = "Инициализация...";
            InitializationStarted?.Invoke();
            ActiveRotations.Initialize(dispatcher);
            Shop.Initialize(dispatcher);
            UserInfo.Initialize(dispatcher);
            ActionService.Initialize(dispatcher);
            SessionService.Initialize(dispatcher);
            if(UserInfo.UserType == BranchType.DEVELOPER)
            {
                Task.Run(() => DeveloperRotations.Initialize(dispatcher));
            }
            UpdateData();
            checkRotations();
            InitializationCompleted?.Invoke();
        }
        private RelayCommand closeMediaCommand;
        public RelayCommand CloseMediaCommand
        {
            get
            {
                return closeMediaCommand ?? (closeMediaCommand = new RelayCommand(() =>
                {
                    SourceOfMediaToOpen = "";
                    IsMediaOpened = Visibility.Collapsed;
                }));
            }
        }

        private RelayCommand settingsCommand;

        private RelayCommand startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return startCommand ?? (startCommand = new RelayCommand((obj) =>
                     {
                         SessionService.StartInjecting(SelectedSession.WowApp.Process.Id);
                     }));
            }
        }
        private RelayCommand unselectSessionCommand;
        public RelayCommand UnselectSessionCommand
        {
            get
            {
                return unselectSessionCommand ?? (unselectSessionCommand = new RelayCommand((obj) =>
                    {
                        SelectedSession = null;
                    }));
            }
        }

        private RelayCommand selectPageCommand;
        public RelayCommand SelectPageCommand
        {
            get
            {
                return selectPageCommand ?? (selectPageCommand = new RelayCommand((obj) =>
                  {
                      int selectedIndex = Convert.ToInt32(obj as string);
                      selectPage(selectedIndex);
                  }));
            }
        }
        public RelayCommand SettingsCommand
        {
            get
            {
                return settingsCommand ??
                (settingsCommand = new RelayCommand(() =>
                    {
                        SettingsWindow settingsWindow = new SettingsWindow(this);
                        settingsWindow.ShowModalDialog();
                    }));
            }
        }

        private RelayCommand shopCommand;
        public RelayCommand ShopCommand
        {
            get
            {
                return shopCommand ?? (shopCommand = new RelayCommand(() =>
                {
                    
                    Shop.FilterOptions = new Filter()
                    {
                        FilterClasses = new ObservableCollection<FilterClass>()
                        {
                            new FilterClass(ActiveRotations.FilterClass),
                        },
                        FilterTypes = new ObservableCollection<string>()
                        {},
                    };
                    selectPage(1);
                }));
            }
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
            if (index >= 0 && index < 3)
                ControlVisibilities[index] = Visibility.Visible;
        }

        private void syncData()
        {

        }

        public void UpdateData()
        {
            Task.Run(() =>
            {
                StatusText = "Обновление данных...";
                App.Current.Dispatcher.Invoke(() =>
                {
                    UpdateDataStarted?.Invoke();
                });
                BackgroundImageDownloader.Suspend();
                try
                {
                    UserInfo.UpdateRemoteData();
                    ActiveRotations.UpdateData();
                    Shop.UpdateData();
                    syncData();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.ToString(), "Ошибка при обновлении данных");
                }
                BackgroundImageDownloader.Resume();
                App.Current.Dispatcher.Invoke(() =>
                {
                    UpdateDataCompleted?.Invoke();
                });
            });
            
        }

        public void Dispose()
        {
            ActionService.Dispose();
            SessionService.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
            if(property == "SelectedSession")
            {
                checkRotations();
            }
        }
        private void checkRotations()
        {
            ActiveRotations.FilterClass = SelectedSession?.SessionClass?.EnumWOWClass ?? WOWClasses.ANY;
            if (ActiveRotations.AllActiveRotations.FirstOrDefault((rotation) => rotation.IsVisibleInList) != null)
            {
                if (SelectedSession == null)
                {
                    selectControls(3);
                    return;
                }
                selectControls(0);
            }
            else
            {
                selectControls(2);
            }
        }
    }
}
