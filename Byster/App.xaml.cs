using Byster.View;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Byster
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static RestClient Rest = new RestClient("https://api.byster.ru");
        public static string Login { get; set; }
        public static string Password { get; set; }

        public static LoginWindow WindowLogin { get; set; }
        public static MainWindow WindowMain { get; set; }

        [STAThread]
        public static void Main()
        {
            App app = new App();
            WindowLogin = new LoginWindow();
            WindowMain = new MainWindow();
            app.Run(WindowLogin);
        }
    }
}
