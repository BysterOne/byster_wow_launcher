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

using Byster.Models.BysterModels;

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
                return (systemSelector.SelectedItem as PaymentSystem).Id;
            }
        }
        public PaymentSystemSelectorWindow(string title, string infoText, List<PaymentSystem> systems)
        {
            InitializeComponent();
            titleTextBlock.Text = title;
            infoTextBlock.Text = infoText;
            systemSelector.ItemsSource = systems;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if(systemSelector.SelectedItem != null)
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
}
