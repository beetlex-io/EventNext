using System;
using System.Collections.Generic;
using System.Text;
using Peanut.Mappings;
namespace SqlietUserAmount
{
    [Table]
    public interface IUser
    {
        [ID]
        string Name { get; set; }
        [Column]
        long Amount { get; set; }
    }
    [Table]
    public interface IEventStore
    {
        [ID]
        string EventID { get; set; }
        [Column]
        string ParentEventID { get; set; }
        [Column]
        string ActorPath { get; set; }
        [Column]
        string EventPath { get; set; }
        [Column]
        [DateTimeConvter]
        DateTime DateTime { get; set; }
        [Column]
        string Log { get; set; }

    }

}
