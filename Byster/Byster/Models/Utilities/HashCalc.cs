using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Byster.Models.Utilities
{
    class HashCalc
    {
        public static string GetMD5Hash(string inputStr)
        {
            byte[] buffer = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(inputStr));
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < buffer.Length; i++)
            {
                builder.Append(buffer[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
