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

namespace Byster.Views
{
    /// <summary>
    /// Логика взаимодействия для LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    { 
        public LoadingWindow()
        {
            InitializeComponent();
            BackgroundPhotoDownloader.Init();
            statusUpdate.Minimum = 0;
            statusUpdate.Maximum = 100;
            statusUpdate.Value = 0;
            if (File.Exists("BysterUpdate.exe")) File.Delete("BysterUpdate.exe");
            if (File.Exists("update.bat")) File.Delete("update.bat");
            incrementStatus();

            string version = "v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            incrementStatus();


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
            incrementStatus();

            if (onlineVersion != version)
            {
                Thread thread = new Thread(() => { updateApp(onlineVersion); });
                thread.Start();
            }
            else
            {
                incrementStatus();
                incrementStatus();
                startApp();
                return;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void incrementStatus()
        {
            statusUpdate.Value += 20;
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
            App.Current.MainWindow = new LoginOrRegistrationWindow();
            App.Current.MainWindow.Show();
            while(!App.Current.MainWindow.IsLoaded) { }
            Close();
            return;
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
            this.Dispatcher.Invoke(new Action(() =>
            {
                incrementStatus();
            }));
            File.WriteAllLines("update.bat", new List<string>(){
                    "taskkill /IM \"Byster.exe\" /F",
                    "timeout /t 2 /NOBREAK",
                    "del /f Byster.exe",
                    "rename BysterUpdate.exe Byster.exe",
                    "Byster.exe",
                });
            this.Dispatcher.Invoke(new Action(() =>
            {
                incrementStatus();
            }));
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
