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
        static void Main(string[] args)
        {
            DBContext.SetConnectionDriver<SqliteDriver>(DB.DB1);
            DBContext.SetConnectionString(DB.DB1, "Data Source=Users.db;Pooling=true;FailIfMissing=false;");
            if ((User.name == "henry0").Count<User>() == 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    User user = new User();
                    user.Name = "henry" + i;
                    user.Amount = 0;
                    user.Save();

                }
            }
            else
            {
                for (int i = 0; i < 100; i++)
                    (User.name == "henry" + i).Edit<User>(u => u.Amount = 0);
            }
            EventCenter.Register(typeof(Program).Assembly);
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
            long time = EventCenter.Watch.ElapsedMilliseconds;
            List<Task> tasks = new List<Task>();
            for (int k = 0; k < 10; k++)
            {
                var task = Task.Run(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var service = EventCenter.Create<IUserService>("henry" + i);
                        service.Income(10);
                    }
                    return Task.CompletedTask;
                });
                tasks.Add(task);
                task = Task.Run(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var service = EventCenter.Create<IUserService>("henry" + i);
                        service.Pay(10);
                    }
                    return Task.CompletedTask;
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"actors use time:{EventCenter.Watch.ElapsedMilliseconds - time}");
            return Task.CompletedTask;
        }

        static Task defaultTest()
        {
            long time = EventCenter.Watch.ElapsedMilliseconds;
            List<Task> tasks = new List<Task>();
            for (int k = 0; k < 20; k++)
            {
                var task = Task.Run(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        DefaultUserService.Income("henry" + i, 10);
                    }
                    return Task.CompletedTask;
                });
                tasks.Add(task);
                task = Task.Run(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        DefaultUserService.Pay("henry" + i, 10);
                    }
                    return Task.CompletedTask;
                });
                tasks.Add(task);
            }
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine($"default use time:{EventCenter.Watch.ElapsedMilliseconds - time}");
            return Task.CompletedTask;
        }
    }
}
