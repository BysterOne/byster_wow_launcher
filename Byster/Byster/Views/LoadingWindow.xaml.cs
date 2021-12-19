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

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        private readonly string NLogConfig = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><nlog xmlns=\"http://www.nlog-project.org/schemas/NLog.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><targets><target name=\"logfile\" xsi:type=\"File\" fileName=\"${specialfolder:folder=ApplicationData:cached=true}/BysterConfig/Byster.log\" layout=\"[${longdate} ${logger}] ${message}${exception:format=ToString}\" keepFileOpen=\"true\" encoding=\"utf-8\" createDirs=\"true\"/></targets><rules><logger name=\"*\" minlevel=\"Info\" writeTo=\"logfile\" /></rules></nlog>";

        public LoadingWindow()
        {
            InitializeComponent();
            string currentDir = Directory.GetCurrentDirectory();

            if (!File.Exists("NLog.config"))
            {
                File.Create("NLog.config").Close();
                File.WriteAllText("NLog.config", NLogConfig);
                var processes = Process.GetProcessesByName("Byster.exe");
                Process.Start("Byster.exe");
                foreach (var p in processes)
                {
                    p.Kill();
                }
                Close();
            }
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            rotateProgressTransform.CenterX = this.ActualWidth / 2;
            rotateProgressTransform.CenterY = this.ActualHeight / 2;
            rotateProgressTransform.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation()
            {
                To = 360,
                Duration = new Duration(new TimeSpan(0, 0, 2)),
                RepeatBehavior = RepeatBehavior.Forever,
            });

            Task.Run(() =>
            {
                BackgroundPhotoDownloader.Init();
                if (File.Exists("BysterUpdate.exe")) File.Delete("BysterUpdate.exe");
                if (File.Exists("update.bat")) File.Delete("update.bat");

                string version = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();


                var response = App.Rest.Get(new RestRequest("launcher/check_updates"));
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        MessageBox.Show($"Ошибка при соединении с сервером\nЗапрос завершён с кодом: {(int)response.StatusCode}\nСообщение ошибки: {response.ErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        closeApp();
                        return;
                    }
                }
                string onlineVersion = Encoding.UTF8.GetString(response.RawBytes);

                if (onlineVersion != version)
                {
                    Thread thread = new Thread(() => { updateApp(onlineVersion); });
                    thread.Start();
                }
                else
                {
                    string login = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", null);
                    string password = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", null);
                    if (!string.IsNullOrEmpty(login) && !string.IsNullOrEmpty(password))
                    {
                        string passwordHash = password;
                        string sessionId;
                        if (TryAuth(login, passwordHash, out sessionId))
                        {
                            StartMainWindow(login, sessionId);
                            Close();
                            return;
                        }
                        else
                        {
                            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Login", "");
                            Registry.SetValue("HKEY_CURRENT_USER\\Software\\Byster", "Password", "");
                            startApp();
                        }
                    }
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

        private bool TryAuth(string login, string passwordHash, out string sessionId)
        {
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                sessionId = null;
                return false;
            }
            if (string.IsNullOrEmpty(passwordHash))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                sessionId = null;
                return false;
            }
            var response = App.Rest.Post<AuthResponse>(new RestRequest("launcher/login", Method.POST).AddJsonBody(new AuthRequest()
            {
                login = login,
                password = passwordHash,
            }));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show(response.Data.error, "Ошибка Byster", MessageBoxButton.OK, MessageBoxImage.Error);
                sessionId = null;
                return false;
            }
            App.Rest.Authenticator = new BysterAuthenticator(response.Data.session);
            sessionId = response.Data.session;
            return true;
        }


        private void closeApp()
        {
            foreach(Window window in App.Current.Windows)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    window.Close();
                }));
                App.Current.Shutdown();
            }
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
            RestClient client = new RestClient("https://api.byster.ru/");
            RestRequest downloadRequest = (RestRequest)new RestRequest("launcher/download").AddQueryParameter("version", versionToUpdate);
            var response = client.Get(downloadRequest);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Ошибка при соединении с сервером\nЗапрос завершён с кодом: {(int)response.StatusCode}\nСообщение ошибки: {response.ErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                closeApp();
                return;
            }
            File.WriteAllBytes("BysterUpdate.exe", response.RawBytes);
            File.WriteAllLines("update.bat", new List<string>(){
                    "taskkill /IM \"Byster.exe\" /F",
                    "timeout /t 2 /NOBREAK",
                    "del /f Byster.exe",
                    "rename BysterUpdate.exe Byster.exe",
                    "Byster.exe",
                });
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
