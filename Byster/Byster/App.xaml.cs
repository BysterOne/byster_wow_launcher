using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using RestSharp;
using System.Diagnostics;
using System.Threading;
using Byster.Models.Utilities;
using Byster.Models.BysterModels;

namespace Byster
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Sessionid;
        private static string apiUrl = Convert.ToBoolean(RegistryEditor.GetEditor("Sandbox").RegistryValue) ? "https://api-test.byster.ru" : "https://api.byster.ru";
        public static RestClient Rest = new RestClient(apiUrl);
        public App()
        {
        }
    }
}
