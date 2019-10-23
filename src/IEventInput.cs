using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public interface IEventInput
    {
        long ID { get; set; }

        string EventPath { get; set; }

        Dictionary<string, string> Properties { get; set; }

        object[] Data { get; set; }

        object Token { get; set; }
    }

    public class EventInput : IEventInput
    {

        public long ID { get; set; }

        public string EventPath { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public object[] Data { get; set; }

        public object Token { get; set; } = "Localhost";
    }
}
