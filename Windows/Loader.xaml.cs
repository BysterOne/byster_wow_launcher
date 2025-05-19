using Cls;
using Cls.Any;
using Cls.Enums;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Api;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Settings;
using Launcher.Windows.AnyLoader.Errors;
using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Launcher.Windows
{
    /// <summary>
    /// Логика взаимодействия для Loader.xaml
    /// </summary>
    public partial class Loader : Window
    {
        public Loader()
        {
            InitializeComponent();
            loader.StartAnimation();
            this.Loaded += Loader_Loaded;
        }

        #region Переменные
        private static LogBox Pref { get; set; } = new LogBox("Loader");
        #endregion

        #region Обработчики событий
        #region Loader_Loaded
        private void Loader_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(Init);
        }
        #endregion
        #endregion
                

        #region Функции
        #region CheckReferalSource
        private async Task CheckReferalSource()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var filename = Path.GetFileName(Process.GetCurrentProcess().MainModule!.FileName);
            var tryGetRefSource = await CApi.GetReferralSource(filename);
            if (!tryGetRefSource.IsSuccess) 
            {
                var uerror = 
                    new UError
                    (
                        ECheckReferalSource.FailApiExecuteRequest,
                        $"Реферальный код и/или источник не обнаружен",
                        tryGetRefSource.Error
                    );
                Functions.Error(uerror, uerror.Message, _proc);
                return;
            }

            var response = tryGetRefSource.Response;
            if 
            (
                !String.IsNullOrWhiteSpace(response.ReferralCode) && 
                response.RegisterSource != -1
            )
            {
                GProp.ReferralSource = tryGetRefSource.Response;
                _proc.Log($"RefCode: {response.ReferralCode}, RefSource: {response.ReferralCode}");
            }
        }
        #endregion
        #region Init
        private async Task Init()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            
            #region Настройки логов
            ConfigureNLog();
            #endregion
            #region Загрузка словаря
            //var tryLoadTranslations = await Dictionary.Load();
            //if (!tryLoadTranslations.IsSuccess)
            //{
            //    this.Hide();
            //    MessageBox.Show("Ошибка инициализации. Приложение будет закрыто.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //    Application.Current.Shutdown();
            //}
            #endregion           

            #region Авторизация, если данные сохранены
            if 
            (
                !String.IsNullOrWhiteSpace(AppSettings.Instance.Login) &&
                !String.IsNullOrWhiteSpace(AppSettings.Instance.Password)
            )
            {
                #region Запрос
                var tryLogin = await CApi.Login
                (
                    new LoginRequestBody
                    {
                        Login = AppSettings.Instance.Login,
                        Password = AppSettings.Instance.Password
                    }
                );
                if (tryLogin.IsSuccess) 
                {
                    #region Сохраняем данные
                    CApi.Session = tryLogin.Response.Session;
                    #endregion
                    #region Главное окно
                    OpenMainWindow();
                    #endregion
                    return; 
                }
                #endregion
            }
            #endregion
            #region В любом другом случае
            await CheckReferalSource(); 
            OpenAuthorization();
            #endregion
        }
        #endregion        
        #region OpenMainWindow
        private void OpenMainWindow() => Application.Current.Dispatcher.Invoke(() => Functions.OpenWindow(this, new Main()));        
        #endregion
        #region OpenAuthorization
        private void OpenAuthorization() => Application.Current.Dispatcher.Invoke(() => Functions.OpenWindow(this, new Authorization()));
        #endregion
        #region ConfigureNLog
        private void ConfigureNLog()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Launcher.NLog.config";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is not null)
            {
                using StreamReader reader = new StreamReader(stream);
                var configXml = reader.ReadToEnd();

                using StringReader stringReader = new StringReader(configXml);
                using XmlReader xmlReader = XmlReader.Create(stringReader);

                var config = new XmlLoggingConfiguration(xmlReader, null);
                LogManager.Configuration = config;

                var logger = LogManager.GetCurrentClassLogger();

                Log.NewMessage += (message, type) =>
                {
                    switch (type)
                    {
                        case ELogType.Warning:
                            logger.Warn(message);
                            break;
                        case ELogType.Error:
                            logger.Error(message);
                            break;
                        default: logger.Info(message); break;
                    }
                };
            }
        }
        #endregion
        #endregion
    }
}
