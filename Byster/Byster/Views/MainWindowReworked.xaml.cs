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
        MainWindowViewModel ViewModel { get; set; }
        public MainWindowReworked(string login, string sessionId)
        {
            BackgroundPhotoDownloader.Init();
            App.Sessionid = sessionId;

            ViewModel = new MainWindowViewModel(App.Rest, App.Sessionid);
            InitializeComponent();
            this.DataContext = ViewModel;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        private void minimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void ClosingHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BackgroundPhotoDownloader.Close();
            Injector.Close();
        }
        public void AddProductButton(object sender, RoutedEventArgs e)
        {
            ShopProductInfo shopProductInfo = ((sender as Button).DataContext as ShopProductInfo);
            shopProductInfo.AddOne();
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
                    case InjectInfoStatusCode.INJECTED_OK:
                        return "Инжект завершён, перейдите в игру.";

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
