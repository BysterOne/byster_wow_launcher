using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Byster.Models.BysterModels;
using Byster.Models.RestModels;

namespace Byster.Models.ViewModels
{
    public class ActiveRotationViewModel : ActiveRotation
    {
        public ActiveRotationViewModel(RestRotationWOW rot) : base(rot)
        {

        }
    }
}
