using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventNext
{
    class EventActionHandlerContext : IEventWork, IEventActionContext
    {

        public EventActionHandlerContext(EventCenter server, IEventInput input, EventActionHandler handler, object controller, NextQueue nextQueue,
            ActorCollection.ActorItem actorItem)
        {
            Input = input;
            EventCenter = server;
            Handler = handler;
            Controller = controller;
            NextQueue = nextQueue;
            ActorItem = actorItem;

        }

        public NextQueue NextQueue { get; private set; }

        public object Controller { get; private set; }

        public EventActionHandler Handler { get; private set; }

        public ActorCollection.ActorItem ActorItem { get; private set; }

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
            IList<IEventActionExecuteHandler> eaeh = EventCenter.ExecuteHandlers;
            ActionFilterAttribute[] filters = Handler.Filters;
            int index = 0;
            bool _continue = true;
            int subindex = 0;
            try
            {
                if (Controller is IActorState state)
                {
                    state.Token = Input.Token;
                    state.EventPath = Input.EventPath;
                    if (ActorItem != null)
                        state.Sequence = ActorItem.GetSequence();
                }
                if (EventCenter.EnabledLog(LogType.Debug))
                    EventCenter.Log(LogType.Debug, $"[{Input.ID}]{Input.Token} process event {Input.EventPath} beginning invoke method");
                EventCenter.EventActionContext = this;
                for (int i = 0; i < eaeh.Count; i++)
                {
                    _continue = eaeh[i].Executing(EventCenter, Handler, Input, mEventOutput);
                    index++;
                    if (!_continue)
                        break;
                }
                for (int i = 0; i < filters.Length; i++)
                {
                    _continue = filters[i].Executing(EventCenter, Handler, Input, mEventOutput);
                    subindex++;
                    if (!_continue)
                        break;
                }
                if (_continue)
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
                    if (EventCenter.EnabledLog(LogType.Debug))
                        EventCenter.Log(LogType.Debug, $"[{Input.ID}]{Input.Token} process event {Input.EventPath} invoke method completed");
                }
            }
            catch (Exception e_)
            {
                mEventOutput.EventError = EventError.InnerError;
                mEventOutput.Data = new object[] { $"Process event {Input.EventPath} error {e_.Message}" };
                if (EventCenter.EnabledLog(LogType.Error))
                    EventCenter.Log(LogType.Error, $"[{Input.ID}]{Input.Token} process event {Input.EventPath} invoke method error {e_.Message}@{e_.StackTrace}");
            }
            finally
            {

                for (int i = subindex - 1; i >= 0; i--)
                {
                    try
                    {
                        filters[i].Executed(EventCenter, Handler, Input, mEventOutput);
                    }
                    catch (Exception e_)
                    {
                        if (EventCenter.EnabledLog(LogType.Error))
                            EventCenter.Log(LogType.Error, $"[{Input.ID}]{Input.Token} process event {Input.EventPath} filter invoke executed error {e_.Message}@{e_.StackTrace}");
                    }
                }

                for (int i = index - 1; i >= 0; i--)
                {
                    try
                    {
                        eaeh[i].Executed(EventCenter, Handler, Input, mEventOutput);
                    }
                    catch (Exception e_)
                    {
                        if (EventCenter.EnabledLog(LogType.Error))
                            EventCenter.Log(LogType.Error, $"[{Input.ID}]{Input.Token} process event {Input.EventPath} invoke executed handler error {e_.Message}@{e_.StackTrace}");
                    }
                }

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

    public interface IEventActionContext
    {
        object Controller { get; }

        IEventInput Input { get; }

        EventCenter EventCenter { get; }

        EventActionHandler Handler { get; }


    }

}
