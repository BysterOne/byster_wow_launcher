using Byster.Models.RestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models
{
    class RegistrationChoicesResponse : BaseResponse
    {
        public int id { get; set; }
        public string selection { get; set; }
    }

    class RegistrationRequest : AuthorizationRequest
    {
        public string referal { get; set; }
        public int register_source { get; set; }
    }

    class RegistrationResponse : AuthorizationResponse { }
}
