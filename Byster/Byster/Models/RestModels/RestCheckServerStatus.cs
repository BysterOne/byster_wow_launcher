using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestCheckServerStatusRequest
    {
        public string realm_list { get; set; }
        public string realm_name { get; set; }
    }

    public class RestCheckServerStatusResponse
    {
        public string address { get; set; }
        public bool is_active { get; set; }
        public string short_name { get; set; }
    }
}
