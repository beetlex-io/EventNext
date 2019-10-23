using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class,AllowMultiple =true)]
    public class ActionFilterAttribute : Attribute, IEventActionExecuteHandler
    {
        public virtual void Executed(EventCenter center, EventActionHandler handler, IEventInput input, IEventOutput output)
        {
            
        }
        public virtual bool Executing(EventCenter center, EventActionHandler handler, IEventInput input, IEventOutput output)
        {
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SkipActionFilterAttribute:Attribute
    {
        public SkipActionFilterAttribute(params Type[] types)
        {
            Types = types;
        }

        public Type[] Types { get; set; }
    }

}
