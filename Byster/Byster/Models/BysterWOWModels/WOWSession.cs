using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterWOWModels
{
    public class WOWSession
    {
        public string UserName { get; private set; }
        public string ServerName { get; private set; }
        public WOWClass SessionClass { get; private set; }

        public WOWSession() { }
    }
}
