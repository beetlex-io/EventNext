using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public class EventStore
    {
        public string EventID { get; set; }

        public string ParentEventID { get; set; }

        public string ActorPath { get; set; }

        public string EventPath { get; set; }

        public string Type { get; set; }

        public DateTime DateTime { get; set; }

        public object Data { get; set; }

        public long Sequence { get; set; }

    }
}
