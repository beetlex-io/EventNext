using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public interface IEventActionExecuteHandler
    {
        bool Executing(EventCenter center,EventActionHandler handler, IEventInput input, IEventOutput output);

        void Executed(EventCenter center, EventActionHandler handler, IEventInput input, IEventOutput output);
    }
}
