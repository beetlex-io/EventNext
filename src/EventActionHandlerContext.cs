using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventNext
{
    public class EventActionHandlerContext : IEventWork
    {

        public EventActionHandlerContext(EventCenter server, IEventInput input, EventActionHandler handler, object controller, NextQueue nextQueue)
        {
            Input = input;
            Server = server;
            Handler = handler;
            Controller = controller;
            NextQueue = nextQueue;

        }

        private TaskCompletionSource<object> mThreadTaskCompletionSource;

        public NextQueue NextQueue { get; private set; }

        public object Controller { get; private set; }

        public EventActionHandler Handler { get; private set; }

        public EventCenter Server
        {
            get; private set;
        }

        public IEventInput Input
        { get; private set; }

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
                mThreadTaskCompletionSource.TrySetResult(data);
            }
            catch (Exception e_)
            {
                mThreadTaskCompletionSource.TrySetException(e_);
            }
        }

        public Task<object> Invoke()
        {
            ThreadType threadType = Handler.ThreadType;
            mThreadTaskCompletionSource = new TaskCompletionSource<object>();
            if (threadType == ThreadType.ThreadPool)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(async (d) => await Execute());
            }
            else
            {
                NextQueue.Enqueue(this);
            }
            return mThreadTaskCompletionSource.Task;
        }

        public void Dispose()
        {

        }

    }
}
