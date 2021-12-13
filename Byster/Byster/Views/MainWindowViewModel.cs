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


        public event Action UpdateStarted;
        public event Action UpdateCompleted;
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
                PreTestElementAction = () =>
                {
                    DialogWindow dialogWindow = new DialogWindow("Подтверждение", "Вы собираетесь взять Тест ротации на 2 часа. Тест выдается только 1 раз на 1 ротацию. Вы уверены, что хотите взять его сейчас?");
                    bool result = dialogWindow.ShowDialog() ?? false;
                    return result;
                },
                TestElementSuccessAction = () =>
                {
                    InfoWindow infoWindow = new InfoWindow("Успех", "Тестовая версия продукта успешно получена");
                    infoWindow.ShowDialog();
                },
                TestElementFailAction = () =>
                {
                    InfoWindow infoWindow = new InfoWindow("Ошибка", "Произошла ошибка при получении тестовой версии продукта");
                    infoWindow.ShowDialog();
                },
                CloseElementAction = () =>
                {
                    SelectedProduct = null;
                },
                PreBuyCartAction = () =>
                {
                    PaymentSystemSelectorWindow selectorWindow = new PaymentSystemSelectorWindow("Выбор платёжной системы", "Выберите платёжную систему для оплаты", Shop.GetAllPaymentSystemsList());
                    bool res = selectorWindow.ShowDialog() ?? false;
                    if(res)
                    {
                        return selectorWindow.SystemId;
                    }
                    return -1;
                    
                },
                BuyCartSuccessAction = (string str) =>
                {
                    LinkPresenterWindow infoWindow = new LinkPresenterWindow("", str);
                    infoWindow.ShowDialog();
                    Shop.ClearCart();
                },
                BuyCartFailAction = () =>
                {
                    InfoWindow infoWindow = new InfoWindow("Ошибка", "Произошла ошибка при покупке товара, попробуйте позже...");
                    infoWindow.ShowDialog();
                },
                SessionId = sessionId,
            };
            ActionService = new ActionService(restService, UpdateData)
            {
                SessionId = sessionId,
                Dispatcher = App.Current.MainWindow.Dispatcher,
            };
            ActionService.Init();
            SessionService = new SessionService(App.Rest, App.Current.MainWindow.Dispatcher);
            updateAction();
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
                        settingsWindow.Show();
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

        private void updateAction()
        {
            UserInfo.UpdateData();
            Shop.UpdateData();
            ActiveRotations.UpdateData();
            syncData();
        }

        public void UpdateData()
        {
            Task.Run(() =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    UpdateStarted?.Invoke();
                });
                UserInfo.UpdateRemoteData();
                Shop.UpdateData();
                syncData();
                App.Current.Dispatcher.Invoke(() =>
                {
                    UpdateCompleted?.Invoke();
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
            if (property == "SelectedSession")
            {
                ActiveRotations.FilterClass = SelectedSession?.SessionClass?.EnumWOWClass ?? WOWClasses.ANY;
                if (ActiveRotations.FilteredActiveRotations.Count > 0)
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
}
