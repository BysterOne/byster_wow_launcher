using Cls;
using Cls.Any;
using Cls.Errors;
using Cls.Exceptions;
using Launcher.Any.PingHelperAny;
using Launcher.Api;
using Launcher.Cls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Launcher.Any
{
    namespace PingHelperAny
    {
        public enum EPing
        {
            FailExecuteRequest,
            SessionWasNull
        }
    }

    public class CPingHelper
    {

        #region Переменные
        public static LogBox Pref { get; set; } = new("Ping Helper");
        private static DispatcherTimer? PingTimer { get; set; }
        #endregion

        #region Обработчики событий
        #region ETick
        private static async void ETick(object? sender, EventArgs e)
        {
            PingTimer!.Stop();
            await Ping();
            PingTimer.Start();
        }
        #endregion
        #endregion

        #region Функции
        #region Start
        public static void Start()
        {
            if (PingTimer is null)
            {
                PingTimer = new DispatcherTimer();
                PingTimer.Interval = TimeSpan.FromSeconds(10);
                PingTimer.Tick += ETick;
            }
            PingTimer.Start();
        }
        #endregion
        #region Ping
        private static async Task Ping()
        {
            var _proc = Pref.CloneAs(Functions.GetMethodName());
            var _failinf = $"Не удалось выполнить пинг";

            #region try
            try
            {
                Debug.WriteLine($"ping");
                #region Проверка наличия сессии
                if (String.IsNullOrWhiteSpace(CApi.Session)) throw new UExcept(EPing.SessionWasNull, $"Пустое значение сессии");
                #endregion
                #region Выполняем запрос                
                var tryPing = await CApi.Ping();
                if (!tryPing.IsSuccess)
                {
                    throw new UExcept(EPing.FailExecuteRequest, $"Не удалось выполнить запрос", tryPing.Error);
                }
                #endregion
                #region Обрабатываем задачи с сервера
                // TODO: Обработка задач с сервера
                #endregion
            }
            #endregion
            #region UExcept
            catch (UExcept ex)
            {
                Functions.Error(ex, _failinf, _proc);
            }
            #endregion
            #region Exception
            catch (Exception ex)
            {
                var uerror = new UError(GlobalErrors.Exception, $"Исключение: {ex.Message}");
                Functions.Error(ex, uerror, $"{_failinf}: исключение", _proc);                
            }
            #endregion
        }
        #endregion
        #endregion
    }
}
