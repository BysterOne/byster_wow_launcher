using System;
using System.Collections.Generic;
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
using System.ComponentModel;

using Byster.Models.BysterModels;
using Byster.Models.ViewModels;
using Byster.Models.Services;
using Byster.Models.Utilities;
using System.Diagnostics;
using Byster.Localizations.Tools;
using System.Windows.Threading;

namespace Byster.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsViewModel SettingsViewModel;

        public SettingsWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            SettingsViewModel = new SettingsViewModel()
            {
                MainViewModel = viewModel,
                CloseAction = () =>
                {
                    this.Close();
                }
            };
            this.DataContext = SettingsViewModel;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsViewModel.MainViewModel.UserInfo.UserType == BranchType.DEVELOPER)
            {
                loadTypeText.Visibility = Visibility.Visible;
                loadTypeSelector.Visibility = Visibility.Visible;
                rotationSettingsBtn.Visibility = Visibility.Visible;
                consoleSwitch.Visibility = Visibility.Visible;
                testElementGrid.Visibility = Visibility.Visible;
                encryptSwitch.Visibility = Visibility.Visible;
                this.Height = masterGrid.ActualHeight + nameGrid.ActualHeight + testElementGrid.ActualHeight + 58;
            }
            else if (SettingsViewModel.MainViewModel.UserInfo.UserType == BranchType.TEST)
            {
                loadTypeText.Visibility = Visibility.Collapsed;
                loadTypeSelector.Visibility = Visibility.Collapsed;
                rotationSettingsBtn.Visibility = Visibility.Collapsed;
                consoleSwitch.Visibility = Visibility.Visible;
                testElementGrid.Visibility = Visibility.Visible;
                encryptSwitch.Visibility = Visibility.Visible;
                this.Height = masterGrid.ActualHeight + nameGrid.ActualHeight + testElementGrid.ActualHeight + 58;
            }
            else
            {
                consoleSwitch.Visibility = Visibility.Collapsed;
                testElementGrid.Visibility = Visibility.Collapsed;
                encryptSwitch.Visibility = Visibility.Collapsed;
                loadTypeText.Visibility = Visibility.Collapsed;
                loadTypeSelector.Visibility = Visibility.Collapsed;
                rotationSettingsBtn.Visibility = Visibility.Collapsed;
                this.Height = masterGrid.ActualHeight + nameGrid.ActualHeight + 58;
            }
        }
    }
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public Action CloseAction { get; set; }
        public MainWindowViewModel MainViewModel { get; set; }

        private RelayCommand resetConfigCommand;

        public RelayCommand ResetConfigCommand
        {
            get => resetConfigCommand ?? (resetConfigCommand = new RelayCommand(() =>
            {
                var resetConfigDialog = new DialogWindow(Localizator.GetLocalizationResourceByKey("ResetConfig"), Localizator.GetLocalizationResourceByKey("ResetConfigText"));
                bool result = resetConfigDialog.ShowDialog() ?? false;
                if(result)
                {
                    MainViewModel.UserInfo.ResetConfig();
                    var successInfoWindow = new InfoWindow(Localizator.GetLocalizationResourceByKey("ResetConfig"), Localizator.GetLocalizationResourceByKey("Success"));
                    successInfoWindow.ShowDialog();
                }
            }));
        }

        private RelayCommand clearCacheCommand;
        public RelayCommand ClearCacheCommand
        {
            get => clearCacheCommand ?? (clearCacheCommand = new RelayCommand(() =>
            {
                var clearCacheDialog = new DialogWindow(Localizator.GetLocalizationResourceByKey("ClearCache"), Localizator.GetLocalizationResourceByKey("ClearCacheText"));
                bool result = clearCacheDialog.ShowDialog() ?? false;
                if(result)
                {
                    var statusWindow = new InfoWindow(Localizator.GetLocalizationResourceByKey("ClearCache"), Localizator.GetLocalizationResourceByKey("ClearInProgress"));
                    statusWindow.Show();
                    Task clearCacheTask = new Task(async () =>
                    {
                        bool requestResult = await MainViewModel.UserInfo.ClearCacheAsync();
                        MainViewModel.UserInfo.Dispatcher.Invoke(() =>
                        {
                            statusWindow.Close();
                            if (requestResult)
                            {
                                var successWindow = new InfoWindow(Localizator.GetLocalizationResourceByKey("ClearCache"), Localizator.GetLocalizationResourceByKey("Success"));
                                successWindow.ShowDialog();
                            }
                            else
                            {
                                var errorInfoWindow = new InfoWindow(Localizator.GetLocalizationResourceByKey("ClearCache"), Localizator.GetLocalizationResourceByKey("Error"));
                                errorInfoWindow.ShowDialog();
                            }
                        });
                    });
                    clearCacheTask.Start();
                }
            }));
        }

        private RelayCommand changePwdCommand;
        public RelayCommand ChangePwdCommand
        {
            get
            {
                return changePwdCommand ?? (changePwdCommand = new RelayCommand(() =>
                {
                    ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow(this);
                    changePasswordWindow.ShowDialog();
                }));
            }
        }

        private RelayCommand linkEmailCommand;
        public RelayCommand LinkEmailCommand
        {
            get
            {
                return linkEmailCommand ?? (linkEmailCommand = new RelayCommand(() =>
                {
                    LinkEmailWindow linkEmailWindow = new LinkEmailWindow(this);
                    linkEmailWindow.ShowDialog();
                }));
            }
        }
        private RelayCommand openRotationSettingsWindow;
        public RelayCommand OpenRotationSettingsWindow
        {
            get
            {
                return openRotationSettingsWindow ?? (openRotationSettingsWindow = new RelayCommand(() =>
                {
                    RotationSettingsWindow window = new RotationSettingsWindow(this.MainViewModel);
                    CloseAction();
                    window.ShowModalDialog();
                }));
            }
        }

        private RelayCommand openAccountPageCommand;
        public RelayCommand OpenAccountPageCommand
        {
            get
            {
                return openAccountPageCommand ?? (openAccountPageCommand = new RelayCommand(() =>
                {
                    Process.Start("https://admin.byster.one/shop/rotation/");
                }));
            }
        }

        private RelayCommand activateCouponCommand;
        public RelayCommand ActivateCouponCommand
        {
            get
            {
                return activateCouponCommand ?? (activateCouponCommand = new RelayCommand(() =>
                {
                    ActivateCouponWindow window = new ActivateCouponWindow(this);
                    window.ShowDialog();
                }));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
