using Byster.RestModels;
using Microsoft.Win32;
using RestSharp;
using System;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace Byster.View
{
    /// <summary>
    /// Логика взаимодействия для login.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        string password_hash;

        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin.Focus();
                btnLogin_Click(null, null);
            }
        }

        private void txtLogin_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                txtPassword.Focus();

        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLogin.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            password_hash = password_hash ?? Utility.GetMd5Hash(txtPassword.Password);

            var response = App.Rest.Post<AuthorizationResponse>(
                new RestRequest("launcher/login").AddJsonBody(new AuthorizationRequest
                {
                    login = txtLogin.Text,
                    password = password_hash
                })
            );

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", txtLogin.Text);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", password_hash);

                App.Rest.Authenticator = new BysterAuthenticator(response.Data.session);
                
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow main = new MainWindow();
                    main.Owner = Owner;
                    main.Show();
                }));

                Hide();
                return;
            }

            MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtLogin.Text = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", null);
            password_hash = txtPassword.Password = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", null);

            if (txtLogin.Text != null && txtPassword.Password != null)
                btnLogin_Click(null, null);
        }
    }
}
