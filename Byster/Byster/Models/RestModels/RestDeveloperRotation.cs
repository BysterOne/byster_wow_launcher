using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.RestModels
{
    public class RestDeveloperRotationRequest
    {
    }

    public class RestDeveloperRotation : BaseResponse
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string klass { get; set; }
        public string git_ssh_url { get; set; }
        public string file_path { get; set; }
    }
}
