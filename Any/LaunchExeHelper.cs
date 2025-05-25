using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any.LaunchExeHelperAny;
using Launcher.Api;
using Launcher.Cls;
using Launcher.Components.MainWindow.Any.PageShop.Models;
using Launcher.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

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
                if (!Directory.Exists(AppSettings.TempBin)) Directory.CreateDirectory(AppSettings.TempBin);
                #endregion
                #region Скачиваем
                var exeBytes = await CApi.GetByster();
                if (!exeBytes.IsSuccess)
                {
                    throw new UExcept(ELaunch.FailLoadExe, $"Не удалось скачать файл", exeBytes.Error);
                }
                #endregion
                #region Проверяем установлен ли уже
                await Task.Run(() => Thread.Sleep(300));
                UpdateStatus(server, ELaunchState.Verifying);

                var needSave = true;
                var pathName = $"{Functions.GetMd5Hash(GProp.User.Username)[13..]}.exe";
                var pathExe = Path.Combine(AppSettings.TempBin, pathName);
                if (File.Exists(pathExe))
                {
                    #region Сравниваем
                    var downloadedHash = Functions.GetMd5Hash(exeBytes.Response);

                    byte[] localBytes = await File.ReadAllBytesAsync(pathExe);
                    var localHash = Functions.GetMd5Hash(localBytes);

                    if (string.Equals(localHash, downloadedHash, StringComparison.OrdinalIgnoreCase)) needSave = false;
                    #endregion
                }
                #endregion
                #region Если надо сохранить/обновить
                if (needSave)
                {
                    await Task.Run(() => Thread.Sleep(300));
                    UpdateStatus(server, ELaunchState.Saving);

                    await File.WriteAllBytesAsync(pathExe, exeBytes.Response);
                }
                #endregion
                #region Запускаем
                await Task.Run(() => Thread.Sleep(300));
                UpdateStatus(server, ELaunchState.Launching);

                var psi = new ProcessStartInfo
                {
                    FileName = pathExe,
                    Arguments = $"\"{server.PathToExe}\"",
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

                await Task.Run(() => Thread.Sleep(300));
                UpdateStatus(server, ELaunchState.Launched);

                #region Удаляем
                RemoveItem(server);
                #endregion
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);

                SetError(server, Dictionary.Translate($"Во время запуска произошла неизвестная ошибка. Попробуйте позже"));
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(GlobalErrors.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);

                SetError(server, Dictionary.Translate($"Во время запуска произошла неизвестная ошибка. Попробуйте позже"));
            }
            #endregion
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
