using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EventNext
{
    public class EventParameter
    {
        public EventParameter(ParameterInfo p)
        {
            ParameterInfo = p;
            Type = p.ParameterType;
        }

        public ParameterInfo ParameterInfo { get; private set; }

        public Type Type { get; internal set; }
    }
}
