using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestRotationWOW
    {
        public string expired_date { get; set; }

        public RestRotationPart rotation { get; set; }
        public RestRotationWOW() { }
    }

    public class RestRotationShop : RestRotationPart
    {

    }
}
