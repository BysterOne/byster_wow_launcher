using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Byster.Models.BysterModels
{
    public class LoadType
    {
        public int Value { get; set; }
        public string Name { get; set; }

        public static LoadType[] AllLoadTypes { get; set; } = new LoadType[] {
            new LoadType(){ Name="Загрузка1", Value=1},
            new LoadType(){ Name="Загрузка2", Value=2},
            new LoadType(){ Name="Загрузка3", Value=3},
        };
    }
}
