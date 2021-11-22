using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestAction
    {
        public string action_id { get; set; }
        public int action_type { get; set; }
        public double expires { get; set; }
        public string payload { get; set; }
        public string session { get; set; }
        public string username { get; set; }
    }
}