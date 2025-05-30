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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private static int Counter500Errors { get; set; }
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
                    HttpStatusCode? errorCode = tryPing.Error.Data.Contains("StatusCode") ? (HttpStatusCode)tryPing.Error.Data["StatusCode"] : null;
                    if (errorCode is HttpStatusCode code)
                    {
                        var errs500 = new HttpStatusCodeRange(500, 599);
                        if (errs500.Contains(code)) 
                        {
                            Counter500Errors++;
                            PingTimer.Interval = TimeSpan.FromSeconds
                            (
                                Counter500Errors switch
                                {
                                    1 => 15,
                                    2 => 20,
                                    _ => 30
                                }
                            );                           
                        }
                        
                        if (Counter500Errors > 3 || code is HttpStatusCode.Forbidden || code is HttpStatusCode.Unauthorized) 
                            Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                    }

                    throw new UExcept(EPing.FailExecuteRequest, $"Не удалось выполнить запрос", tryPing.Error);
                }
                #endregion
                #region Очищаем если были ошибки
                if (Counter500Errors > 0)
                {
                    PingTimer.Interval = TimeSpan.FromSeconds(10);
                    Counter500Errors = 0;
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
                var uexcept = new UExcept(GlobalErrors.Exception, $"Исключение: {ex.Message}", ex);
                Functions.Error(uexcept, $"{_failinf}: исключение", _proc);                
            }
            #endregion
        }
        #endregion
        #endregion
    }
}
