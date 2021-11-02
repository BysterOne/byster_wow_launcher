using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestRotationPart
    { 
        public string name { get; set; }
        public string type { get; set; }
        public string klass { get; set; }
        public string specialization { get; set; }
        public string role_type { get; set; }
        public List<MediaPart> media { get; set; }
        public RestRotationPart() { }
    }

    public class MediaPart
    {
        public string type { get; set; }
        public string url { get; set; }
        public MediaPart() { }
    }
}
