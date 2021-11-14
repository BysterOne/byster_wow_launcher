using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Byster.Models.BysterModels;
using Byster.Models.RestModels;
using Byster.Models.Utilities;
using RestSharp;
using System.Net;
using System.Globalization;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindowReworked.xaml
    /// </summary>
    public partial class MainWindowReworked : Window
    {
        private WoWSearcher searcher;
        MainWindowManager Manager { get; set; }
        public MainWindowReworked(string login)
        {
            BackgroundPhotoDownloader.Init();

            Manager = new MainWindowManager();
            InitializeComponent();
            this.DataContext = Manager;
            Manager.UserName = login;

            searcher = new WoWSearcher("World of Warcraft");
            searcher.OnWowFounded += OnWOWFound;
            searcher.OnWowChanged += OnWOWChanged;
            searcher.OnWowClosed += OnWOWClosed;
            Injector.Init();
            Injector.Rest = App.Rest;

            this.Closing += ClosingHandler;
        }

        

        private bool OnWOWClosed(WoW p)
        {
            Dispatcher.Invoke(() =>
            {
                Manager.SessionsCollection.Remove(Manager.SessionsCollection.First((session) => session.WowApp.Process == p.Process));
            });
            return true;
        }

        private bool OnWOWChanged(WoW p)
        {
            Dispatcher.Invoke(() =>
            {
                SessionWOW changedSession = Manager.SessionsCollection.First((session) => session.WowApp.Process == p.Process);
                changedSession.WowApp = p;
                changedSession.SessionClass = new ClassWOW(SessionWOW.ConverterOfClasses(p.Class));
                changedSession.UserName = p.Name;
                changedSession.ServerName = p.Version;
            });
            return true;
        }

        private bool OnWOWFound(WoW p)
        {
            Dispatcher.Invoke(() =>
            {
                Manager.SessionsCollection.Add(new SessionWOW()
                {
                    SessionClass = new ClassWOW(SessionWOW.ConverterOfClasses(p.Class)),
                    UserName = p.Name,
                    ServerName = p.Version,
                    WowApp = p,
                    InjectInfo = new InjectInfo()
                    {
                        ProcessId = (UInt32)p.Process.Id,
                    }
                });
            });
            return true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void sessionsViewBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Manager.FilterClass = Manager.selectedSession != null ? Manager.selectedSession.SessionClass.EnumWOWClass : WOWClasses.ANY;
            Manager.FilterRotations();
        }

        private void titleButton_Click(object sender, RoutedEventArgs e)
        {
            sessionsViewBox.SelectedItem = null;
        }

        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void shopPageSelect_Clicked(object sender, RoutedEventArgs e)
        {
            Manager.SelectPage(1, true);
        }

        private void rotPageSelect_Clicked(object sender, RoutedEventArgs e)
        {
            Manager.SelectPage(0, true);
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            if(Manager.selectedSession?.WowApp?.Process?.Id != null)
            {
                Injector.AddProcessToInject(Manager.selectedSession.InjectInfo);
            }
        }
        private void ClosingHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BackgroundPhotoDownloader.Close();
            Injector.Close();
            searcher.Dispose();
        }
        public void AddProductButton(object sender, RoutedEventArgs e)
        {
            ShopProductInfo shopProductInfo = ((sender as Button).DataContext as ShopProductInfo);
            shopProductInfo.Add();
        }
        public void RemoveProductButton(object sender, RoutedEventArgs e)
        {
            ShopProductInfo shopProductInfo = ((sender as Button).DataContext as ShopProductInfo);
            shopProductInfo.RemoveOne();
        }
        public void TestProductButton(object sender, RoutedEventArgs e)
        {

        }
    }


    public class MainWindowManager
    {
        public string UserName { get; set; }
        public SessionWOW selectedSession { get; set; }
        public ObservableCollection<SessionWOW> SessionsCollection { get; set; }
        public ObservableCollection<RotationWOW> AllRotations { get; set; }
        public ObservableCollection<RotationWOW> RotationViewCollection { get; set; }

        public ObservableCollection<ShopProductInfo> ProductList { get; set; }
        public WOWClasses FilterClass { get; set; }
        public bool IsInjecting { get; set; }
        public bool IsNotInjecting { get; set; }

        public ObservableCollection<Visibility> ControlVisibilities { get; private set; }
        public ObservableCollection<Visibility> PageVisibilities { get; private set; }

        public MainWindowManager()
        {
            SessionsCollection = new ObservableCollection<SessionWOW>();
            AllRotations = new ObservableCollection<RotationWOW>();
            RotationViewCollection = new ObservableCollection<RotationWOW>();
            ProductList = new ObservableCollection<ShopProductInfo>();


            FilterClass = WOWClasses.ANY;

            ControlVisibilities = new ObservableCollection<Visibility>()
            {
                Visibility.Visible,
                Visibility.Collapsed,
                Visibility.Collapsed,
            };

            PageVisibilities = new ObservableCollection<Visibility>()
            {
                Visibility.Visible,
                Visibility.Collapsed,
            };

            UserName = "Default";
            UpdateRotations();
            UpdateProductList();
            FilterRotations();
        }

        public void SelectPage(int pageIndex, bool condition)
        {
            if(condition)
            {
                PageVisibilities[PageVisibilities.IndexOf(Visibility.Visible)] = Visibility.Collapsed;
                PageVisibilities[pageIndex] = Visibility.Visible;
            }
        }

        public void SelectControls(int controlIndex, bool condition)
        {
            if(condition)
            {
                ControlVisibilities[ControlVisibilities.IndexOf(Visibility.Visible)] = Visibility.Collapsed;
                ControlVisibilities[controlIndex] = Visibility.Visible;
            }
        }

        public void FilterRotations()
        {
            RotationViewCollection.Clear();
            foreach(var rotation in AllRotations)
            {
                if(rotation.RotationClass.EnumWOWClass == FilterClass || rotation.RotationClass.EnumWOWClass == WOWClasses.ANY || FilterClass == WOWClasses.ANY)
                {
                    RotationViewCollection.Add(rotation);
                }
            }
            CheckAvailableRotations();
        }

        public void UpdateRotations()
        {
            AllRotations.Clear();
            var rotationsResponse = App.Rest.Post<List<RestRotationWOW>>(new RestRequest("shop/my_subscriptions"));
            if(rotationsResponse.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Запрос завершён с кодом: {(int)rotationsResponse.StatusCode}\nСообщение ошибки: {rotationsResponse.ErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            foreach( var RestRotationWOW in rotationsResponse.Data)
            {
                AllRotations.Add(new RotationWOW(RestRotationWOW));
            }

        }

        public void CheckAvailableRotations()
        {
            if (RotationViewCollection.Count == 0)
            {
                SelectControls(2, true);
            }
            else
            {
                SelectControls(0, true);
            }
        }

        public void UpdateProductList()
        {
            ProductList.Clear();
            var productListResponse = App.Rest.Post<List<RestShopProduct>>(new RestRequest("shop/product_list"));

            if(productListResponse.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Запрос завершён с кодом: {(int)productListResponse.StatusCode}\nСообщение ошибки: {productListResponse.ErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            foreach(var productResponse in productListResponse.Data)
            {
                ProductList.Add(new ShopProductInfo(new ShopProduct(productResponse)));
            }
        }
    }
    public class FromInjectInfoStatusCodeToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { 
            if(value != null && parameter != null && value is InjectInfoStatusCode && parameter is string)
            {
                InjectInfoStatusCode statusCode = (InjectInfoStatusCode)value;
                int status = (int)statusCode;

                switch(parameter.ToString())
                {
                    case "Default":
                        return status == 0 ? Visibility.Visible : Visibility.Collapsed;
                    case "Active":
                        return status != 0 ? Visibility.Visible : Visibility.Collapsed;
                    case "Enqueued":
                        return status == 1 ? Visibility.Visible : Visibility.Collapsed;
                    case "Downloading":
                        return status == 2 ? Visibility.Visible : Visibility.Collapsed;
                    case "Injecting":
                        return status == 3 ? Visibility.Visible : Visibility.Collapsed;
                        
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class FromInjectInfoStatusCodeToStatusText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is InjectInfoStatusCode)
            {
                InjectInfoStatusCode statusCode = (InjectInfoStatusCode)value;
                int status = (int)statusCode;

                switch (statusCode)
                {
                    case InjectInfoStatusCode.INACTIVE:
                        return "Инжект инактивен...";
                    case InjectInfoStatusCode.ENEQUEUED:
                        return "Ожидание в очереди для инжекта...";
                    case InjectInfoStatusCode.DOWNLOADING:
                        return "Скачивание ядра....";
                    case InjectInfoStatusCode.INJECTING:
                        return "Инжект...";

                }
            }
            return "---";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class FromNullToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
