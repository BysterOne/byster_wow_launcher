using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Byster.Models.Utilities
{
    static class HashCalc
    {

        private static char[] abc = new char[62];
        static HashCalc()
        {
            int index = 0;
            for (int i = (int)'0'; i <= (int)'9'; i++) abc[index++] = (char)i;
            for (int i = (int)'a'; i <= (int)'z'; i++) abc[index++] = (char)i;
            for (int i = (int)'A'; i <= (int)'Z'; i++) abc[index++] = (char)i;
        }
        public static string GetMD5Hash(string inputStr)
        {
            byte[] buffer = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(inputStr));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                builder.Append(buffer[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static string GetRandomString(int length)
        {
            StringBuilder sb = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(abc[random.Next(abc.Length)]);
            }
            return sb.ToString();
        }
    }
}
