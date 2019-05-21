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
        public string EventPath { get; set; }

        public EventCenter EventCenter { get; set; }

        public object Token { get; set; }

        public string ActorPath { get; set; }

        private User user;

        public void Flush()
        {
            user.Save();
        }

        public Task<long> GetAmount()
        {
            return Task.FromResult(user.Amount);
        }

        public void Init(string id)
        {
            user = (User.name == id).ListFirst<User>();
        }

        public async Task<long> Income(int amount)
        {
            await EventCenter.WriteEvent(this, null, null, new { History = user.Amount, Change = amount, Value = user.Amount + amount });
            user.Amount += amount;
            return user.Amount;
        }



        public async Task<long> Pay(int amount)
        {
            await EventCenter.WriteEvent(this, null, null, new { History = user.Amount, Change = -amount, Value = user.Amount - amount });
            user.Amount -= amount;
            return user.Amount;
        }
    }

    public interface IUserService
    {
        Task<long> Income(int amount);

        Task<long> Pay(int amount);

        Task<long> GetAmount();

    }

    public class SqliteEventStore : EventNext.IEventLogHandler
    {
        public Task<string> Write(IActorState actor, EventLog log)
        {
            EventStore store = new EventStore();
            store.EventID = log.EventID ?? Guid.NewGuid().ToString("N");
            store.ParentEventID = log.ParentEventID;
            store.DateTime = log.DateTime;
            store.EventPath = log.EventPath;
            store.ActorPath = log.ActorPath;
            store.Log = Newtonsoft.Json.JsonConvert.SerializeObject(log.Data);
            store.Save();
            return Task.FromResult(store.EventID);
        }
    }

}
