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
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string login = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", null);
            string password = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", null);

            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                string passwordHash = password;
                if (TryAuth(login, passwordHash))
                {
                    StartMainWindow();
                    Close();
                    return;
                }
                else
                {
                    Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", "");
                    Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", "");
                }
            }
            var response = App.Rest.Post<List<RegisterChoice>>(new RestRequest("launcher/register_choices"));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<RegisterChoice> registerChoices = response.Data;
                registerChoicesComboBox.ItemsSource = registerChoices;
                registerChoicesComboBox.DisplayMemberPath = "selection";
                registerChoicesComboBox.SelectedIndex = 1;
            }
            else
            {
                MessageBox.Show($"Запрос завершился с кодом: {(int)response.StatusCode}\nСообщение ошибки: {response.ErrorMessage}", "Ошибка HTTP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryAuth(string login, string passwordHash)
        {
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
            var response = App.Rest.Post<AuthResponse>(new RestRequest("launcher/login", Method.POST).AddJsonBody(new AuthRequest()
            {
                login = login,
                password = passwordHash,
            }));

            if(response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.Data.error, "Ошибка Byster", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            App.Rest.Authenticator = new BysterAuthenticator(response.Data.session);
            return true;
        }

        private bool TryRegister(string login, string passwordHash, string referal, int idOfRegisterChoice)
        {
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (string.IsNullOrEmpty(passwordHash))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if(idOfRegisterChoice == 0)
            {
                MessageBox.Show("Укажите, откуда Вы о нас узнали", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            var response = (referal != "") ? App.Rest.Post<RegisterResponse>(new RestRequest("launcher/registration", Method.POST).AddJsonBody(new RegisterRequestReferal()
            {
                login = login,
                password = passwordHash,
                referal = referal,
                register_source = idOfRegisterChoice.ToString(),
            })) : App.Rest.Post<RegisterResponse>(new RestRequest("launcher/registration", Method.POST).AddJsonBody(new RegisterRequestNoReferal()
            {
                login = login,
                password = passwordHash,
                register_source = idOfRegisterChoice.ToString(),
            }));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                if(!string.IsNullOrEmpty(response.Data.error))
                {
                    MessageBox.Show(response.Data.error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                else
                {
                    MessageBox.Show($"Запрос завершён с кодом:{(int)response.StatusCode}\nСообщение ошибки: {response.ErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            App.Rest.Authenticator = new BysterAuthenticator(response.Data.session);
            return true;
        }

        public void StartMainWindow()
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                App.Current.MainWindow = new MainWindow();
                App.Current.MainWindow.Show();
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
            this.Height = 520;
            loginBtn.IsDefault = false;
            registerBtn.IsDefault = true;
        }
        private void navigateToAuth_Click(object sender, RoutedEventArgs e)
        {
            authBlock.Visibility = Visibility.Visible;
            registerBlock.Visibility = Visibility.Collapsed;
            this.Height = 370;
            loginBtn.IsDefault = true;
            registerBtn.IsDefault = false;
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
                StartMainWindow();
                Close();
            }
        }

        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            string newLoginText = newLoginBox.Text;
            string newPasswordText = newPasswordBox.Password;
            string newPasswordConfirmText = newPasswordConfirmBox.Password;
            string referal = referalBox.Text;
            int idOfRegisterChoice = (registerChoicesComboBox.SelectedItem as RegisterChoice).id;

            if(newPasswordText != newPasswordConfirmText)
            {
                MessageBox.Show("Введённые пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string newPasswordHash = HashCalc.GetMD5Hash(newPasswordText);
            if(TryRegister(newLoginText, newPasswordHash, referal, idOfRegisterChoice))
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", newLoginText);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", newPasswordHash);
                StartMainWindow();
                Close();
            }
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password == "") passwordBox.Tag = "Пароль"; else passwordBox.Tag = "";
        }

        private void newPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (newPasswordBox.Password == "") newPasswordBox.Tag = "Пароль"; else newPasswordBox.Tag = "";
        }

        private void newPasswordConfirmBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (newPasswordConfirmBox.Password == "") newPasswordConfirmBox.Tag = "Подтвердите пароль"; else newPasswordConfirmBox.Tag = "";
        }
    }
}
