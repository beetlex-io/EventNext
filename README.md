# EventNext
EventNext is logic interface design in event driven components  for .net core
#### 0.5.6
Optimize queues， 20% performance improvement in 500 concurrent
## Install
```
PM> Install-Package EventNext
```
## Define logic
``` csharp
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
```
## using
``` csharp
EventCenter.Register(typeof(Program).Assembly);
henry = EventCenter.Create<IUserService>("henry");
nb = EventCenter.Create<IUserService>("nb");
var result = await henry.Income(10);
result = await henry.Payout(10);
result = await nb.Income(10);
result = await nb.Payout(10);
```
## Performance (vs akka.net)
default setting `Environment:e3-1230v2 16g memory windows 2008r2 .netcore 2.15`
- Akka.net
```
    public class UserActor : ReceiveActor
    {
        public UserActor()
        {
            Receive<Income>(Income =>
            {
                mAmount += Income.Memory;
                this.Sender.Tell(mAmount);
            });
            Receive<Payout>(Outlay =>
            {
                mAmount -= Outlay.Memory;
                this.Sender.Tell(mAmount);
            });
            Receive<Get>(Outlay =>
            {
                this.Sender.Tell(mAmount);
            });
        }
        private decimal mAmount;
    }
    //invoke
    Income income = new Income { Memory = i };
    var result = await nbActor.Ask<decimal>(income);
    Payout payout = new Payout { Memory = i };
    var result = await nbActor.Ask<decimal>(payout);
```
- EventNext
```
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
    //invoke
    var result = await nb.Income(i);
    var result = await nb.Payout(i);
```
![](https://github.com/IKende/EventNext/blob/master/EventNext0.5.6.png?raw=true)
