using EventNext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventNext_AkkaNet
{
    [Service(typeof(IUserService))]
    public class UserService : IUserService, IActorState
    {
        private int mAmount;

        public string Path { get; set; }

        public EventCenter EventCenter { get; set; }

        public Task<int> Amount()
        {
            return Task.FromResult(mAmount);
        }

        public void Flush()
        {

        }

        public Task<int> Income(int value)
        {
            mAmount += value;
            return Task.FromResult(mAmount);
        }

        public void Init()
        {
            Console.WriteLine($"{Path} init");
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
