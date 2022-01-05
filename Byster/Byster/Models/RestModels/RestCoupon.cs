using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestCouponRequest
    {
        public string coupon_code { get; set; }
    }

    public class RestCouponResponse : BaseResponse
    {
    }
}
