using EventNext.Proxy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventNext
{
    public class EventCenter : IDisposable
    {
        static EventCenter()
        {
            Watch = new System.Diagnostics.Stopwatch();
            Watch.Start();
        }

        public string ACTOR_TAG = "ACTOR";

        public const string ACTOR_NULL_TAG = "___NULL_ACTOR";

        public EventCenter()
        {
            LogType = LogType.Error;

        }

        private ConcurrentDictionary<string, EventActionHandler> mActionHadlers = new ConcurrentDictionary<string, EventActionHandler>();

        private ConcurrentDictionary<Type, ActorProxy> mActorProxyMap = new ConcurrentDictionary<Type, ActorProxy>();

        private ConcurrentDictionary<Type, ServiceCollection> mServiceCollection = new ConcurrentDictionary<Type, ServiceCollection>();

        private ConcurrentDictionary<string, Object> mProperties = new ConcurrentDictionary<string, object>();

        protected virtual object CreateController(Type type)
        {
            if (ServiceInstance != null)
            {
                Events.EventServiceInstanceArgs e = new Events.EventServiceInstanceArgs(this, type);
                ServiceInstance(this, e);
                return e.Service ?? Activator.CreateInstance(type);
            }
            return Activator.CreateInstance(type);
        }

        private long mID = 0;

        public virtual long GetInputID()
        {
            return System.Threading.Interlocked.Increment(ref mID);
        }

        public NextQueueGroup InputNextQueue { get; set; } = new NextQueueGroup();

        public NextQueueGroup OutputNextQueue { get; set; } = new NextQueueGroup();

        public static System.Diagnostics.Stopwatch Watch { get; private set; }

        public LogType LogType { get; set; }

        public event EventHandler<Events.EventLogArgs> LogOutput;

        public bool EnabledLog(LogType type)
        {
            return (this.LogType & type) > 0;
        }

        public void Log(LogType logType, string message)
        {
            LogOutput?.Invoke(this, new Events.EventLogArgs(this, logType, message));
        }

        private void OnRegister(ServiceAttribute attribute, Type type, object controller)
        {
            foreach (Type itype in attribute.Types)
            {
                if (!itype.IsInterface)
                {
                    continue;
                }
                if (type.GetInterface(itype.Name) == null)
                {
                    continue;
                }
                string serviceName = (attribute.Name ?? itype.Name);
                string url = "/" + serviceName + "/";
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    try
                    {
                        if (string.Compare("Equals", method.Name, true) == 0
                      || string.Compare("GetHashCode", method.Name, true) == 0
                      || string.Compare("GetType", method.Name, true) == 0
                      || string.Compare("ToString", method.Name, true) == 0 || method.Name.IndexOf("set_") >= 0
                      || method.Name.IndexOf("get_") >= 0)
                            continue;
                        var resultType = method.ReturnType;
                        if (resultType.BaseType == typeof(Task) || resultType == typeof(Task))
                        {
                            Type serviceType = controller.GetType();
                            if (!mServiceCollection.TryGetValue(serviceType, out ServiceCollection serviceCollection))
                            {
                                serviceCollection = new ServiceCollection(serviceType);
                                mServiceCollection[serviceType] = serviceCollection;
                                mServiceCollection[itype] = serviceCollection;
                            }
                            ActionAttribute aa = method.GetCustomAttribute<ActionAttribute>(false);

                            var actionUrl = url + (aa == null ? method.Name : aa.Name);
                            if (mActionHadlers.TryGetValue(actionUrl, out EventActionHandler handler))
                            {
                                Log(LogType.Warring, $"{itype.Name}->{type.Name}.{method.Name} action already exists, can add ActionAttribute on the method");
                            }
                            else
                            {
                                handler = new EventActionHandler(type, method, controller);
                                handler.ServiceName = serviceName;
                                handler.ActionName = (aa == null ? method.Name : aa.Name);
                                handler.SingleInstance = attribute.SingleInstance;
                                handler.ServiceCollection = serviceCollection;
                                handler.Interface = itype;
                                mActionHadlers[actionUrl] = handler;
                                Log(LogType.Info, $"Register {itype.Name}->{type.Name}@{method.Name} to {actionUrl}");
                            }
                        }
                        else
                        {
                            Log(LogType.Error, $"Register {itype.Name}->{type.Name}.{method.Name} error, method result type not a Task!");
                        }
                    }
                    catch (Exception e_)
                    {
                        Log(LogType.Error, $"Register {itype.Name}->{type.Name}@{method.Name} action error {e_.Message}@{e_.StackTrace}");
                    }
                }
            }
        }

        public event EventHandler<Events.EventServiceInstanceArgs> ServiceInstance;

        public Object this[string name]
        {
            get
            {
                return mProperties[name];
            }
            set
            {
                mProperties[name] = value;
            }
        }

        public T GetProperty<T>(string name)
        {
            return (T)this[name];
        }

        public void Register(object service)
        {

            Type type = service.GetType();
            var attribute = type.GetCustomAttribute<ServiceAttribute>(false);
            if (attribute != null)
            {
                OnRegister(attribute, type, service);
            }
            else
            {
                Log(LogType.Warring, $"{type.Name} no controller attribute");
            }
        }

        public void Register(params Assembly[] assemblies)
        {
            foreach (Assembly item in assemblies)
            {
                foreach (Type type in item.GetTypes())
                {
                    try
                    {
                        if (type.IsPublic && !type.IsAbstract && type.IsClass)
                        {
                            var attribute = type.GetCustomAttribute<ServiceAttribute>(false);
                            if (attribute != null)
                            {
                                object controller = CreateController(type);
                                OnRegister(attribute, type, controller);
                            }
                        }
                    }
                    catch (Exception e_)
                    {
                        Log(LogType.Error, $"Register {type.Name} error {e_.Message}@{e_.StackTrace}");
                    }
                }
            }
        }

        public EventActionHandler GetActionHandler(string url)
        {
            mActionHadlers.TryGetValue(url, out EventActionHandler handler);
            return handler;
        }

        public async Task<IEventOutput> Execute(IEventInput input)
        {
            if (input.ID == 0)
                input.ID = GetInputID();
            double runTime = Watch.Elapsed.TotalMilliseconds;
            EventOutput output = new EventOutput();
            output.ID = input.ID;
            try
            {
                output.EventError = EventError.Success;
                EventActionHandler handler = GetActionHandler(input.EventPath);
                if (handler == null)
                {
                    output.EventError = EventError.NotFound;
                    output.Data = new object[] { $"Process event error '{input.EventPath}' not found!" };
                }
                else
                {

                    var actorID = input.Properties?[ACTOR_TAG];
                    string actorPath = null;
                    object controller = null;
                    if (string.IsNullOrEmpty(actorID))
                    {
                        controller = handler.Controller;
                        if (!handler.SingleInstance)
                        {
                            controller = CreateController(handler.ControllerType);
                        }
                    }
                    else
                    {
                        controller = handler.ServiceCollection.Get(actorID);
                        if (controller == null)
                        {
                            actorPath = "/" + handler.ServiceName + "/" + actorID;
                            if (EnabledLog(LogType.Debug))
                                Log(LogType.Debug, $"create {handler.ControllerType.Name}@{actorPath} actor");
                            controller = CreateController(handler.ServiceCollection.ServiceType);
                            handler.ServiceCollection.Set(actorID, controller);
                            IActorState state = controller as IActorState;
                            if (state != null)
                            {
                                state.Path = actorPath;
                                state.EventCenter = this;
                                state.Init(actorID);
                                if (EnabledLog(LogType.Debug))
                                    Log(LogType.Debug, $"create {handler.ControllerType.Name}@{actorPath} actor initialized");
                            }
                        }
                    }
                    EventActionHandlerContext context = new EventActionHandlerContext(this, input, handler, controller);
                    var result = await context.Execute();
                    if (result != null)
                        output.Data = new object[] { result };
                    if (EnabledLog(LogType.Debug))
                        Log(LogType.Debug, $"execute {handler.ControllerType.Name}@{handler.ActionName} actor path {actorPath} successed");
                }
            }
            catch (Exception e_)
            {
                output.EventError = EventError.InnerError;
                output.Data = new object[] { $"Process event {input.EventPath} error {e_.Message}" };
                if (EnabledLog(LogType.Error))
                    Log(LogType.Error, $"Process event {input.EventPath} error {e_.Message}@{e_.StackTrace}");
            }
            output.ResponseTime = Watch.Elapsed.TotalMilliseconds - runTime;
            return output;
        }

        private ActorProxy GetActorProxy(Type type)
        {
            if (!mActorProxyMap.TryGetValue(type, out ActorProxy actorProxy))
            {
                lock (mActorProxyMap)
                {
                    if (!mActorProxyMap.TryGetValue(type, out actorProxy))
                    {
                        actorProxy = new ActorProxy();
                        mActorProxyMap[type] = actorProxy;
                    }
                }
            }
            return actorProxy;
        }

        private T CreateActorProxy<T>(string actor)
        {
            Type type = typeof(T);
            var actorProxy = GetActorProxy(type);
            var result = actorProxy.Get(actor);
            if (result == null)
            {
                lock (actorProxy)
                {
                    result = actorProxy.Get(actor);
                    if (result == null)
                    {
                        result = (EventDispatchProxy)CreateProxy<T>(actor);
                        actorProxy.Set(actor, result);
                    }
                }
            }
            return (T)(object)result;
        }

        private object CreateProxy<T>(string actor = null)
        {
            object result = DispatchProxy.Create<T, Proxy.EventDispatchProxy>();
            EventDispatchProxy dispatch = ((EventDispatchProxy)result);
            dispatch.EventCenter = this;
            dispatch.Actor = actor;
            dispatch.Type = typeof(T);
            dispatch.InitHandlers();
            if (EnabledLog(LogType.Debug))
                Log(LogType.Debug, $"create {dispatch.Type.Name}@{actor} event dispatch proxy successed!");
            return result;
        }

        public T Create<T>(string actor = null)
        {
            Type type = typeof(T);
            if (!type.IsInterface)
            {
                var error = new ENException($"{type.Name} not a interface!");
                error.EventError = EventError.NotSupport;
                throw error;
            }
            if (string.IsNullOrEmpty(actor))
            {
                actor = ACTOR_NULL_TAG;
            }
            return CreateActorProxy<T>(actor);
        }

        public void ActorFlush<T>(string actor)
        {
            ActorFlush(typeof(T), actor);
        }

        public void ActorFlush(Type type, string actor)
        {
            if (mServiceCollection.TryGetValue(type, out ServiceCollection service))
            {
                var state = service.Get(actor) as IActorState;
                if (state != null)
                {
                    try
                    {
                        state.Flush();
                        if (EnabledLog(LogType.Debug))
                        {
                            Log(LogType.Debug, $"Flash {state.Path} success");
                        }
                    }
                    catch (Exception e_)
                    {
                        if (EnabledLog(LogType.Error))
                        {
                            Log(LogType.Error, $"Flash {state.Path} error {e_.Message}@{e_.StackTrace}");
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            mActionHadlers.Clear();
            mActorProxyMap.Clear();
        }
    }
}
