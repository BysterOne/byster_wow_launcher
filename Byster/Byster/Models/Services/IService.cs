using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
namespace Byster.Models.Services
{
    internal interface IService
    {
        bool IsInitialized { get; set; }
        Dispatcher Dispatcher { get; set; }
        string SessionId { get; set; }
        RestService RestService { get; set; }
        void UpdateData();
        void Initialize(Dispatcher dispatcher);
    }
}
