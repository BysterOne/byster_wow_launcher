using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Byster.Protection.Utils
{
    public static class EnumExtentions
    {
        public static T SetFlag<T>(this Enum value, T flag, bool set)
        {
            Type underlyingType = Enum.GetUnderlyingType(value.GetType());

            dynamic valueAsInt = Convert.ChangeType(value, underlyingType);
            dynamic flagAsInt = Convert.ChangeType(flag, underlyingType);
            if (set)
            {
                valueAsInt |= flagAsInt;
            }
            else
            {
                valueAsInt &= ~flagAsInt;
            }

            return (T)valueAsInt;
        }
    }
}
