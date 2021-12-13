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

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        private string oldPasswordHash { get; set; } = "";
        private SettingsViewModel Model { get; set; } = null;
        public ChangePasswordWindow(string title, string oldPasswordHash, SettingsViewModel model)
        {
            InitializeComponent();
            this.Title = title;
            titleTextBlock.Text = title;
            this.oldPasswordHash = oldPasswordHash;
            Model = model;
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
                var w = new InfoWindow("Успех", "Пароль изменён");
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
