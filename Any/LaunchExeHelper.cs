using Cls;
using Cls.Any;
using Cls.Exceptions;
using Launcher.Any.LaunchExeHelperAny;
using Launcher.Api;
using Launcher.Cls;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings;
using PEFile;
using System.Diagnostics;
using System.IO;

namespace Launcher.Any
{
    #region LaunchExeHelperAny
    namespace LaunchExeHelperAny
    {
        public enum ELaunchState
        {
            None,
            Downloading,
            Verifying,
            Saving,
            Launching,
            Launched,
            ErrorOccurred
        }

        public enum ELaunch
        {
            FailLoadExe,
            FailGetLibVersion,
            FailRunExe,
        }

        public class LaunchItem
        {
            public CServer Server { get; set; } = null!;
            public ELaunchState State { get; set; } = ELaunchState.None;
            public string? Error { get; set; } = null;
        }
    }
    #endregion

    public class LaunchExeHelper
    {
        #region Переменные
        private static LogBox Pref { get; set; } = new("Launch Exe Helper");
        private static List<LaunchItem> Items { get; set; } = [];
        #endregion

        #region События
        public delegate void LaunchItemUpdateDelegate(LaunchItem item);
        public static event LaunchItemUpdateDelegate? OnLaunchItemUpdate;
        #endregion

        #region Функции
        #region GetState
        public static LaunchItem? GetItem(CServer server)
        {
            return Items.FirstOrDefault(x => x.Server.Id == server.Id);
        }
        #endregion
        #region CanLaunch
        public static bool CanLaunch(CServer server)
        {
            if (Items.Any(x => x.Server.Id == server.Id)) return false;
            return true;
        }
        #endregion
        #region Launch
        public static async Task Launch(CServer server)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить запуск клиента";

            #region try
            try
            {
                if (!CanLaunch(server)) return;

                #region Создаем в очередь
                Items.Add(new LaunchItem() { Server = server, State = ELaunchState.None });
                UpdateStatus(server, ELaunchState.Downloading);
                #endregion
                #region Наличие папки
                await Task.Run(() => Thread.Sleep(300));
                UpdateStatus(server, ELaunchState.Verifying);

                VerifyFolders();
                var saveDir = GProp.User.Protection ? AppSettings.ProtectedFolder : AppSettings.UnprotectedFolder;
                #endregion
                #region Получение версии
                var tryGetLibVersion = await CApi.GetLibVersion();
                if (!tryGetLibVersion.IsSuccess)
                {
                    throw new UExcept(ELaunch.FailGetLibVersion, $"Не удалось получить версию либы", tryGetLibVersion.Error);
                }
                var currentVersion = tryGetLibVersion.Response.Version;
                #endregion
                #region Проверка наличия файла
                var fileDir = Path.Combine(saveDir, currentVersion);
                if (Directory.Exists(fileDir))
                {
                    var exeFile = Directory.GetFiles(fileDir, "*.exe").FirstOrDefault();
                    if (exeFile is not null)
                    {
                        var tryRun = await Run(server, exeFile);
                        if (!tryRun.IsSuccess) throw new UExcept(ELaunch.FailRunExe, $"Не удалось запустить исполняемый файл", tryRun.Error);
                        return;
                    }
                }
                #endregion
                #region Установка
                #region Скачиваем
                var exeBytes = await CApi.GetLib();
                if (!exeBytes.IsSuccess)
                {
                    throw new UExcept(ELaunch.FailLoadExe, $"Не удалось скачать файл", exeBytes.Error);
                }
                #endregion
                #region Сохраняем
                Directory.CreateDirectory(fileDir);
                var pathName = $"{Functions.GetMd5Hash(GProp.User.Username + Guid.NewGuid().ToString())[..12]}.exe";
                var pathExe = Path.Combine(fileDir, pathName);

                await Task.Run(() => Thread.Sleep(300));
                UpdateStatus(server, ELaunchState.Saving);

                await File.WriteAllBytesAsync(pathExe, exeBytes.Response);
                #endregion
                #region Запускаем
                var tryRunExe = await Run(server, pathExe);
                if (!tryRunExe.IsSuccess) throw new UExcept(ELaunch.FailRunExe, $"Не удалось запустить исполняемый файл", tryRunExe.Error);
                #endregion
                #endregion
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                Functions.Error(ex, _failinf, _proc);
                SetError(server, Dictionary.Translate($"Во время запуска произошла неизвестная ошибка. Попробуйте позже"));
            }
            #endregion
            #region finally
            finally
            {
                RemoveItem(server);
            }
            #endregion
        }
        #endregion
        #region Run
        private static async Task<UResponse> Run(CServer server, string pathExe)
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить запуск";

            #region try
            try
            {
                #region Статус
                await Task.Run(() => Thread.Sleep(300));
                UpdateStatus(server, ELaunchState.Launching);
                #endregion
                #region Запуск процесса
                var psi = new ProcessStartInfo
                {
                    FileName = pathExe,
                    Arguments = $"\"{server.PathToExe}\" \"{AppDomain.CurrentDomain.BaseDirectory}\"",
                    UseShellExecute = true,
                    Verb = "runas"
                };
                var process = Process.Start(psi);
                if (process is not null)
                {
                    await Task.Run(() => Thread.Sleep(300));
                    UpdateStatus(server, ELaunchState.Launched);
                }
                #endregion
                #region Статус
                await Task.Run(() => Thread.Sleep(300));
                UpdateStatus(server, ELaunchState.Launched);
                #endregion

                return new() { IsSuccess = true };
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
        #region VerifyFolders
        public static void VerifyFolders()
        {
            if (GProp.User.Protection && Directory.Exists(AppSettings.UnprotectedFolder))
                    try { Directory.Delete(AppSettings.UnprotectedFolder, true); } catch (Exception ex) { Debugger.Break(); }
        }
        #endregion
        #region SetError
        private static async void SetError(CServer server, string error)
        {
            UpdateStatus(server, ELaunchState.ErrorOccurred, error);
            await Task.Run(() => Thread.Sleep(2000));
            RemoveItem(server);
        }
        #endregion
        #region RemoveItem
        private static void RemoveItem(CServer server)
        {
            var removeItem = Items.FirstOrDefault(x => x.Server.Id == server.Id);
            if (removeItem is not null) Items.Remove(removeItem);
        }
        #endregion
        #region UpdateStatus
        private static void UpdateStatus(CServer server, ELaunchState state, string? error = null)
        {
            var item = Items.FirstOrDefault(x => x.Server.Id == server.Id);
            if (item is null) return;
            item.State = state;
            item.Error = error;

            OnLaunchItemUpdate?.Invoke(item);
        }
        #endregion
        #endregion
    }
}
