using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestRotationPart
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string klass { get; set; }
        public string specialization { get; set; }
        public string role_type { get; set; }
        public List<RestMediaPart> media { get; set; }
        public RestRotationPart() { }
    }

    public class RestMediaPart
    {
        public string type { get; set; }
        public string url { get; set; }
        public RestMediaPart() { }
    }
}
