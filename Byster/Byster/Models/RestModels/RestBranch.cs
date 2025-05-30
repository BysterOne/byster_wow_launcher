using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestBranchResponse : BaseResponse
    {
        public bool dev { get; set; }
        public bool test { get; set; }
        public bool master { get; set; }
    }
}
