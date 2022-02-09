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
using Byster.Models.ViewModels;
using Byster.Models.BysterModels;
using System.ComponentModel;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для PaymentSystemSelectorWindow.xaml
    /// </summary>
    public partial class PaymentSystemSelectorWindow : Window
    {
        public int SystemId
        {
            get
            {
                return (systemSelector.SelectedItem as PaymentSystem)?.Id ?? -1;
            }
        }
        public PaymentSystemSelectorWindow(List<PaymentSystem> systems, MainWindowViewModel mainWindowViewModel, Cart carttoBuy)
        {
            InitializeComponent();
            systemSelector.ItemsSource = systems;
            this.DataContext = new PaymentWindowViewModel()
            {
                MainViewModel = mainWindowViewModel,
            };
            mainWindowViewModel.Shop.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != "Sum") return;
                if (mainWindowViewModel.Shop.Sum == 0)
                    Close();
            };
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if(systemSelector.SelectedItem != null || (DataContext as PaymentWindowViewModel).MainViewModel.Shop.ResultSum == 0)
                this.DialogResult = true;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult= false;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }


    public class PaymentWindowViewModel : INotifyPropertyChanged
    {
        private RelayCommand clearCommand;

        private MainWindowViewModel mainViewModel;
        public MainWindowViewModel MainViewModel
        {
            get { return mainViewModel; }
            set
            {
                mainViewModel = value;
                OnPropertyChanged("MainViewModel");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
