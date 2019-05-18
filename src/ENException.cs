using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public class ENException : Exception
    {
        public ENException()
        {
        }

        public EventError EventError { get; set; }

        public ENException(string message) : base(message) { }

        public ENException(string message, params object[] parameters) : base(string.Format(message, parameters)) { }

        public ENException(string message, Exception baseError) : base(message, baseError) { }

        public ENException(Exception baseError, string message, params object[] parameters) : base(string.Format(message, parameters), baseError) { }
    }
}
