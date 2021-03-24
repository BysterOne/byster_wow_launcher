using PostSharp.Patterns.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models
{
    [NotifyPropertyChanged]
    public class WoWProcess
    {
        public Process Process { get; set; }
        public string NickName { get; set; }
        public string Server { get; set; }
        public string Class { get; set; }
    }
}
