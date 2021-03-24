using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    class AuthorizationRequest
    {
        public string login { get; set; }
        public string password { get; set; }
    }

    class AuthorizationResponse : BaseResponse
    {
        public string session { get; set; }
    }
}
