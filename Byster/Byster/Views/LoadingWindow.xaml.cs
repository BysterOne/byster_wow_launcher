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
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            statusUpdate.Minimum = 0;
            statusUpdate.Maximum = 5;
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
                var downloadResponse = App.Rest.Get(new RestRequest().AddQueryParameter("version", onlineVersion));
                if (downloadResponse.StatusCode != HttpStatusCode.OK)
                {
                    MessageBox.Show($"Ошибка при соединении с сервером\nЗапрос завершён с кодом: {(int)response.StatusCode}\nСообщение ошибки: {response.ErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    closeApp();
                    return;
                }
                File.WriteAllBytes("BysterUpdate.exe", downloadResponse.RawBytes);
                incrementStatus();


                File.WriteAllLines("update.bat", new List<string>(){
                    "taskkill /IM \"Byster.exe\" /F",
                    "timeout /t 2 /NOBREAK",
                    "del /f Byster.exe",
                    "rename BysterUpdate.exe Byster.exe",
                    "Byster.exe",
                });
                incrementStatus();

                Process process = new Process();
                process.StartInfo.FileName = "update.bat";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start();
                closeApp();
                return;
            }
            else
            {
                incrementStatus();
                incrementStatus();
                Close();
                return;
            }
        }

        private void incrementStatus()
        {
            statusUpdate.Value += 1;
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
    }
}
