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

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public string[] TrustedHosts = new string[] {"api.byster.ru",
                                                     "s3.byster.ru"};
        public LoadingWindow()
        {
            InitializeComponent();
            
            string currentDir = Directory.GetCurrentDirectory();

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") {
                FileName = "${specialfolder:folder=ApplicationData:cached=true}/BysterConfig/Byster.log",
                Layout = "[${longdate}] ${message}${exception:format=ToString}",
                KeepFileOpen = true,
                Encoding = Encoding.UTF8,
                CreateDirs = true,
                ArchiveFileName = "${specialfolder:folder=ApplicationData:cached=true}/BysterConfig/BysterLogsArchive/${longdate}-BysterLogs.zip",
                EnableArchiveFileCompression = true,
                ArchiveOldFileOnStartupAboveSize = 5000,
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
            App.Current.DispatcherUnhandledException += (sender, e) =>
            {
                BysterLogger.Log("Fatal:Необработанное исключение", e.Exception.Message, e.Exception.StackTrace);
                e.Handled = true;
            };
            Byster.Models.Utilities.BysterLogger.Log("Версия Windows", Environment.OSVersion.Version.Major);
            if (Environment.OSVersion.Version.Major <= 7)
            {
                
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                       | SecurityProtocolType.Tls11
                                                       | SecurityProtocolType.Tls12
                                                       | SecurityProtocolType.Ssl3;
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
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Log("Open LoadingWindow");

            rotateProgressTransform.CenterX = this.ActualWidth / 2;
            rotateProgressTransform.CenterY = this.ActualHeight / 2;
            rotateProgressTransform.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation()
            {
                To = 360,
                Duration = new Duration(new TimeSpan(0, 0, 2)),
                RepeatBehavior = RepeatBehavior.Forever,
            });

            Log("Запускаем поток");

            Task.Run(() =>
            {
                Log("Запустили поток");
                BackgroundImageDownloader.Init();
                Log("Запустили BackgroundImageDownloader");
                if (File.Exists("BysterUpdate.exe")) File.Delete("BysterUpdate.exe");
                if (File.Exists("update.bat")) File.Delete("update.bat");
                Log("Удаление остаточных файлов");
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                Log("Проверка обновлений");
                var response = App.Rest.Get(new RestRequest("launcher/check_updates"));
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        MessageBox.Show($"Error while connecting server\nResponse HTTP-Code: {(int)response.StatusCode}\nError message: {response.ErrorMessage}\nServer error message: {JsonConvert.DeserializeObject<BaseResponse>(response.Content)?.error ?? "No server answer"}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        closeApp();
                        return;
                    }
                }
                string onlineVersion = Encoding.UTF8.GetString(response.RawBytes);

                if (onlineVersion != version)
                {
                    
                    Log($"Обновление от версии {version} до {onlineVersion}");
                    Thread thread = new Thread(() => { updateApp(onlineVersion); });
                    thread.Start();
                }
                else
                {
                    Log("Обновления не найдены. Запуск приложения...");
                    string login = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", null);
                    string password = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", null);
                    if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                    {
                        string passwordHash = password;
                        string sessionId;
                        int status_code = TryAuth(login, passwordHash, out sessionId);

                        switch (status_code)
                        {
                            case 200:
                                StartMainWindow(login, sessionId);
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
                    startApp();
                }
            });
        }

        public void StartMainWindow(string login, string sessionId)
        {
            Dispatcher.Invoke(() =>
            {
                var mainWindow = new MainWindowReworked(login, sessionId);
                mainWindow.Show();
                Close();
                mainWindow.Initialize();
            });
            //Dispatcher.Invoke(() =>
            //{
            //    App.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            //});
            //Thread newThread = new Thread(new ThreadStart(() =>
            //{
            //    var mainWindow = new MainWindowReworked(login, sessionId);
            //    mainWindow.Show();
            //    System.Windows.Threading.Dispatcher.Run();
            //    mainWindow.Initialize();
            //    App.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            //}));
            //newThread.IsBackground = true;
            //newThread.SetApartmentState(ApartmentState.STA);
            //newThread.Start();
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    Close();
            //    Dispatcher.InvokeShutdown();
            //}));
        }

        private int TryAuth(string login, string passwordHash, out string sessionId)
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
        private void startApp()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                App.Current.MainWindow = new LoginOrRegistrationWindow();
                App.Current.MainWindow.Show();
                while(!App.Current.MainWindow.IsLoaded) { }
                Close();
            });
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void updateApp(string versionToUpdate)
        {
            Log("Получение новой версии");
            RestClient client = new RestClient("https://api.byster.ru/");
            var response = client.Get(new RestRequest("launcher/download"));
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Error while connecting server\nResponse HTTP-Code: {(int)response.StatusCode}\nError message: {response.ErrorMessage}\nServer error message: {JsonConvert.DeserializeObject<BaseResponse>(response.Content)?.error ?? "No Server answer"}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                closeApp();
                return;
            }
            Log("Обновление");
            File.WriteAllBytes("BysterUpdate.exe", response.RawBytes);
            File.WriteAllLines("update.bat", new List<string>(){
                    "taskkill /IM \"Byster.exe\" /F",
                    "timeout /t 2 /NOBREAK",
                    "del /f Byster.exe",
                    "rename BysterUpdate.exe Byster.exe",
                    "Byster.exe",
                });
            Log("Перезапуск...");
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
