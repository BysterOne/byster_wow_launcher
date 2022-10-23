using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using static Byster.Models.Utilities.BysterLogger;
using NLog.LayoutRenderers.Wrappers;

namespace Byster.Models.Services
{
    public class ActionService : IService, IDisposable
    {
        private Random rnd = new Random();
        private int tickCounter = 0;
        private int tickLimiter = 0;
        public bool IsInitialized { get; set; } = false;
        public Dispatcher Dispatcher { get; set; }
        public string SessionId { get; set; }
        public RestService RestService { get; set; }

        public Action UpdateAction;
        public Timer updatingTimer;

        private void TimerTick(object obj)
        {
            if (tickCounter++ == tickLimiter)
            {
                tickCounter = 0;
                tickLimiter = rnd.Next(7, 16);
                bool isUpdateRequired = RestService.GetActionState(SessionId);
                if (isUpdateRequired)
                {
                    LogInfo("Action Service", "Получен запрос на обновление данных");
                    Dispatcher?.Invoke(() =>
                    {
                        if (UpdateAction != null)
                            UpdateAction();
                        LogInfo("Action Service", "Запрос на обновление обработан");
                    });
                }
            }
        }

        public void UpdateData() { }

        public ActionService(RestService restService, Action updateDel)
        {
            RestService = restService;
            UpdateAction = updateDel;
        }

        public void Dispose()
        {
            updatingTimer?.Dispose();
        }

        public async void Initialize(Dispatcher dispatcher)
        {
            LogInfo("Action Service", "Запуск сервиса...");
            Dispatcher = dispatcher;
            IsInitialized = true;
            await Task.Run(() => updatingTimer = new Timer(new TimerCallback(TimerTick), null, 0, 1000));
            LogInfo("Action Service", "Запуск сервиса завершён");

        }
    }
}
