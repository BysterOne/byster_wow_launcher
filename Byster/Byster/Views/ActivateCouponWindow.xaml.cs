using Byster.Localizations.Tools;
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
            titleTextBlock.Text = Localizator.GetLocalizationResourceByKey("ActivateCouponTitle").Value;
            infoTextBlock.Text = Localizator.GetLocalizationResourceByKey("ActivateCouponInfo").Value;
            okBtn.Content = Localizator.GetLocalizationResourceByKey("OK").Value;
            cancelBtn.Content = Localizator.GetLocalizationResourceByKey("Cancel").Value;
            couponCodeTextBox.Tag = Localizator.GetLocalizationResourceByKey("CouponCode").Value;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            string couponCode = couponCodeTextBox.Text;
            if (string.IsNullOrEmpty(couponCode)) return;
            string status = Model.MainViewModel.Shop.ActivateCoupon(couponCode);
            bool result = status == "success";
            if (result)
            {
                var w = new InfoWindow(Localizator.GetLocalizationResourceByKey("Success"), Localizator.GetLocalizationResourceByKey("CouponActivateSuccessMessage").Value);
                w.ShowDialog();
                this.DialogResult = true;
            }
            else
            {
                var w = new InfoWindow(Localizator.GetLocalizationResourceByKey("Error"), $"{Localizator.GetLocalizationResourceByKey("CouponActivateErrorMessage")}\n{status}");
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
