using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterWOWModels
{
    class Rotation
    {
        public DateTime ExpiringTime { get; set; }
        public WOWClass RotationClass { get; set; }
        public string Name { get; set; }
        public Uri ImageOfRotation { get; set; }
        public string ModeOfRotation { get; set; }
        public int PVPorPVE { get; set; }
        public Rotation() { }

        enum PVPorPVEModes
        {
            PVE = 0,
            PVP = 1,
        }

    }
}
