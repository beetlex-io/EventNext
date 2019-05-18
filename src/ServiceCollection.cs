using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace EventNext
{
   public class ServiceCollection
    {

        public ServiceCollection(Type type)
        {
            ServiceType = type;
        }

        private ConcurrentDictionary<string, Object> mServices = new ConcurrentDictionary<string, object>();

        public object Get(string id)
        {
            mServices.TryGetValue(id, out object result);
            return result;
        }

        public void Set(string id, object service)
        {
            mServices[id] = service;
        }

        public Type ServiceType { get; private set; }

    }
}
