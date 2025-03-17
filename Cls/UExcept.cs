using Cls.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cls.Exceptions
{
    public class UExcept : Exception
    {
        public UError Error { get; set; }

        public UExcept(Enum errorType, string message) : base(message)
        {
            Error = new(errorType, message);
        }
        public UExcept(Enum errorType, string message, UError error) : base(message)
        {
            Error = new (errorType, message, error);
        }
    }
}
