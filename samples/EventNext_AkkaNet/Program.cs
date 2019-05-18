using Akka.Actor;
using EventNext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventNext_AkkaNet
{
    class Program
    {
        private static EventCenter EventCenter = new EventCenter();

        private static ActorSystem Akka;

        private static int mCount;

        private static int mRequests = 100;

        private static int mFors = 10000;

        static void Main(string[] args)
        {

            EventCenter.Register(typeof(Program).Assembly);
            EventCenter.LogOutput += (o, e) =>
            {
                Console.WriteLine($"[{e.Type}]{e.Message}");
            };
            Akka = ActorSystem.Create("MySystem");
            Test();
            Console.Read();
        }

        static async void Test()
        {
            await AkkaTest();
            await EventNextTest();
        }

        static async Task AkkaTest()
        {
            mCount = 0;
            var henry = Akka.ActorOf<UserActor>("henry");
            var nb = Akka.ActorOf<UserActor>("nb");
            double start = EventCenter.Watch.ElapsedMilliseconds;

            List<Task> tasks = new List<Task>();
            for (int k = 0; k < mRequests; k++)
            {
                var task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        Income income = new Income { Memory = i };
                        var result = await henry.Ask<decimal>(income);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        Payout payout = new Payout { Memory = i };
                        var result = await henry.Ask<decimal>(payout);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        Income income = new Income { Memory = i };
                        var result = await nb.Ask<decimal>(income);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        Payout payout = new Payout { Memory = i };
                        var result = await nb.Ask<decimal>(payout);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


            }
            Task.WaitAll(tasks.ToArray());
            var get = new Get();
            var henryAmount = await henry.Ask<decimal>(get);
            var nbAmount = await nb.Ask<decimal>(get);
            Console.WriteLine($"Akka use time:{EventCenter.Watch.ElapsedMilliseconds - start}|count:{mCount}|henry:{henryAmount}|nb:{nbAmount}");
        }

        static async Task EventNextTest()
        {
            mCount = 0;
            IUserService henry = EventCenter.Create<IUserService>("henry"); ;
            IUserService nb = EventCenter.Create<IUserService>("nb"); ;
            double start = EventCenter.Watch.ElapsedMilliseconds;
            List<Task> tasks = new List<Task>();
            for (int k = 0; k < mRequests; k++)
            {
                var task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        var result = await henry.Income(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        var result = await henry.Payout(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        var result = await nb.Income(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        var result = await nb.Payout(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


            }
            Task.WaitAll(tasks.ToArray());
            var henryAmount = await henry.Amount();
            var nbAmount = await nb.Amount();
            Console.WriteLine($"EventNext use time:{EventCenter.Watch.ElapsedMilliseconds - start}|count:{mCount}|henry:{henryAmount}|nb:{nbAmount}");
        }
    }
}
