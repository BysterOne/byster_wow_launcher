using Microsoft.Win32;
using System;
using System.Net;
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
using RestSharp;
using Byster.Models.RestModels;
using Byster.Models.Utilities;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginOrRegistrationWindow.xaml
    /// </summary>
    public partial class LoginOrRegistrationWindow : Window
    {
        public LoginOrRegistrationWindow()
        {
            string login = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", null);
            string password = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", null);

            if(!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                string passwordHash = password;
                if(TryAuth(login, passwordHash))
                {
                    StartMainWindow();
                    Close();
                }
                else
                {
                    InitializeComponent();
                }
            }
        }

        public bool TryAuth(string login, string passwordHash)
        {
            MessageBox.Show($"{login}\n{passwordHash}");

            if(string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if(string.IsNullOrEmpty(passwordHash))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            var response = App.Rest.Post<AuthResponse>(new RestRequest("launcher/login").AddJsonBody(new AuthRequest
            {
                login = login,
                password = passwordHash,
            }));

            if(response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Запрос завершён с кодом: {(int)response.StatusCode}\nСообщение ошибки: {response.ErrorMessage}", "Ошибка HTTP", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if(!string.IsNullOrEmpty(response.Data.error))
            {
                MessageBox.Show(response.Data.error, "Ошибка Byster", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            App.Rest.Authenticator = new BysterAuthenticator(response.Data.session);
            return true;
        }

        public void StartMainWindow()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {

            }));
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void navigateToRegister_Click(object sender, RoutedEventArgs e)
        {
            authBlock.Visibility = Visibility.Collapsed;
            registerBlock.Visibility = Visibility.Visible;
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            string loginText = loginBox.Text;
            string passwordText = passwordBox.Password;
            string passwordHash = HashCalc.GetMD5Hash(passwordText);
            if(TryAuth(loginText, passwordHash))
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", loginText);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", passwordHash);
            }
        }
    }
}
