using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.Services
{
    internal interface IService
    {
        string SessionId { get; set; }
        RestService RestService { get; set; }
        void UpdateData();
    }
}
