using EventNext;
using System;
using System.Threading.Tasks;

namespace BaseUsing
{
    class Program
    {
        private static EventCenter EventCenter = new EventCenter();

        private static IUserService henry;

        private static IUserService nb;

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
            var result = await henry.Income(10);
            Console.WriteLine(result);
            result = await henry.Payout(10);
            Console.WriteLine(result);
            result = await nb.Income(10);
            Console.WriteLine(result);
            result = await nb.Payout(10);
            Console.WriteLine(result);
        }
    }

    public interface IUserService
    {
        Task<int> Income(int value);

        Task<int> Payout(int value);

        Task<int> Amount();
    }

    [Service(typeof(IUserService))]
    public class UserService : IUserService
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


}
