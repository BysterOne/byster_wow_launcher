using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterModels
{
    public class Media
    {
        public string Uri { get; set; }
        public MediaTypes Type { get; set; }

        public static MediaTypes GetMediaTypeByName(string name)
        {
            List<string> names = new List<string>()
            {
                null,
                "img",
                "video"
            };
            return (MediaTypes)names.IndexOf(name);
        }

        public Media(string uri, MediaTypes type)
        {
            Uri = uri;
            Type = type;
        }
    }

    public enum MediaTypes
    {
        Unknown = 0,
        Image = 1,
        Video = 2,
    }
}
