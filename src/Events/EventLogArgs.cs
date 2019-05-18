using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext.Events
{
    public class EventLogArgs : EventBase
    {
        public EventLogArgs(EventCenter center, LogType type, string message

            ) : base(center)
        {
            Type = type;
            Message = message;
           
        }

        public LogType Type { get; private set; }

        public string Message { get; private set; }

        public Exception Error { get; private set; }
    }





}
