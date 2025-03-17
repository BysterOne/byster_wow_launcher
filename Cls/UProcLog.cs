using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cls.Any
{
    public static class UProcLog
    {
        public static void Log(this string proc, string message)
        {
            Cls.Log.Add($"{proc} {message}");
        }
    }
}
