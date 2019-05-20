using System;
using System.Collections.Generic;
using System.Text;

namespace EventNext
{
    public interface IActorState
    {
        string Path { get; set; }

        void Init(string id);

        object Token { get; set; }

        EventCenter EventCenter { get; set; }

        void Flush();
    }
}
