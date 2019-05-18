using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext.Events
{
    public class EventBase : System.EventArgs
    {
        public EventBase(EventCenter server)
        {
            Center = server;
        }

        public EventCenter Center { get; private set; }
    }
}
