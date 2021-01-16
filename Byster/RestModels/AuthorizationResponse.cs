using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.RestModels
{
    class AuthorizationRequest
    {
        public string login { get; set; }
        public string password { get; set; }
    }

    class AuthorizationResponse
    {
        public string session { get; set; }
    }

    class UserInfoResponse
    {
        public string username { get; set; }
        public decimal balance { get; set; }
        public string referral_code { get; set; }
    }
}
