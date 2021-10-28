using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    class RegisterRequestNoReferal : AuthRequest
    {
        public string register_source { get; set; }
        public RegisterRequestNoReferal() { }
    }

    class RegisterRequestReferal : RegisterRequestNoReferal
    {
        public string referal { get; set; }
        public RegisterRequestReferal() { }
    }

    class RegisterResponse : AuthResponse
    {
        public RegisterResponse() { }
    }

    class RegisterChoice
    {
        public int id { get; set; }
        public string selection { get; set; }
        public bool need_referral_code { get; set; }

        public RegisterChoice() { }
    }
}
