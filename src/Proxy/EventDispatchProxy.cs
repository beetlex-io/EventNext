using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventNext.Proxy
{
    public class EventDispatchProxy : DispatchProxy,IHeader
    {
        public EventDispatchProxy()
        {
        }

        private Dictionary<string, ActionHandlerProxy> mHandlers = new Dictionary<string, ActionHandlerProxy>();

        private Dictionary<string, string> mHeader = new Dictionary<string, string>();

        public Type Type { get; internal set; }

        public Dictionary<string, ActionHandlerProxy> Handlers => mHandlers;

        public NextQueue ActorNextQueue = new NextQueue();

        public EventCenter EventCenter { get; internal set; }

        protected void OnActorFlush()
        {
            if (Actor != EventCenter.ACTOR_NULL_TAG)
            {
                EventCenter.ActorFlush(Type, Actor);
            }
        }

        public string Actor { get; internal set; }

        public string Name { get; internal set; }

        public Dictionary<string, string> Header => mHeader;

        internal void InitHandlers()
        {
            Type type = Type;
            ServiceAttribute attribute = type.GetCustomAttribute<ServiceAttribute>(false);
            Name = (attribute?.Name ?? type.Name);
            string url = "/" + Name + "/";
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (string.Compare("Equals", method.Name, true) == 0
              || string.Compare("GetHashCode", method.Name, true) == 0
              || string.Compare("GetType", method.Name, true) == 0
              || string.Compare("ToString", method.Name, true) == 0 || method.Name.IndexOf("set_") >= 0
              || method.Name.IndexOf("get_") >= 0)
                    continue;
                ActionAttribute aa = method.GetCustomAttribute<ActionAttribute>(false);
                var actionUrl = url + (aa == null ? method.Name : aa.Name);
                var handler = mHandlers.Values.FirstOrDefault(c => c.Url == actionUrl);
                if (handler != null)
                {
                    throw new ENException($"{type.Name}.{method.Name} action already exists, can add ActionAttribute on the method");
                }
                else
                {
                    handler = new ActionHandlerProxy(method);
                    handler.Url = actionUrl;
                    mHandlers[method.Name] = handler;
                }
            }
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (!mHandlers.TryGetValue(targetMethod.Name, out ActionHandlerProxy handler))
            {
                var error = new ENException($"{targetMethod.Name} action not found!");
                error.EventError = EventError.NotFound;
                throw error;
            }
            else
            {
                if (!handler.IsTaskResult)
                {
                    var error = new ENException("Definition is not supported, please define task with return value!");
                    error.EventError = EventError.NotSupport;
                    throw error;
                }
                var input = new EventInput();
                input.ID = EventCenter.GetInputID();
                input.EventPath = handler.Url;
                input.Data = args;
                if (Actor != EventCenter.ACTOR_NULL_TAG)
                {
                    input.Properties = new Dictionary<string, string>();
                    input.Properties[EventCenter.ACTOR_TAG] = this.Actor;
                }
                if(mHeader.Count>0)
                {
                    if (input.Properties == null)
                        input.Properties = new Dictionary<string, string>();
                    foreach(var item in mHeader)
                    {
                        input.Properties[item.Key] = item.Value;
                    }
                }
                var task = handler.GetCompletionSource();
                ProxyInputWork inputWork = new ProxyInputWork { CompletionSource = task, Input = input, EventCenter = EventCenter, DispatchProxy = this };
                if (Actor == EventCenter.ACTOR_NULL_TAG)
                {
                    if (EventCenter.EnabledLog(LogType.Debug))
                        EventCenter.Log(LogType.Debug, $"{input.Token} {Type.Name} proxy executing {input.EventPath}");
                }
                else
                {
                    if (EventCenter.EnabledLog(LogType.Debug))
                        EventCenter.Log(LogType.Debug, $"{input.Token} {Type.Name} proxy {Type.Name}/{Actor} actor executing {input.EventPath} ");
                }
                inputWork.Execute();
                return task.GetTask();
            }
        }

        protected void OnEventInvoke(IAnyCompletionSource e, IEventInput input, IEventOutput output)
        {
            ProxyOutputWork resultWork = new ProxyOutputWork { CompletionSource = e, EventCenter = EventCenter, Input = input, Output = output, EventDispatchProxy = this };
            EventCenter.OutputNextQueue.Enqueue(resultWork, EventCenter.NextQueueWaits);
        }

        public override string ToString()
        {
            return $"/{Name}/{Actor}";
        }

        class ProxyOutputWork : IEventWork
        {
            public void Dispose()
            {

            }

            public EventDispatchProxy EventDispatchProxy { get; set; }

            public IEventInput Input { get; set; }

            public IEventOutput Output { get; set; }

            public IAnyCompletionSource CompletionSource { get; set; }

            public EventCenter EventCenter { get; set; }

            public Task Execute()
            {
                if (Output.EventError != EventError.Success)
                {
                    if (EventCenter.EnabledLog(LogType.Debug))
                        EventCenter.Log(LogType.Debug, $"{Input.Token} {EventDispatchProxy.Type.Name} proxy execute {Input.EventPath} error {(string)Output.Data[0]}");
                    ENException exception = new ENException((string)Output.Data[0]);
                    exception.EventError = Output.EventError;
                    CompletionSource.Error(exception);
                }
                else
                {
                    if (EventCenter.EnabledLog(LogType.Debug))
                        EventCenter.Log(LogType.Debug, $"{Input.Token} {EventDispatchProxy.Type.Name} proxy execute {Input.EventPath} successed!");
                    if (Output.Data != null && Output.Data.Length > 0)
                    {
                        CompletionSource.Success(Output.Data[0]);
                    }
                    else
                    {
                        CompletionSource.Success(new object());
                    }
                }
                return Task.CompletedTask;
            }
        }

        class ProxyInputWork : IEventCompleted
        {
            public EventCenter EventCenter { get; set; }

            public IEventInput Input { get; set; }

            public IAnyCompletionSource CompletionSource { get; set; }

            public EventDispatchProxy DispatchProxy { get; set; }

            public void Completed(IEventOutput data)
            {
                DispatchProxy.OnEventInvoke(CompletionSource, Input, data);
            }

            public void Dispose()
            {

            }

            public void Execute()
            {
                EventCenter.Execute(Input, this);
            }
        }

    }





}
