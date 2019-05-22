using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext.Proxy
{
    public class ActorProxyCollection
    {
        private Dictionary<string, EventDispatchProxy> mActor = new Dictionary<string, EventDispatchProxy>();

        public EventDispatchProxy Get(string id)
        {
            mActor.TryGetValue(id, out EventDispatchProxy result);
            return result;
        }

        public void Set(string id, EventDispatchProxy item)
        {
            mActor[id] = item;
        }

        public void Remove(string id)
        {
            mActor.Remove(id);
        }
    }
}
