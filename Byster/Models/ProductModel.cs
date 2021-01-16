using PostSharp.Patterns.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models
{
    [NotifyPropertyChanged]
    class ProductModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string Specialization { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }  
        public DurationType DurationType { get; set; }
        public int Duration { get; set; }

        public List<object> Attachments { get; set; }

    }

    enum DurationType : int
    {
        Month = 0,
        Unlimited = 1,
        TimeSpan = 2
    }
}


