using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventNext
{
    public interface IEventStoreHandler
    {
        Task Initialize(EventCenter eventCenter);

        Task<string> Write(IActorState actor, EventStore log);

        Task<EventStore> GetByEventID(IActorState actor, string eventid);
        //
        Task<EventStore> GetByParentEventID(IActorState actor, string parentEventid);

        Task<IList<EventStore>> ListByParentEventID(IActorState actor, string parentEventid);

        //用于获取最后事件，确保数据一致性
        Task<EventStore> GetByTypeLast(IActorState actor, string type);
    }

}
