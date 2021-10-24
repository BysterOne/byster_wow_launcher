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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;
using Byster.Models.Utilities;
using RestSharp;
using Byster.Models.RestModels;
using Microsoft.Win32;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            string loginText = loginBox.Text;
            string passwordText = passwordBox.Text;

            if(string.IsNullOrEmpty(loginText))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (string.IsNullOrEmpty(passwordText))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string passwordHash = HashCalc.GetMD5Hash(passwordText);

            var response = App.Rest.Post<AuthResponse>(new RestRequest().AddJsonBody(new AuthRequest()
            {
                login = loginText,
                password = passwordHash,
            }));
            
            if(response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(string.IsNullOrEmpty(response.Data.error))
            {
                MessageBox.Show(response.Data.error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", loginText);
            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", passwordHash);

            App.Rest.Authenticator = new BysterAuthenticator(response.Data.session);
        }
    }
}
