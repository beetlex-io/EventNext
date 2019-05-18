using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public interface IEventOutput
    {
        long ID { get; set; }

        string EventPath { get; set; }

        Dictionary<string, string> Properties { get; set; }

        object[] Data { get; set; }

        EventError EventError { get; set; }

        double ResponseTime { get; }
    }

    public class EventOutput : IEventOutput
    {
        public long ID { get; set; }

        public string EventPath { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public object[] Data { get; set; }

        public EventError EventError { get; set; }

        public double ResponseTime { get; internal set; }
    }

}
