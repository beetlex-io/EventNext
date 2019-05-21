using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventNext
{
    public interface IEventLogHandler
    {
        Task<string> Write(IActorState actor, EventLog log);
    }

}
