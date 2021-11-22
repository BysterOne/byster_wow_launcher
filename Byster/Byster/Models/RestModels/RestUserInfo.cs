using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestUserInfoResponse : BaseResponse
    {
        public string username { get; set; }
        public double balance { get; set; }
        public string referral_code { get; set; }
    }
}
