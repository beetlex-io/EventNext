using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.EventNext
{
    public class OutputAwaiterFactory
    {
        const int COUNT = 1024 * 1024;

        public OutputAwaiterFactory(int startid = 1, int end = 100000000)
        {
            mID = startid;
            mStartID = startid;
            mEndID = end;
            mTimer = new System.Threading.Timer(OnTimeout, null, 1000, 1000);

        }

        private NextQueueGroup NextQueueGroup = new NextQueueGroup();

        private System.Threading.Timer mTimer;

        private int mID;

        private int mStartID;

        private int mEndID;

        private AwaiterGroup mAwaiterItemGroup = new AwaiterGroup();

        private void OnTimeout(object state)
        {
            try
            {
                mTimer.Change(-1, -1);
                long timeout = EventCenter.Watch.ElapsedMilliseconds;
                var items = mAwaiterItemGroup.GetTimeouts(timeout);
                if (items.Count > 0)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        TimeoutWork tw = new TimeoutWork();
                        tw.Awaiter = items[i];
                        EventOutput output = new EventOutput();
                        output.ID = tw.Awaiter.ID;
                        NextQueueGroup.Enqueue(tw);
                    }

                }
            }
            catch
            {

            }
            finally
            {
                mTimer.Change(1000, 1000);
            }
        }

        internal OutputAwaiter GetItem(int id)
        {
            return mAwaiterItemGroup.Get(id);
        }

        public (int, TaskCompletionSource<EventOutput>) Create(EventInput input, Type[] resultType, int timeout = 1000 * 100)
        {
            int id = 0;
            long expiredTime;
            lock (this)
            {
                mID++;
                if (mID >= mEndID)
                    mID = mStartID;
                id = mID;

            }
            expiredTime = EventCenter.Watch.ElapsedMilliseconds + timeout;
            var item = new OutputAwaiter();
            item.ID = id;
            mAwaiterItemGroup.Set(item.ID, item);
            return (id, item.Create(expiredTime));
        }

        public bool Completed(OutputAwaiter item, EventOutput data)
        {
            if (item.Completed(data))
            {
                return true;
            }
            return false;
        }

        public class AwaiterGroup
        {
            public AwaiterGroup()
            {
                for (int i = 0; i < Groups; i++)
                {
                    mGroups.Add(new GroupItem());
                }
            }

            const int Groups = 10;

            private List<GroupItem> mGroups = new List<GroupItem>();

            public void Set(int id, OutputAwaiter item)
            {
                mGroups[id % Groups].Set(id, item);
            }

            public OutputAwaiter Get(int id)
            {
                return mGroups[id % Groups].Get(id);
            }

            public IList<OutputAwaiter> GetTimeouts(double time)
            {
                List<OutputAwaiter> items = new List<OutputAwaiter>();
                for (int i = 0; i < Groups; i++)
                {
                    mGroups[i].GetTimeouts(items, time);
                }
                return items;
            }


            public class GroupItem
            {
                private ConcurrentDictionary<int, OutputAwaiter> mItems = new ConcurrentDictionary<int, OutputAwaiter>();

                public void Set(int id, OutputAwaiter item)
                {
                    mItems[id] = item;
                }

                public OutputAwaiter Get(int id)
                {
                    mItems.TryRemove(id, out OutputAwaiter item);
                    return item;
                }

                public void GetTimeouts(List<OutputAwaiter> items, double time)
                {
                    foreach (var item in mItems.Values)
                    {
                        if (time > item.TimeOut)
                        {
                            items.Add(Get(item.ID));
                        }
                    }
                }
            }
        }
    }

    class TimeoutWork : IEventWork
    {
        public OutputAwaiter Awaiter { get; set; }

        public EventOutput EventOutput { get; set; }

        public OutputAwaiterFactory OutputAwaiterFactory { get; set; }

        public void Dispose()
        {
            Awaiter = null;
            EventOutput = null;
        }

        public Task Execute()
        {
            OutputAwaiterFactory.Completed(Awaiter, EventOutput);
            return Task.CompletedTask;
        }
    }



    public class OutputAwaiter
    {
        public OutputAwaiter()
        {

        }

        private TaskCompletionSource<EventOutput> completionSource;

        public int ID { get; set; }

        public double TimeOut { get; set; }

        private int mFree = 0;

        public TaskCompletionSource<EventOutput> Create(long expiredTime)
        {
            TimeOut = expiredTime;
            completionSource = new TaskCompletionSource<EventOutput>();
            return completionSource;
        }

        public bool Completed(EventOutput data)
        {
            if (System.Threading.Interlocked.CompareExchange(ref mFree, 1, 0) == 0)
            {
                completionSource.TrySetResult(data);
                return true;
            }
            return false;
        }

        public EventInput Input { get; set; }

    }
}
