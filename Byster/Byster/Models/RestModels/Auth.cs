using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace Byster.Models.RestModels
{
    class AuthRequest
    {
        public string login { get; set; }
        public string password { get; set; }
        public AuthRequest() { }
    }
    class AuthResponse : BaseResponse
    {
        public string session { get; set; }
        public AuthResponse() { }
    }
}
