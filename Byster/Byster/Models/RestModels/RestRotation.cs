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

        public RestRotationPart rotation { get; set;}
        public RotationResponse() { }
    }

    
}
