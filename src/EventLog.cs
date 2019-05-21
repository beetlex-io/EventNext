using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public class EventLog
    {
        public string EventID { get; set; }

        public string ParentEventID { get; set; }

        public string ActorPath { get; set; }

        public string EventPath { get; set; }

        public DateTime DateTime { get; set; }

        public object Data { get; set; }

    }
}
