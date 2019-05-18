using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventNext
{
    public class EventActionHandlerContext
    {

        public EventActionHandlerContext(EventCenter server, IEventInput input, EventActionHandler handler, object controller)
        {
            Input = input;
            Server = server;
            Handler = handler;
            Controller = controller;
        }

        public object Controller { get; private set; }

        public EventActionHandler Handler { get; private set; }

        public EventCenter Server
        {
            get; private set;
        }

        public IEventInput Input
        { get; private set; }

        public async Task<object> Execute()
        {
            var result = Handler.MethodHandler.Execute(Controller, Input.Data);
            var task = result as Task;
            if (task != null)
            {
                await task;
            }
            return Handler.GetResult(result);
        }
    }
}
