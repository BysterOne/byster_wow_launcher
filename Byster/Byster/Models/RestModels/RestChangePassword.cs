using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestChangePasswordRequest
    {
        public string new_password { get; set; }
    }

    public class RestChangePasswordResponse : BaseResponse
    {

    }
}
