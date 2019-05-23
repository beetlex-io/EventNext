using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public interface IController
    {
        void Initialize(EventCenter center);
    }

    public interface IActorState
    {
        string EventPath { get; set; }

        string ActorPath { get; set; }

        object Token { get; set; }

        EventCenter EventCenter { get; set; }

        void ActorInit(string id);

        void ActorFlush();

    }

    public abstract class ActorState : IActorState
    {
        public string EventPath { get; set; }

        public string ActorPath { get; set; }

        public object Token { get; set; }

        public EventCenter EventCenter { get; set; }

        public virtual void ActorFlush()
        {

        }

        public virtual void ActorInit(string id)
        {

        }
    }

}
