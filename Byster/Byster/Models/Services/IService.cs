using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
namespace Byster.Models.Services
{
    internal interface IService : IDisposable
    {
        bool IsInitialized { get; }
        Dispatcher Dispatcher { get; set; }
        void UpdateData();
        void Initialize(Dispatcher dispatcher);
    }

    internal interface INetService : IService
    {
        RestService RestService { get; set; }
    }
}
