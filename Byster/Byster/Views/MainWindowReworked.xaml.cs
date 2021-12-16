using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
using Byster.Models.Services;
using System.ComponentModel;

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
            App.Sessionid = sessionId;

            ViewModel = new MainWindowViewModel(App.Rest, App.Sessionid);
            ViewModel.UpdateStarted += () =>
            {
                 updatingDataActionGrid.Visibility = Visibility.Visible;
            };
            ViewModel.UpdateCompleted += () =>
            {
                 updatingDataActionGrid.Visibility = Visibility.Collapsed;
            };
            InitializeComponent();
            this.DataContext = ViewModel;
            FromSumToBonuses.bonuses = ViewModel.UserInfo.BonusBalance;
            ViewModel.UserInfo.PropertyChanged += (o, e) => { FromSumToBonuses.bonuses = ViewModel.UserInfo.BonusBalance; };
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

        private void filterClassListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var removingElement in e.RemovedItems)
            {
                ViewModel.Shop.FilterOptions.FilterClasses.Remove(removingElement as FilterClass);
            }
            foreach (var addingElement in e.AddedItems)
            {
                ViewModel.Shop.FilterOptions.FilterClasses.Add(addingElement as FilterClass);
            }
            ViewModel.Shop.FilterProducts();
        }

        private void filterTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach(var removingElement in e.RemovedItems)
            {
                ViewModel.Shop.FilterOptions.FilterTypes.Remove(removingElement as string);
            }
            foreach(var addingElement in e.AddedItems)
            {
                ViewModel.Shop.FilterOptions.FilterTypes.Add(addingElement as string);
            }
            ViewModel.Shop.FilterProducts();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            ViewModel.Dispose();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.F5)
            {
                ViewModel.UpdateData();
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
                    default:
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
                    case "Injected":
                        return status == 4 ? Visibility.Visible : Visibility.Collapsed;
                        
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

    public class FromSumToBonuses : IValueConverter
    {
        public static int bonuses;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value < bonuses ? (double)value : bonuses;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
