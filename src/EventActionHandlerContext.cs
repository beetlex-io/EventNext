using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventNext
{
    class EventActionHandlerContext : IEventWork
    {

        public EventActionHandlerContext(EventCenter server, IEventInput input, EventActionHandler handler, object controller, NextQueue nextQueue)
        {
            Input = input;
            EventCenter = server;
            Handler = handler;
            Controller = controller;
            NextQueue = nextQueue;

        }

        public NextQueue NextQueue { get; private set; }

        public object Controller { get; private set; }

        public EventActionHandler Handler { get; private set; }

        public EventCenter EventCenter
        {
            get; private set;
        }

        public IEventInput Input
        { get; private set; }

        private EventOutput mEventOutput;

        private IEventCompleted mEventCompleted;

        public async Task Execute()
        {
            try
            {
                var result = Handler.MethodHandler.Execute(Controller, Input.Data);
                if (result is Task task)
                {
                    await task;
                }
                object data = Handler.GetResult(result);
                mEventOutput.EventError = EventError.Success;
                if (data != null)
                    mEventOutput.Data = new object[] { data };
            }
            catch (Exception e_)
            {
                mEventOutput.EventError = EventError.InnerError;
                mEventOutput.Data = new object[] { $"Process event {Input.EventPath} error {e_.Message}" };
                if (EventCenter.EnabledLog(LogType.Error))
                    EventCenter.Log(LogType.Error, $"{Input.Token} process event {Input.EventPath} error {e_.Message}@{e_.StackTrace}");
            }
            finally
            {
                mEventCompleted.Completed(mEventOutput);
            }
        }

        public void Execute(EventOutput eventOutput, IEventCompleted completed)
        {
            mEventOutput = eventOutput;
            mEventCompleted = completed;
            ThreadType threadType = Handler.ThreadType;
            if (threadType == ThreadType.ThreadPool)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(async (d) => await Execute());
            }
            else
            {
                NextQueue.Enqueue(this);
            }
        }

        public void Dispose()
        {

        }

    }
}
