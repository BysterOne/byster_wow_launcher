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

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
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
            if(SettingsViewModel.MainViewModel.UserInfo.UserType == BranchType.DEVELOPER)
            {
                this.Height = masterGrid.Height + nameGrid.Height + testElementGrid.Height + 30;
            }
            else if(SettingsViewModel.MainViewModel.UserInfo.UserType == BranchType.MASTER)
            {
                consoleSwitch.Visibility = Visibility.Collapsed;
                testElementGrid.Visibility = Visibility.Collapsed;
                this.Height = masterGrid.Height + nameGrid.Height + 30;
            }
            else if(SettingsViewModel.MainViewModel.UserInfo.UserType == BranchType.TEST)
            {
                loadTypeSelector.Visibility = Visibility.Collapsed;
                rotationSettingsBtn.Visibility = Visibility.Collapsed;
                this.Height = masterGrid.Height + nameGrid.Height + testElementGrid.Height + 30;
            }
            switch (viewModel.UserInfo.Branch.ToLower())
            {
                case "dev":
                    SettingsViewModel.SelectedBranch = Branch.AllBranches.First((branch) => branch.BranchType == BranchType.DEVELOPER);
                    break;
                case "test":
                    SettingsViewModel.SelectedBranch = Branch.AllBranches.First((branch) => branch.BranchType == BranchType.TEST);
                    break;
                default:
                case "master":                    SettingsViewModel.SelectedBranch = Branch.AllBranches.First((branch) => branch.BranchType == BranchType.MASTER);
                    break;
            }
            SettingsViewModel.SelectedLoadType = SettingsViewModel.MainViewModel.UserInfo.LoadTypes.Find(loadtype => loadtype.Value == SettingsViewModel.MainViewModel.UserInfo.LoadType);
            consoleSwitch.IsChecked = SettingsViewModel.MainViewModel.UserInfo.Console == 1;
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingsViewModel.MainViewModel.UserInfo.SetBranch(SettingsViewModel.SelectedBranch);
            SettingsViewModel.MainViewModel.UserInfo.SetConsole(consoleSwitch.IsChecked ?? false);
            SettingsViewModel.MainViewModel.UserInfo.SetLoadType(SettingsViewModel.SelectedLoadType);
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public Action CloseAction { get; set; }
        public MainWindowViewModel MainViewModel { get; set; }

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
                    Process.Start("https://admin.byster.ru/shop/rotation/");
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

        public Branch SelectedBranch { get; set; }
        public LoadType SelectedLoadType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
