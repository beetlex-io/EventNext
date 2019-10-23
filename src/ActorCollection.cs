using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EventNext
{
    public class ActorCollection
    {

        public ActorCollection(Type type, Type interfaceType, string serviceName)
        {
            ServiceType = type;
            InterfaceType = interfaceType;
            ServiceName = serviceName;
        }

        private ConcurrentDictionary<string, ActorItem> mServices = new ConcurrentDictionary<string, ActorItem>();

        public ActorItem Get(string id)
        {
            mServices.TryGetValue(id, out ActorItem result);
            return result;
        }



        public ActorItem Set(string id, ActorItem service, out bool add)
        {
            lock (this)
            {
                if (mServices.TryGetValue(id, out ActorItem result))
                {
                    add = false;
                    return result;
                }
                else
                {
                    add = true;
                    mServices[id] = service;
                    return service;
                }
            }
        }

        public string ServiceName { get; private set; }

        public Type InterfaceType { get; private set; }

        public Type ServiceType { get; private set; }

        private async void OnFlush(EventCenter center, ActorItem item)
        {
            if (item.Actor is IActorState actorState)
            {
                try
                {
                    await actorState.ActorFlush();
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
                if (mServices.TryGetValue(actor, out ActorItem item))
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
                    mServices.TryRemove(item.ActorID, out ActorItem value);
                    if (center.EnabledLog(LogType.Debug))
                    {
                        center.Log(LogType.Debug, $"Free {item.ActorID}@{item.Interface.Name} success!");
                    }
                }
            }
        }

        public class ActorItem
        {

            public ActorItem()
            {
                CreateIDTag();
            }

            public string ActorID { get; set; }

            public long TimeOut { get; set; }

            public NextQueue NextQueue { get; set; } = new NextQueue();

            public object Actor { get; set; }

            public Type Interface { get; set; }

            public string ServiceName { get; internal set; }

            const int MAX_ID = 10000000;

            private long mTagID;

            private long mID = 0;

            private bool mInitialized = false;

            private void CreateIDTag()
            {
                DateTime dateTime = DateTime.Parse("2010-1-1");
                var ts = DateTime.Now - dateTime;
                mTagID = (int)ts.TotalSeconds;
            }

            public async Task Initialize()
            {
                if (!mInitialized)
                {
                    System.Threading.Monitor.Enter(Actor);
                    try
                    {
                        if (!mInitialized)
                        {
                            if (Actor is IActorState actorState)
                                await actorState.ActorInit(ActorID);
                        }
                    }
                    finally
                    {
                        mInitialized = true;
                        System.Threading.Monitor.Exit(Actor);
                    }
                }
            }

            public long GetSequence()
            {
                lock (this)
                {
                    mID++;
                    if (mID >= MAX_ID)
                    {
                        CreateIDTag();
                        mID = 1;
                    }
                    return mTagID << 24 | mID;
                }
            }

        }

    }
}
