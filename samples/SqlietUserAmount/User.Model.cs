using System;
using System.Collections.Generic;
using System.Text;
using Peanut.Mappings;

namespace SqlietUserAmount
{
    ///<summary>
    ///Peanut Generator Copyright @ henryfan 2018 email:henryfan@msn.com
    ///website:http://www.ikende.com
    ///</summary>
    [Table()]
    public partial class User : Peanut.Mappings.DataObject
    {
        private string mName;
        public static Peanut.FieldInfo<string> name = new Peanut.FieldInfo<string>("User", "Name");
        private long mAmount;
        public static Peanut.FieldInfo<long> amount = new Peanut.FieldInfo<long>("User", "Amount");
        ///<summary>
        ///Type:string
        ///</summary>
        [ID()]
        public virtual string Name
        {
            get
            {
                return mName;
                
            }
            set
            {
                mName = value;
                EntityState.FieldChange("Name");
                
            }
            
        }
        ///<summary>
        ///Type:long
        ///</summary>
        [Column()]
        public virtual long Amount
        {
            get
            {
                return mAmount;
                
            }
            set
            {
                mAmount = value;
                EntityState.FieldChange("Amount");
                
            }
            
        }
        
    }
    ///<summary>
    ///Peanut Generator Copyright @ henryfan 2018 email:henryfan@msn.com
    ///website:http://www.ikende.com
    ///</summary>
    [Table()]
    public partial class EventStore : Peanut.Mappings.DataObject
    {
        private string mEventID;
        public static Peanut.FieldInfo<string> eventID = new Peanut.FieldInfo<string>("EventStore", "EventID");
        private string mParentEventID;
        public static Peanut.FieldInfo<string> parentEventID = new Peanut.FieldInfo<string>("EventStore", "ParentEventID");
        private string mActorPath;
        public static Peanut.FieldInfo<string> actorPath = new Peanut.FieldInfo<string>("EventStore", "ActorPath");
        private string mEventPath;
        public static Peanut.FieldInfo<string> eventPath = new Peanut.FieldInfo<string>("EventStore", "EventPath");
        private DateTime mDateTime;
        public static Peanut.FieldInfo<DateTime> dateTime = new Peanut.FieldInfo<DateTime>("EventStore", "DateTime");
        private string mLog;
        public static Peanut.FieldInfo<string> log = new Peanut.FieldInfo<string>("EventStore", "Log");
        ///<summary>
        ///Type:string
        ///</summary>
        [ID()]
        public virtual string EventID
        {
            get
            {
                return mEventID;
                
            }
            set
            {
                mEventID = value;
                EntityState.FieldChange("EventID");
                
            }
            
        }
        ///<summary>
        ///Type:string
        ///</summary>
        [Column()]
        public virtual string ParentEventID
        {
            get
            {
                return mParentEventID;
                
            }
            set
            {
                mParentEventID = value;
                EntityState.FieldChange("ParentEventID");
                
            }
            
        }
        ///<summary>
        ///Type:string
        ///</summary>
        [Column()]
        public virtual string ActorPath
        {
            get
            {
                return mActorPath;
                
            }
            set
            {
                mActorPath = value;
                EntityState.FieldChange("ActorPath");
                
            }
            
        }
        ///<summary>
        ///Type:string
        ///</summary>
        [Column()]
        public virtual string EventPath
        {
            get
            {
                return mEventPath;
                
            }
            set
            {
                mEventPath = value;
                EntityState.FieldChange("EventPath");
                
            }
            
        }
        ///<summary>
        ///Type:DateTime
        ///</summary>
        [Column()]
        [DateTimeConvter]
        public virtual DateTime DateTime
        {
            get
            {
                return mDateTime;
                
            }
            set
            {
                mDateTime = value;
                EntityState.FieldChange("DateTime");
                
            }
            
        }
        ///<summary>
        ///Type:string
        ///</summary>
        [Column()]
        public virtual string Log
        {
            get
            {
                return mLog;
                
            }
            set
            {
                mLog = value;
                EntityState.FieldChange("Log");
                
            }
            
        }
        
    }
    
}
