using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterModels
{
    public class Cart
    {
        public List<(int, int)> Products { get; set; }
        public int Bonuses { get; set; }
    }
}
