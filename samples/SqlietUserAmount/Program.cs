using Peanut;
using System;
using EventNext;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SqlietUserAmount
{
    class Program
    {

        private static DefaultUserService DefaultUserService = new DefaultUserService();
        private static EventCenter EventCenter = new EventCenter();
        private static long mCount;
        private static int mConcurrent = 10;
        private static int mUsers = 100;
        static void Main(string[] args)
        {

            DBContext.SetConnectionDriver<SqliteDriver>(DB.DB1);
            DBContext.SetConnectionString(DB.DB1, "Data Source=Users.db;Pooling=true;FailIfMissing=false;Synchronous=Full");
            new Expression().Delete<EventStore>();
            new Expression().Delete<User>();
            if ((User.name == "henry0").Count<User>() == 0)
            {
                for (int i = 0; i < mUsers; i++)
                {
                    User user = new User();
                    user.Name = "henry" + i;
                    user.Amount = 0;
                    user.Save();
                }
            }
            else
            {
                for (int i = 0; i < mUsers; i++)
                    (User.name == "henry" + i).Edit<User>(u => u.Amount = 0);
            }
            EventCenter.Register(typeof(Program).Assembly);
            EventCenter.EventLogHandler = new SqliteEventStore();
            Test();
            Console.Read();

        }
        static async void Test()
        {
            await defaultTest();
            await actorTest();
        }

        static Task actorTest()
        {
            mCount = 0;
            long time = EventCenter.Watch.ElapsedMilliseconds;
            List<Task> tasks = new List<Task>();
            for (int k = 0; k < mConcurrent; k++)
            {
                var task = Task.Run(() =>
                {
                    for (int i = 0; i < mUsers; i++)
                    {
                        var service = EventCenter.Create<IUserService>("henry" + i);
                        service.Income(10);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                    return Task.CompletedTask;
                });
                tasks.Add(task);
                task = Task.Run(() =>
                {
                    for (int i = 0; i < mUsers; i++)
                    {
                        var service = EventCenter.Create<IUserService>("henry" + i);
                        service.Pay(10);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                    return Task.CompletedTask;
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            double useTime = (double)(EventCenter.Watch.ElapsedMilliseconds - time) / 1000d;
            Console.WriteLine($"Actors count:{mCount}|time:{useTime}s|rps:{(mCount / useTime):#####}");
            return Task.CompletedTask;
        }

        static Task defaultTest()
        {
            mCount = 0;
            long time = EventCenter.Watch.ElapsedMilliseconds;
            List<Task> tasks = new List<Task>();
            for (int k = 0; k < mConcurrent; k++)
            {
                var task = Task.Run(() =>
                {
                    for (int i = 0; i < mUsers; i++)
                    {
                        DefaultUserService.Income("henry" + i, 10);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                    return Task.CompletedTask;
                });
                tasks.Add(task);
                task = Task.Run(() =>
                {
                    for (int i = 0; i < mUsers; i++)
                    {
                        DefaultUserService.Pay("henry" + i, 10);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                    return Task.CompletedTask;
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            double useTime = (double)(EventCenter.Watch.ElapsedMilliseconds - time) / 1000d;
            Console.WriteLine($"Default count:{mCount}|time:{useTime}s|rps:{(mCount / useTime):#####}");
            return Task.CompletedTask;
        }
    }
}
