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
            foreach (var p in method.GetParameters())
            {
                Parameters.Add(new EventParameter(p));
            }
        }

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

        internal ServiceCollection ServiceCollection { get; set; }

    }
}
