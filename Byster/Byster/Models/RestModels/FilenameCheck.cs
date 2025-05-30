using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Byster.Models.RestModels
{
    public class FilenameCheckResponse : BaseResponse
    {
        public string referal { get; set; }
        public int register_source { get; set; }
    }

    public class FilenameCheckRequest
    {
        public string filename { get; set; }
    }
}
