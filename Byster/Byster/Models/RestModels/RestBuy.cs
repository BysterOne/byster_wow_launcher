using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestBuyRequest
    {
        public List<RestBuyProduct> items { get; set; }
        public int bonuses { get; set; }
        public int payment_system_id { get; set; }
    }

    public class RestBuyResponse : BaseResponse
    {
        public string payment_url { get; set; }
        public string status { get; set; }
    }

    public class RestBuyProduct
    {
        public int product_id { get; set; }
        public int amount { get; set; }
    }
}
