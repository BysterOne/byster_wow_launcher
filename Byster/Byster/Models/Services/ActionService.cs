using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;


namespace Byster.Models.Services
{
    public class ActionService : IService
    {
        public string SessionId { get; set; }
        public RestService RestService { get; set; }
        
        public Action UpdateAction;
        public Timer updatingTimer;
        
        public void Init()
        {
            updatingTimer = new Timer(new TimerCallback(TimerTick), null, 5000, 5000);
        }

        private void TimerTick(object obj)
        {
            bool isUpdateRequired = RestService.GetActionState(SessionId);
            if (isUpdateRequired) UpdateAction();
        }

        public void UpdateData() { }

        public ActionService(RestService restService, Action updateDel)
        {
            RestService = restService;
            UpdateAction = updateDel;
        }
    }
}
