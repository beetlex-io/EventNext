using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Concurrent;
namespace EventNext
{
    public class EventActionHandler
    {
        public EventActionHandler(Type controllerType, MethodInfo method, object controller)
        {
            ControllerType = controllerType;
            Controller = controller;
            Method = method;
            MethodHandler = new MethodHandler(method);
            ResultType = method.ReturnType;
            PropertyInfo pi = method.ReturnType.GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
            if (pi != null)
                ResultProperty = new PropertyHandler(pi);
            ThreadInvokeAttribute thread = method.GetCustomAttribute<ThreadInvokeAttribute>();
            if (thread != null)
                ThreadType = thread.Type;
            int i = 0;
            foreach (var p in method.GetParameters())
            {
                if (p.GetCustomAttribute<ThreadUniqueID>() != null)
                {
                    if (p.ParameterType.IsValueType || p.ParameterType == typeof(string))
                        ThreadUniqueID.Add(i);
                }
                Parameters.Add(new EventParameter(p));
                i++;
            }
        }

        public List<int> ThreadUniqueID { get; private set; } = new List<int>();

        private ConcurrentDictionary<string, object> mProperties = new ConcurrentDictionary<string, object>();


        public ActionFilterAttribute[] Filters { get; internal set; }

        public object this[string name]
        {
            get
            {
                mProperties.TryGetValue(name, out object value);
                return value;
            }
            set
            {
                mProperties[name] = value;
            }
        }

        private NextQueue mDefaultQueue = new NextQueue();

        public ThreadType ThreadType { get; set; } = ThreadType.None;

        public bool SingleInstance { get; set; } = true;

        public bool IsTaskResult => ResultType.BaseType == typeof(Task);

        public bool IsVoid => ResultType == typeof(void);

        public string Url { get; set; }

        public Type Interface { get; set; }

        public string ServiceName { get; internal set; }

        public string ActionName { get; internal set; }

        public object Controller { get; set; }

        public Type ControllerType { get; private set; }

        public MethodInfo Method { get; private set; }

        internal MethodHandler MethodHandler { get; private set; }

        public List<EventParameter> Parameters { get; private set; } = new List<EventParameter>();

        public Type ResultType { get; set; }

        internal PropertyHandler ResultProperty { get; set; }

        public object GetResult(object result)
        {
            if (IsVoid)
                return null;
            if (IsTaskResult)
            {
                if (ResultProperty != null)
                    return ResultProperty.Get(result);
                return null;
            }
            else
            {
                return result;
            }
        }

        internal ActorCollection Actors { get; set; }

        private Dictionary<string, NextQueue> mNextQueues = new Dictionary<string, NextQueue>();

        public NextQueue GetNextQueue(object[] data)
        {
            if (ThreadUniqueID.Count == 0)
                return mDefaultQueue;
            else
            {

                string key = "";
                for (int i = 0; i < ThreadUniqueID.Count; i++)
                {
                    key += data[ThreadUniqueID[i]].ToString();
                }
                lock (mNextQueues)
                {
                    if (!mNextQueues.TryGetValue(key, out NextQueue value))
                    {
                        value = new NextQueue();
                        mNextQueues[key] = value;
                    }
                    return value;
                }
            }
        }


    }
}
