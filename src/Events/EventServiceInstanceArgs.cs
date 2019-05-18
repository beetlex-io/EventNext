using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext.Events
{
    public class EventServiceInstanceArgs : EventBase
    {
        public EventServiceInstanceArgs(EventCenter center, Type type) : base(center)
        {
            Type = type;
        }

        public Type Type { get; internal set; }

        public object Service { get; set; }
    }
}
