using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public interface IActorState
    {
        string EventPath { get; set; }

        string ActorPath { get; set; }

        void Init(string id);

        object Token { get; set; }

        EventCenter EventCenter { get; set; }

        void Flush();
    }

    public abstract class ActorState : IActorState
    {
        public string EventPath { get; set; }

        public string ActorPath { get; set; }

        public object Token { get; set; }

        public EventCenter EventCenter { get; set; }

        public virtual void Flush()
        {

        }

        public virtual void Init(string id)
        {

        }
    }

}
