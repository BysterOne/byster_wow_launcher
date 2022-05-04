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
using RestSharp;
using Byster.Models.RestModels;
using Byster.Models.Utilities;
using Byster.Localizations.Tools;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginOrRegistrationWindow.xaml
    /// </summary>
    public partial class LoginOrRegistrationWindow : Window
    {

        private bool useAutoReferal = false;
        private string autoReferal = "";
        private int autoRegisterSource = 0;
        public LoginOrRegistrationWindow()
        {
            InitializeComponent();
            registerBtn.Content = Localizator.GetLocalizationResourceByKey("Register").Value;
            loginBtn.Content = Localizator.GetLocalizationResourceByKey("Login").Value;
            loginBox.Tag = Localizator.GetLocalizationResourceByKey("Username").Value;
            passwordBox.Tag = Localizator.GetLocalizationResourceByKey("Password").Value;
            newLoginBox.Tag = Localizator.GetLocalizationResourceByKey("NewLogin").Value;
            newPasswordBox.Tag = Localizator.GetLocalizationResourceByKey("NewPassword").Value;
            newPasswordConfirmBox.Tag = Localizator.GetLocalizationResourceByKey("NewPasswordConfirmation").Value;
            referalBox.Tag = Localizator.GetLocalizationResourceByKey("ReferalCode").Value;
            swapLanguageTextBlock.Text = Localizator.GetLocalizationResourceByKey("SwapLanguage").Value;
            navigateToAuthTextBlock.Text = Localizator.GetLocalizationResourceByKey("AuthorizationQ").Value;
            swapLanguageTextBlockReg.Text = Localizator.GetLocalizationResourceByKey("SwapLanguage").Value;
            navigateToRegisterTextBlock.Text = Localizator.GetLocalizationResourceByKey("RegistrationQ").Value;
            tooltipReferal.ToolTip = Localizator.GetLocalizationResourceByKey("ReferalTip").Value;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            authBlock.Visibility = Visibility.Collapsed;
            setHeight();

            string login = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", null);
            string password = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", null);

            if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
            {
                string passwordHash = password;
                string sessionId;
                if (tryAuth(login, passwordHash, out sessionId))
                {
                    StartMainWindow(login, sessionId);
                    return;
                }
                else
                {
                    Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", "");
                    Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", "");
                }
            }
            autoAuthPresenterGrid.Visibility = Visibility.Collapsed;

            if(checkFileNameServer(out autoReferal, out autoRegisterSource))
            {
                referalBox.Visibility = Visibility.Collapsed;
                tooltipReferal.Visibility = Visibility.Collapsed;
                setHeight();
                useAutoReferal = true;
            }
            else
            {
                if (!checkFileNameLocal())
                {
                    MessageBox.Show(Localizator.GetLocalizationResourceByKey("FileCheckErrorMessage"), Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
        }

        private void setHeight()
        {
            double h = 0;
            if(registerBlock.IsVisible)
            {
                if(referalBox.IsVisible)
                {
                    h = 465.0;
                }
                else
                {
                    h = 415.0;
                }
            }
            else if(authBlock.IsVisible)
            {
                h = 360.0;
            }
            else
            {
                h = 500.0;
            }
            this.Height = h;
        }

        private bool tryAuth(string login, string passwordHash, out string sessionId)
        {
            if(string.IsNullOrEmpty(login))
            {
                MessageBox.Show(Localizator.GetLocalizationResourceByKey("EnterLogin"), Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Information);
                sessionId = null;
                return false;
            }
            if(string.IsNullOrEmpty(passwordHash))
            {
                MessageBox.Show(Localizator.GetLocalizationResourceByKey("EnterPassword"), Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Information);
                sessionId = null;
                return false;
            }
            var response = App.Rest.Post<AuthResponse>(new RestRequest("launcher/login", Method.POST).AddJsonBody(new AuthRequest()
            {
                login = login,
                password = passwordHash,
            }));

            if(response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.Data.error, Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                sessionId = null;
                return false;
            }
            App.Rest.Authenticator = new BysterAuthenticator(response.Data.session);
            sessionId = response.Data.session;
            return true;
        }

        private bool tryRegister(string login, string passwordHash, string referal, int? idOfRegisterChoice, out string sessionId)
        {
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show(Localizator.GetLocalizationResourceByKey("EnterLogin"), Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Information);
                sessionId = null;
                return false;
            }
            if (string.IsNullOrEmpty(passwordHash))
            {
                MessageBox.Show(Localizator.GetLocalizationResourceByKey("EnterPassword"), Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Information);
                sessionId = null;
                return false;
            }

            var response = string.IsNullOrEmpty(referal) ? (idOfRegisterChoice == null) ? App.Rest.Post<RegisterResponse>(new RestRequest("launcher/registration", Method.POST).AddJsonBody(new RegisterRequestNoReferalNoRegisterSource()
            {
                login = login,
                password = passwordHash,
            })) : App.Rest.Post<RegisterResponse>(new RestRequest("launcher/registration", Method.POST).AddJsonBody(new RegisterRequestNoReferal()
            {
                login = login,
                password = passwordHash,
                register_source = idOfRegisterChoice.ToString(),
            })) : (idOfRegisterChoice == null) ? App.Rest.Post<RegisterResponse>(new RestRequest("launcher/registration", Method.POST).AddJsonBody(new RegisterRequestNoRegisterSource()
            {
                login = login,
                password = passwordHash,
                referal = referal,
            })) : App.Rest.Post<RegisterResponse>(new RestRequest("launcher/registration", Method.POST).AddJsonBody(new RegisterRequestReferal()
            {
                login = login,
                password = passwordHash,
                register_source = idOfRegisterChoice.ToString(),
                referal = referal,
            }));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                if(!string.IsNullOrEmpty(response.Data.error))
                {
                    MessageBox.Show(response.Data.error, Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    sessionId = null;
                    return false;
                }
                else
                {
                    MessageBox.Show($"Запрос завершён с кодом: {(int)response.StatusCode}\nСообщение ошибки: {response.ErrorMessage}\nСообщение ошибки от сервера: {response.Data.error ?? "-"}", Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                    sessionId = null;
                    return false;
                }
            }

            App.Rest.Authenticator = new BysterAuthenticator(response.Data.session);
            sessionId = response.Data.session;
            return true;
        }

        public void StartMainWindow(string login, string sessionId)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                App.Current.MainWindow = new MainWindowReworked(login, sessionId);
                App.Current.MainWindow.Show();
                Close();
                (App.Current.MainWindow as MainWindowReworked).Initialize();
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
            setHeight();
            loginBtn.IsDefault = false;
            registerBtn.IsDefault = true;
        }
        private void navigateToAuth_Click(object sender, RoutedEventArgs e)
        {
            authBlock.Visibility = Visibility.Visible;
            registerBlock.Visibility = Visibility.Collapsed;
            setHeight();
            loginBtn.IsDefault = true;
            registerBtn.IsDefault = false;
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            string loginText = loginBox.Text;
            string passwordText = passwordBox.Password;
            string passwordHash = HashCalc.GetMD5Hash(passwordText);
            string sessionId;
            if(tryAuth(loginText, passwordHash, out sessionId))
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", loginText);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", passwordHash);
                StartMainWindow(loginText, sessionId);
                Close();
            }
        }

        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            string newLoginText = newLoginBox.Text;
            string newPasswordText = newPasswordBox.Password;
            string newPasswordConfirmText = newPasswordConfirmBox.Password;
            string referal = "";
            int? idOfRegisterChoice = null;
            if (useAutoReferal)
            {
                referal = autoReferal;
                idOfRegisterChoice = autoRegisterSource;
            }
            else
            {
                idOfRegisterChoice = null;
                referal = referalBox.Text;
            }

            if(newPasswordText != newPasswordConfirmText)
            {
                MessageBox.Show(Localizator.GetLocalizationResourceByKey("ChangePasswordErrorPwdNotMatched"), Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(string.IsNullOrEmpty(newPasswordText))
            {
                MessageBox.Show(Localizator.GetLocalizationResourceByKey("EnterPassword"), Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string newPasswordHash = HashCalc.GetMD5Hash(newPasswordText);
            string sessionId;
            if(tryRegister(newLoginText, newPasswordHash, referal, idOfRegisterChoice, out sessionId))
            {
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", newLoginText);
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", newPasswordHash);
                StartMainWindow(newLoginText, sessionId);
                Close();
            }
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password == "") passwordBox.Tag = Localizator.GetLocalizationResourceByKey("Password").Value; else passwordBox.Tag = "";
        }

        private void newPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (newPasswordBox.Password == "") newPasswordBox.Tag = Localizator.GetLocalizationResourceByKey("NewPassword").Value; else newPasswordBox.Tag = "";
        }

        private void newPasswordConfirmBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (newPasswordConfirmBox.Password == "") newPasswordConfirmBox.Tag = Localizator.GetLocalizationResourceByKey("NewPasswordConfirmation").Value; else newPasswordConfirmBox.Tag = "";
        }

        private void referalBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!registerBlock.IsVisible) return;
            setHeight();
        }

        private void swapLanguage_Click(object sender, RoutedEventArgs e)
        {
            Localizator.ReloadLocalization(Localizator.LoadedLocalizationInfo.LanguageCode == "ruRU" ? "enUS" : "ruRU");
        }

        private bool checkFileNameLocal()
        {
            string filename = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            if (filename.ToLower().Contains("byster")) return true;
            return false;
        }

        private bool checkFileNameServer(out string calculatedReferal, out int calculatedRegisterSource)
        {
            string filename = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
            var response = App.Rest.Post<FilenameCheckResponse>(new RestRequest("launcher/get_referal_source").AddJsonBody(new FilenameCheckRequest()
            {
                filename = filename,
            }));
            HttpStatusCode[] restrictedCodes = new HttpStatusCode[]
            {
                HttpStatusCode.BadRequest,
                HttpStatusCode.Forbidden,
                HttpStatusCode.BadGateway,
                HttpStatusCode.GatewayTimeout,
                (HttpStatusCode)0,
            };
            if(restrictedCodes.Contains(response.StatusCode))
            {
                calculatedReferal = "";
                calculatedRegisterSource = 0;
                return false;
            }
            if(!string.IsNullOrEmpty(response.Data.error))
            {
                calculatedReferal = "";
                calculatedRegisterSource = -1;
                return false;
            }
            else
            {
                calculatedRegisterSource = response.Data.register_source;
                calculatedReferal = response.Data.referal;
                return true;
            }
        }        
    }
}
