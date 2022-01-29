using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestAddRotationRequest
    {
        public string name { get; set; }
        public string description { get; set; }
        public int type { get; set; }
        public string klass { get; set; }
        public string specialization { get; set; }
        public string role_type { get; set; }
    }
    public class RestAddRotationResponse : RestDeveloperRotation
    {

    }
}
