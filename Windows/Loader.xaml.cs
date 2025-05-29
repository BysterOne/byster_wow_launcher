using Cls;
using Cls.Any;
using Cls.Enums;
using Cls.Exceptions;
using Launcher.Api;
using Launcher.Api.Models;
using Launcher.Cls;
using Launcher.Settings;
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
            var _failinf = $"Не удалось определить источник";

            #region try
            try
            {
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
                var tryCheckVersion = await CheckLauncherUpdates();
                if (!tryCheckVersion.IsSuccess)
                {
                    if (tryCheckVersion.Error.Code is ECheckLauncherUpdates.LauncherUpdateRequired) return;
                    throw new UExcept(EInit.FailCheckLauncherUpdates, $"Не удалось проверить обновления для лаунчера", tryCheckVersion.Error);
                }
                #endregion
                #region Загрузка словаря
                var tryLoadTranslations = await Dictionary.LoadLocal();
                if (!tryLoadTranslations.IsSuccess)
                {
                    throw new UExcept(EInit.FailLoadDictionary, $"Не удалось загрузить словари", tryLoadTranslations.Error);
                }
                #endregion
                #region Синхронизация настроек
                var trySyncSettings = await SyncConfigs();
                if (!trySyncSettings.IsSuccess)
                {
                    var uex = new UExcept(EInit.FailSyncConfigs, $"Не удалось синхронизировать конфиги", trySyncSettings.Error);
                    Functions.Error(uex, uex.Message, _proc);
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
                        foreach (var valueName in key.GetValueNames())
                        {
                            switch (valueName.ToLower())
                            {
                                case "login": AppSettings.Instance.Login = key.GetValue(valueName)!.ToString()!; break;
                                case "password": AppSettings.Instance.Password = key.GetValue(valueName)!.ToString()!; break;
                            }
                        }
                        AppSettings.Instance.Console = true;
                        AppSettings.Save();
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

        #region CopyRegToFile
        private async Task CopyRegToFile()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось скопировать данные с реестра";

            #region try
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Byster"))
                {
                    if (key is not null)
                    {
                        #region Собираем данные
                        var dict = new Dictionary<string, object?>();
                        foreach (var valueName in key.GetValueNames()) dict[valueName] = key.GetValue(valueName);
                        var json = JsonConvert.SerializeObject(dict, Newtonsoft.Json.Formatting.Indented);
                        #endregion
                        #region Сохраняем
                        var pathToSave = Path.Combine(AppSettings.RootFolder, "reg_config.json");
                        Directory.CreateDirectory(AppSettings.RootFolder);
                        await File.WriteAllTextAsync(pathToSave, json);
                        #endregion
                        #region Удаляем раздел
                        //using (RegistryKey? parent = Registry.CurrentUser.OpenSubKey("Software", writable: true))
                        //{
                        //    if (parent is not null && parent.OpenSubKey("Byster") is not null)
                        //    {
                        //        parent.DeleteSubKeyTree("Byster");
                        //    }
                        //}
                        #endregion
                    }
                }
            }
            #endregion            
            #region Exception
            catch (Exception ex)
            {
                var uex = new UExcept(ELoader.FailCopyRegToFile, _failinf, ex);
                Functions.Error(uex, uex.Message, _proc);
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
                var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BysterConfig", "configs");
                var newConfigPath = Path.Combine(AppSettings.RootFolder, "config", "configs");
                var mediaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BysterImages");

                #region Копируем и удаляем конфиг папку
                if (Directory.Exists(configPath))
                {
                    Functions.CopyDirectory(configPath, newConfigPath);
                    Directory.Delete(configPath, true);
                }
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
