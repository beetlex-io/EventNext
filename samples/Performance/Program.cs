using EventNext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Performance
{
    class Program
    {
        private static EventCenter EventCenter = new EventCenter();

        private static IUserService henry;

        private static IUserService nb;

        private static int mCount;

        static void Main(string[] args)
        {
            EventCenter.Register(typeof(Program).Assembly);
            EventCenter.LogOutput += (o, e) =>
            {
                Console.WriteLine($"[{e.Type}]{e.Message}");
            };
            henry = EventCenter.Create<IUserService>("henry");
            nb = EventCenter.Create<IUserService>("nb");
            Test();
            Console.Read();
        }

        static async void Test()
        {
            double start = EventCenter.Watch.ElapsedMilliseconds;
            int count = 40;
            List<Task> tasks = new List<Task>();
            for (int k = 0; k < count; k++)
            {
                var task = Task.Run(async () =>
                {
                    for (int i = 0; i < 100000; i++)
                    {
                        var result = await henry.Income(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < 100000; i++)
                    {
                        var result = await henry.Payout(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


                task = Task.Run(async () =>
                {
                    for (int i = 0; i < 100000; i++)
                    {
                        var result = await nb.Income(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < 100000; i++)
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
            Console.WriteLine($"use time:{EventCenter.Watch.ElapsedMilliseconds - start}|count:{mCount}|henry:{henryAmount}|nb:{nbAmount}");
        }
    }

    [Service(typeof(IUserService))]
    public class UserService : ActorState,IUserService
    {
        private int mAmount;



        public Task<int> Amount()
        {
            return Task.FromResult(mAmount);
        }




        public Task<int> Income(int value)
        {
            mAmount += value;
            return Task.FromResult(mAmount);
        }



        public Task<int> Payout(int value)
        {
            mAmount -= value;
            return Task.FromResult(mAmount);
        }
    }

    public interface IUserService
    {


        Task<int> Income(int value);

        Task<int> Payout(int value);

        Task<int> Amount();

    }
}
