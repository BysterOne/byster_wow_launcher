using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{

    public class RegisterRequestNoReferalNoRegisterSource : AuthRequest
    {
        public RegisterRequestNoReferalNoRegisterSource() { }
    }
    public class RegisterRequestNoReferal : AuthRequest
    {
        public string register_source { get; set; }
        public RegisterRequestNoReferal() { }
    }

    public class RegisterRequestNoRegisterSource : AuthRequest
    {
        public string referal { get; set; }
        public RegisterRequestNoRegisterSource() { }
    }

    public class RegisterRequestReferal : RegisterRequestNoReferal
    {
        public string referal { get; set; }
        public RegisterRequestReferal() { }
    }

    public class RegisterResponse : AuthResponse
    {
        public RegisterResponse() { }
    }
}
