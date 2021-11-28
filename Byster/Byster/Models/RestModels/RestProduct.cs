using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestShopProduct
    {
        public int id { get; set; }
        public string name { get; set; }
        public string image_url { get; set; }
        public string description { get; set; }
        public double price { get; set; }
        public string currency { get; set; }
        public int duration { get; set; }
        public bool can_test { get; set; }
        public List<RestMediaPart> media { get; set; }
        public List<RestRotationShop> rotations { get; set; }

    }
}
