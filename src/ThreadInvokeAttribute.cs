using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ThreadInvokeAttribute : Attribute
    {
        public ThreadInvokeAttribute(ThreadType type)
        {
            Type = type;
        }

        public ThreadType Type { get; set; }
    }

    public enum ThreadType
    {
        None,
        ThreadPool,
        SingleQueue
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class ThreadUniqueID : Attribute
    {

    }
}
