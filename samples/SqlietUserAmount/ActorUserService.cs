using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventNext;
namespace SqlietUserAmount
{
    [Service(typeof(IUserService))]
    public class ActorUserService : IUserService, IActorState
    {
        public string Path { get; set; }

        public EventCenter EventCenter { get; set; }
        public object Token { get; set; }

        private User user;

        public void Flush()
        {
            user.Save();
        }

        public Task<long> GetAmount()
        {
            return Task.FromResult(user.Amount);
        }

        public Task<long> Income(int amount)
        {
            user.Amount += amount;
            return Task.FromResult(user.Amount);
        }

        public void Init(string id)
        {
            user = (User.name == id).ListFirst<User>();
        }

        public Task<long> Pay(int amount)
        {
            user.Amount -= amount;
            return Task.FromResult(user.Amount);
        }
    }

    public interface IUserService
    {
        Task<long> Income(int amount);

        Task<long> Pay(int amount);

        Task<long> GetAmount();

    }

}
