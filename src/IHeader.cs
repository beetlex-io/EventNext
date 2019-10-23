using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public interface IHeader
    {
       Dictionary<string,string> Header { get; }
    }
}
