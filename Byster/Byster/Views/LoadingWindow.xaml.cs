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
using System.Reflection;
using System.Net;
using System.IO;
using RestSharp;
using System.Diagnostics;
using System.Threading;
using Byster.Models.Utilities;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Byster.Models.RestModels;
using Newtonsoft.Json;
using NLog;
using static Byster.Models.Utilities.BysterLogger;
using Byster.Localizations.Tools;
using System.Web.Configuration;

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public string[] TrustedHosts = new string[] {"api.byster.one",
                                                     "r2.byster.one",
                                                     "byster.one"};
        public LoadingWindow()
        {
            InitializeComponent();

            string currentDir = Directory.GetCurrentDirectory();

            // Запуск логирования
            BysterLogger.Init();

            //Обработчик исключений диспетчера
            App.Current.DispatcherUnhandledException += (sender, e) =>
            {
                LogFatal("Common Dispather", "Необработанное исключение", e.Exception.Message, e.Exception.StackTrace, e.Exception.InnerException?.StackTrace ?? "[]");
                e.Handled = true;
            };
            LogInfo("Common", "Килл процессов");
            ProcessKiller.KillProcesses();
            LogInfo("Common", "Подготовка к запуску");
            LogInfo("Common", "Версия Windows", Environment.OSVersion.Version.Major);
            if (Environment.OSVersion.Version.Major <= 7)
            {
                LogInfo("Common", "Применение настроек для версий Windows <7");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                LogInfo("Сеть", "Разрешены сертификаты типов: TLS1.2");
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) =>
                {
                    if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                    {
                        return true;
                    }
                    var request = sender as HttpWebRequest;
                    if (request != null)
                    {
                        return TrustedHosts.Contains(request.RequestUri.Host);
                    }
                    return false;
                };
                LogInfo("Сеть", "Отключена проверка сертификатов для хостов: api.byster.one, s3.byster.one");
            }
            LogInfo("Common", "Подготовка завершена, запуск...");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogInfo("Common", "Запуск окна LoadingWindow");
            rotateProgressTransform.CenterX = this.ActualWidth / 2;
            rotateProgressTransform.CenterY = this.ActualHeight / 2;
            rotateProgressTransform.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation()
            {
                To = 360,
                Duration = new Duration(new TimeSpan(0, 0, 2)),
                RepeatBehavior = RepeatBehavior.Forever,
            });

            LogInfo("Common", "Запуск потока");

            Task.Run(() =>
            {
                LogInfo("Common", "Запущен поток");
                LogInfo("Common", "Запуск BackgroundImageDownloader");
                BackgroundImageDownloader.Init();
                LogInfo("Common", "Запущен BackgroundImageDownloader");
                LogInfo("Common", "Удаление остаточных файлов");
                if (File.Exists("BysterUpdate.exe")) try { File.Delete("BysterUpdate.exe"); } catch { LogWarn("Common", "Ошибка удаления BysterUpdate.exe"); }
                if (File.Exists("update.bat")) try { File.Delete("update.bat"); } catch { LogWarn("Common", "Ошибка удаления update.bat"); }
                if (File.Exists("changeLocalization.bat")) try { File.Delete("changeLocalization.bat"); } catch { LogWarn("Common", "Ошибка удаления changeLocalization.bat"); }
                string qrCodesPath = Path.Combine(Path.GetTempPath(), "BysterQRCodes");
                if(Directory.Exists(qrCodesPath))
                {
                    foreach (var file in Directory.GetFiles(qrCodesPath))
                    {
                        try
                        {
                            File.Delete(Path.Combine(qrCodesPath, file));
                        }
                        catch
                        {
                            LogWarn("Common", "Ошибка удаления QR Code", "Path: ", file);
                        }
                    }
                }
                LogInfo("Common", "Удаление остаточных файлов завершено");
                LogInfo("Common", "Проверка обновлений");
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var response = App.Rest.Get(new RestRequest("launcher/check_updates"));
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        LogError("Common", "Ошибка соединения с сервером", "HTTP-Code: " + response.StatusCode.ToString());
                        MessageBox.Show($"Error while connecting server\nResponse HTTP-Code: {(int)response.StatusCode}\nError message: {response.ErrorMessage}\nServer error message: {JsonConvert.DeserializeObject<BaseResponse>(response.Content)?.error ?? "No server answer"}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        closeApp();
                        return;
                    }
                }
                string onlineVersion = Encoding.UTF8.GetString(response.RawBytes);
                if (onlineVersion != version)
                {

                    LogInfo("Common", $"Обновление от версии {version} до {onlineVersion}");
                    Thread thread = new Thread(() => { updateApp(onlineVersion); });
                    thread.Start();
                }
                else
                {
                    LogInfo("Common", "Обновления не найдены. Запуск основного приложения");
                    string login = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", null);
                    string password = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", null);
                    if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                    {
                        string passwordHash = password;
                        string sessionId;
                        LogInfo("Authorization", "Попытка авторизации");
                        int status_code = tryAuth(login, passwordHash, out sessionId);
                        switch (status_code)
                        {
                            case 200:
                                ShowMainWindow(login, sessionId);
                                return;

                            case 401:
                            case 404:
                                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", "");
                                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", "");
                                break;
                        }

                        Dispatcher.Invoke(() => Close());
                        return;
                    }
                    ShowLoginWindow();
                }
            });
        }

        public void ShowMainWindow(string login, string sessionId)
        {
            Dispatcher.Invoke(() =>
            {
                var mainWindow = new MainWindowReworked(login, sessionId);
                mainWindow.Show();
                Close();
                mainWindow.Initialize();
            });
        }

        private int tryAuth(string login, string passwordHash, out string sessionId)
        {
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show(Localizator.GetLocalizationResourceByKey("EnterLogin"), Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Information);
                sessionId = null;
                return 404;
            }

            if (string.IsNullOrEmpty(passwordHash))
            {
                MessageBox.Show(Localizator.GetLocalizationResourceByKey("EnterPassword"), Localizator.GetLocalizationResourceByKey("Error"), MessageBoxButton.OK, MessageBoxImage.Information);
                sessionId = null;
                return 404;
            }

            var response = App.Rest.Post<AuthResponse>(new RestRequest("launcher/login", Method.POST).AddJsonBody(new AuthRequest()
            {
                login = login,
                password = passwordHash,
            }));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.Data.error, Localizator.GetLocalizationResourceByKey("ErrorByster"), MessageBoxButton.OK, MessageBoxImage.Error);
                sessionId = null;
                return (int)response.StatusCode;
            }

            App.Rest.Authenticator = new BysterAuthenticator(response.Data.session);
            sessionId = response.Data.session;

            return 200;
        }


        private void closeApp()
        {
            Dispatcher.Invoke(() =>
            {
                foreach (Window window in App.Current.Windows)
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        window.Close();
                    }));
                    App.Current.Shutdown();
                }
            });
        }
        private void ShowLoginWindow()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    App.Current.MainWindow = new LoginOrRegistrationWindow();
                    App.Current.MainWindow.Show();
                    while (!App.Current.MainWindow.IsLoaded) { }
                }
                finally
                {
                    Close();
                }
            });
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void updateApp(string versionToUpdate)
        {
            LogInfo("Common", "Получение новой версии");
            var response = App.Rest.Get(new RestRequest("launcher/download"));
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Error while connecting server\nResponse HTTP-Code: {(int)response.StatusCode}\nError message: {response.ErrorMessage}\nServer error message: {JsonConvert.DeserializeObject<BaseResponse>(response.Content)?.error ?? "No Server answer"}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                closeApp();
                return;
            }
            LogInfo("Common", "Обновление");
            File.WriteAllBytes("BysterUpdate.exe", response.RawBytes);
            File.WriteAllLines("update.bat", new List<string>(){
                    $"taskkill /IM \"{ Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location)}.exe\" /F",
                    "timeout /t 2 /NOBREAK",
                    $"del /f { Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location)}.exe",
                    $"rename BysterUpdate.exe { Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location)}.exe",
                    $"{ Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location)}.exe",
                });
            LogInfo("Common", "Перезапуск...");
            Process process = new Process();
            process.StartInfo.FileName = "update.bat";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            closeApp();
            return;
        }
    }
}
