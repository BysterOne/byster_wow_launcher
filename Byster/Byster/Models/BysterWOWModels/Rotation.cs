using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Models.BysterWOWModels
{
    public class RotationWOW
    {
        public DateTime ExpiringTime { get; set; }
        public WOWClass RotationClass { get; set; }
        public string Name { get; set; }
        public string ImageOfRotation { get; set; }
        public string ModeOfRotation { get; set; }
        public int PVPorPVE { get; set; }

        public enum PVPorPVEModes
        {
            PVE = 0,
            PVP = 1,
        }

        public enum RotationModes
        {
            Defend = 0,
            Attack = 1,
        }

        public RotationWOW() { }

    }
}