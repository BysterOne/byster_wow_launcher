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
using Byster.Models.Utilities;
using Byster.Models.Services;
using Byster.Localizations.Tools;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        private SettingsViewModel Model { get; set; } = null;
        public ChangePasswordWindow(SettingsViewModel model)
        {
            InitializeComponent();
            Model = model;
            titleTextBlock.Text = Localizator.GetLocalizationResourceByKey("ChangePasswordTitle");
            newPassrowdBox.Tag = Localizator.GetLocalizationResourceByKey("NewPassword");
            newPassrowdBoxConfirm.Tag = Localizator.GetLocalizationResourceByKey("NewPasswordConfirmation");
            okBtn.Content = Localizator.GetLocalizationResourceByKey("OK").Value;
            cancelBtn.Content = Localizator.GetLocalizationResourceByKey("Cancel").Value;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (newPassrowdBox.Password != newPassrowdBoxConfirm.Password)
            {
                var w = new InfoWindow("Ошибка", "Введённые пароли не совпадают");
                w.ShowDialog();
                return;
            }
            bool result = Model.MainViewModel.UserInfo.ChangePasssword(HashCalc.GetMD5Hash(newPassrowdBox.Password));
            if (result)
            {
                var w = new InfoWindow(Localizator.GetLocalizationResourceByKey("Success"), "Пароль изменён");
                w.ShowDialog();
                this.DialogResult = true;
            }
            else
            {
                var w = new InfoWindow("Ошибка", "Непредвиденная ошибка");
                w.ShowDialog();
            }
                
        }

        private void newPassrowdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            newPassrowdBox.Tag = newPassrowdBox.Password.Length > 0 ? "" : "Новый пароль";
        }
        private void newPassrowdBoxConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            newPassrowdBoxConfirm.Tag = newPassrowdBoxConfirm.Password.Length > 0 ? "" : "Подтверждение пароля";
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
