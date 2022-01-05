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

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для ActivateCouponWindow.xaml
    /// </summary>
    public partial class ActivateCouponWindow : Window
    {
        private SettingsViewModel Model { get; set; } = null;

        public ActivateCouponWindow(SettingsViewModel model)
        {
            InitializeComponent();
            Model = model;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            string couponCode = couponCodeTextBox.Text;
            if (string.IsNullOrEmpty(couponCode)) return;
            string status = Model.MainViewModel.Shop.ActivateCoupon(couponCode);
            bool result = status == "success";
            if (result)
            {
                var w = new InfoWindow("Успех", "Купон активирован");
                w.ShowDialog();
                this.DialogResult = true;
            }
            else
            {
                var w = new InfoWindow("Ошибка", $"Произошла ошибка активации купона\n{status}");
                w.ShowDialog();
            }

        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
