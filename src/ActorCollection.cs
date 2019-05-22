using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace EventNext
{
    public class ActorCollection
    {

        public ActorCollection(Type type)
        {
            ServiceType = type;
        }

        private ConcurrentDictionary<string, ActorCollectionItem> mServices = new ConcurrentDictionary<string, ActorCollectionItem>();

        public ActorCollectionItem Get(string id)
        {
            mServices.TryGetValue(id, out ActorCollectionItem result);
            return result;
        }

        public ActorCollectionItem Set(string id, ActorCollectionItem service)
        {
            lock (this)
            {
                if (mServices.TryGetValue(id, out ActorCollectionItem result))
                {
                    return result;
                }
                else
                {
                    mServices[id] = service;
                    return service;
                }
            }
        }

        public Type ServiceType { get; private set; }

        private void OnFlush(EventCenter center, ActorCollectionItem item)
        {
            if (item.Actor is IActorState actorState)
            {
                try
                {
                    actorState.Flush();
                    if (center.EnabledLog(LogType.Debug))
                    {
                        center.Log(LogType.Debug, $"Free {item.ActorID}@{item.Interface.Name} actor flush success!");
                    }
                }
                catch (Exception e_)
                {
                    if (center.EnabledLog(LogType.Error))
                    {
                        center.Log(LogType.Error, $"Free {item.ActorID}@{item.Interface.Name} actor flush error {e_.Message}@{e_.StackTrace}!");
                    }
                }
            }
        }

        public void Flush(EventCenter center, string actor = null)
        {
            if (string.IsNullOrEmpty(actor))
            {
                foreach (var item in mServices.Values)
                {
                    OnFlush(center, item);
                }
            }
            else
            {
                if (mServices.TryGetValue(actor, out ActorCollectionItem item))
                {
                    OnFlush(center, item);
                }
            }
        }

        public void Free(EventCenter center, bool clearAll = false)
        {
            foreach (var item in mServices.Values)
            {
                if (EventCenter.Watch.ElapsedMilliseconds > item.TimeOut || clearAll)
                {
                    OnFlush(center, item);
                    mServices.TryRemove(item.ActorID, out ActorCollectionItem value);
                    if (center.EnabledLog(LogType.Debug))
                    {
                        center.Log(LogType.Debug, $"Free {item.ActorID}@{item.Interface.Name} success!");
                    }
                }
            }
        }

        public class ActorCollectionItem
        {
            public string ActorID { get; set; }

            public long TimeOut { get; set; }

            public object Actor { get; set; }

            public Type Interface { get; set; }

            public string ServiceName { get; internal set; }
        }

    }
}
