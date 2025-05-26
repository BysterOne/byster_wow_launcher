using Cls.Errors;
using Cls.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cls.Any
{
    public class UResponse
    {
        public bool IsSuccess { get; set; } = false;
        public UExcept Error { get; set; } = null!;

        public UResponse() { }
        public UResponse(UExcept error)
        {
            IsSuccess = false;
            Error = error;
        }
    }

    public class UResponse<T>
    {
        public bool IsSuccess { get; set; } = false;
        public UExcept Error { get; set; } = null!;
        public T Response { get; set; }

        public UResponse(T obj) 
        {
            IsSuccess = true;
            Response = obj;
        }

        public UResponse(UExcept error)
        {
            IsSuccess = false;
            Error = error;
        }
    }
}
