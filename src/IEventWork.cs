using System;
using System.Threading.Tasks;

namespace EventNext
{
    public interface IEventWork : IDisposable
    {
        Task Execute();
    }
}
