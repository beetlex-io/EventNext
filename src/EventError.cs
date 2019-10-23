using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public enum EventError
    {
        None = 0,
        Success = 200,
        InnerError = 500,
        TimeOut = 408,
        NotSupport = 403,
        NotFound = 404,
        DataError = 412
    }
}
