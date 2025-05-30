using Cls;
using Cls.Any;
using Cls.Enums;
using Cls.Exceptions;
using Launcher.Any;
using Launcher.Api;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Settings;
using Launcher.Settings.Enums;
using Launcher.Windows.AnyLoader.Errors;
using Launcher.Windows.LoaderAny;
using Microsoft.Win32;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace Launcher.Windows
{
    namespace LoaderAny
    {
        public enum ELoader
        {
            FailCopyRegToFile,
            FailCopyConfigFolderAndClear,
            FailCheckLauncherUpdates,
            FailCheckReferalSource,
        }

        public enum ECheckLauncherUpdates
        {
            FailGetServerVersion,
            CurrentVersionIsEmpty,
            FailGetLauncher,
            LauncherUpdateRequired
        }

        public enum EInit
        {
            FailLoadDictionary,
            FailCheckLauncherUpdates,
            FailSyncConfigs,
        }
    }
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
        private ISpan? InitSpan { get; set; }
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
        private async Task CheckReferralSource()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось определить источник";

            #region try
            try
            {
                var checkRefSource = InitSpan?.StartChild("check-referral");

                var filename = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule!.FileName);
                var tryGetRefSource = await CApi.GetReferralSource(filename);
                if (!tryGetRefSource.IsSuccess)
                {
                   throw new UExcept(ECheckReferalSource.FailApiExecuteRequest,$"Реферальный код и/или источник не обнаружен", tryGetRefSource.Error);
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

                checkRefSource?.Finish();
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(ELoader.FailCheckReferalSource, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);
            }
            #endregion
        }
        #endregion
        #region Init
        private async Task Init()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось инициализировать лаунчер";

            #region try
            try
            {
                #region Sentry
                InitSpan = SentryExtensions.FirstLoadTransaction?.StartChild("preloader", "initialization");
                SentrySdk.ConfigureScope(scope => scope.Span = InitSpan);
                #endregion
                #region Для дебага ставим staging
                #if DEBUG
                AppSettings.Instance.Server = EServer.Staging;
                #endif
                #endregion
                #region Настройки логов
                ConfigureNLog();
                #endregion
                #region Установка username если есть
                if (!String.IsNullOrEmpty(AppSettings.Instance.Login))
                {
                    SentrySdk.ConfigureScope(scope => scope.User = new SentryUser() { Username = AppSettings.Instance.Login });
                }
                #endregion
                #region Проверка версии
                var checkVersionSpan = InitSpan?.StartChild("check-updates");
                var tryCheckVersion = await CheckLauncherUpdates();
                if (!tryCheckVersion.IsSuccess)
                {
                    if (tryCheckVersion.Error.Code is ECheckLauncherUpdates.LauncherUpdateRequired) return;                    
                    throw new UExcept(EInit.FailCheckLauncherUpdates, $"Не удалось проверить обновления для лаунчера", tryCheckVersion.Error);
                }
                checkVersionSpan?.Finish();
                #endregion
                #region Загрузка словаря
                var loadTranslSpan = InitSpan?.StartChild("load-local-dictionaries");
                var tryLoadTranslations = await Dictionary.LoadLocal();
                if (!tryLoadTranslations.IsSuccess)
                {
                    throw new UExcept(EInit.FailLoadDictionary, $"Не удалось загрузить словари", tryLoadTranslations.Error);
                }
                loadTranslSpan?.Finish();
                #endregion
                #region Синхронизация настроек
                var syncConfigSpan = InitSpan?.StartChild("syncing-configs");
                var trySyncSettings = await SyncConfigs();
                if (!trySyncSettings.IsSuccess)
                {
                    var uex = new UExcept(EInit.FailSyncConfigs, $"Не удалось синхронизировать конфиги", trySyncSettings.Error);
                    Functions.Error(uex, uex.Message, _proc);                    
                }else
                {
                    syncConfigSpan?.Finish();
                }
                #endregion
                #region Задачи                
                await Task.WhenAll(CopyConfigFolderAndClearAppData());
                #endregion
                #region Авторизация, если данные сохранены
                if
                (
                    !String.IsNullOrWhiteSpace(AppSettings.Instance.Login) &&
                    !String.IsNullOrWhiteSpace(AppSettings.Instance.Password)
                )
                {
                    var loginLocalDataSpan = InitSpan?.StartChild("login-with-local-data");
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
                        loginLocalDataSpan?.Finish();
                        #endregion
                        #region Главное окно
                        InitSpan?.Finish();
                        InitSpan = null;
                        SentryExtensions.MainWindowLoadingTransaction = SentryExtensions.FirstLoadTransaction?.StartChild("main-window", "launching");
                        OpenMainWindow();
                        #endregion
                        return;
                    }
                    #endregion
                }
                #endregion
                #region В любом другом случае                
                await CheckReferralSource();
                InitSpan?.Finish();
                InitSpan = null;
                SentryExtensions.AuthorizationWindowLoadingTransaction = SentrySdk.StartTransaction("authorization-window", "launching");
                OpenAuthorization();
                #endregion
                
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, ex.Message, _proc);
                CriticalError();
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uex, $"{_failinf}: исключение", _proc);
                CriticalError();
            }
            #endregion
        }
        #endregion
        #region CriticalError
        private void CriticalError()
        {
            Dispatcher.Invoke(() =>
            {
                this.Hide();
                MessageBox.Show(Dictionary.Translate("Ошибка инициализации. Приложение будет закрыто"), Dictionary.Translate("Ошибка"), MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            });            
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
        #region SyncConfigs
        private async Task<UResponse> SyncConfigs()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось синхронизировать настройки";

            #region try
            try
            {
                #region Смотрим реестр
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Byster"))
                {
                    if (key is not null)
                    {
                        #region Сохраняем
                        foreach (var valueName in key.GetValueNames())
                        {
                            switch (valueName.ToLower())
                            {
                                case "login": AppSettings.Instance.Login = key.GetValue(valueName)!.ToString()!; break;
                                case "password": AppSettings.Instance.Password = key.GetValue(valueName)!.ToString()!; break;
                            }
                        }
                        AppSettings.Save();
                        #endregion                        
                        #region Удаляем раздел
                        using (RegistryKey? parent = Registry.CurrentUser.OpenSubKey("Software", writable: true))
                        {
                            if (parent is not null && parent.OpenSubKey("Byster") is not null)
                            {
                                parent.DeleteSubKeyTree("Byster");
                            }
                        }
                        #endregion

                        return new() { IsSuccess = true };
                    }
                }
                #endregion
                #region await
                await Task.Run(() => { return; });
                #endregion

                return new() { IsSuccess = true };
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                return new(ex);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);               
                return new(uerror);
            }
            #endregion
        }
        #endregion
        #region CopyConfigFolderAndClearAppData
        private async Task CopyConfigFolderAndClearAppData()
        {
            await Task.Run(() => { return; });

            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось скопировать папку конфига и очистить другие папки";

            #region try
            try
            {
                #region Sentry
                var copyConfigSpan = InitSpan?.StartChild("copying-configs");
                #endregion

                var configFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BysterConfig");
                var configsPath = Path.Combine(configFolder, "configs");
                var newConfigPath = Path.Combine(AppSettings.RootFolder, "config", "configs");
                var mediaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BysterImages");

                #region Копируем и удаляем конфиг папку
                if (Directory.Exists(configsPath))
                {
                    Functions.CopyDirectory(configsPath, newConfigPath);
                    var deletingSpan = copyConfigSpan?.StartChild("deleting-config-folder");
                    Directory.Delete(configFolder, true);
                    deletingSpan?.Finish();
                }
                copyConfigSpan?.Finish();
                #endregion
                #region Удаляем папку медиа                
                if (Directory.Exists(mediaPath))
                {
                    Directory.Delete(mediaPath, true);
                }
                #endregion                
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(ELoader.FailCopyConfigFolderAndClear, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);
            }
            #endregion
        }
        #endregion
        #region CheckLauncherUpdates
        private async Task<UResponse> CheckLauncherUpdates()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось проверить и/или обновить лаунчер";

            #region try
            try
            {
                #region Текущая версия
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (currentVersion is null)
                {
                    throw new UExcept(ECheckLauncherUpdates.CurrentVersionIsEmpty, $"Пустое значение текущей версии");
                }
                Pref.Log($"Version: {currentVersion}");
                #endregion
                #region Версия сервера
                var tryGetVersion = await CApi.GetServerVersion();
                if (!tryGetVersion.IsSuccess)
                {
                    throw new UExcept(ECheckLauncherUpdates.FailGetServerVersion, $"Не удалось получить актуальную версию", tryGetVersion.Error);
                }
                Pref.Log($"Server version: {tryGetVersion.Response.Version}");
                #endregion
                #region Сравнение и обновление
                var isDebug = false;
                #if DEBUG
                isDebug = true; // Отключаем обновление в режиме отладки
                #endif
                if (tryGetVersion.Response.Version != currentVersion.ToString() && !isDebug)
                {
                    #region Скачивание
                    _proc.Log($"Обновление...");
                    var getNewLauncher = await CApi.GetLauncher();
                    if (!getNewLauncher.IsSuccess)
                    {
                        throw new UExcept(ECheckLauncherUpdates.FailGetLauncher, $"Не удалось загрузить новый лаунчер", getNewLauncher.Error);
                    }
                    #endregion
                    #region Сохранение
                    var currentFolder = AppContext.BaseDirectory;
                    Directory.CreateDirectory(currentFolder);
                    var name = $"BysterUpdates.exe";
                    var pathToUpdater = Path.Combine(currentFolder, name);
                    await File.WriteAllBytesAsync(pathToUpdater, getNewLauncher.Response);
                    #endregion
                    #region Путь к данному экземпляру
                    string filePath = Process.GetCurrentProcess().MainModule!.FileName;
                    string thisExePath = Path.ChangeExtension(filePath, ".exe");
                    #endregion
                    #region Запуск
                    Thread update_thread = new Thread(() =>
                    {
                        string dir = Path.GetDirectoryName(thisExePath);
                        string exeName = Path.GetFileName(thisExePath);
                        string updaterName = Path.GetFileName(pathToUpdater);

                        ProcessStartInfo info = new ProcessStartInfo();
                        info.Arguments =
                            $"/C choice /C Y /N /D Y /T 1 & " +
                            $"cd /d \"{dir}\" & " +
                            $"del \"{exeName}\" & " +
                            $"rename \"{updaterName}\" \"{exeName}\" & " +
                            $"\"{exeName}\"";

                        info.WindowStyle = ProcessWindowStyle.Hidden;
                        info.CreateNoWindow = true;
                        info.FileName = "cmd.exe";
                        info.Verb = "runas";
                        Process.Start(info);
                    });
                    update_thread.Start();
                    #endregion
                    #region Выход
                    Dispatcher.Invoke(() => Application.Current.Shutdown());
                    #endregion
                    #region Ошибка для обновления
                    throw new UExcept(ECheckLauncherUpdates.LauncherUpdateRequired, $"Обновление лаунчера");
                    #endregion
                }
                #endregion

                return new() { IsSuccess = true };
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                return new(ex);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                return new(new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex));
            }
            #endregion
        }
        #endregion
        #endregion
    }
}
