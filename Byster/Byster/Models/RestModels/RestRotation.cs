using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RotationResponse : BaseResponse
    {
        public string expired_date { get; set; }

        public PartRotationResponse rotation {get; set;}
        public RotationResponse() { }
    }

    public class PartRotationResponse
    {
        public string name { get; set; }
        public string type { get; set; }
        public string klass { get; set; }
        public string specialization { get; set; }
        public string role_type { get; set; }
    }
}
