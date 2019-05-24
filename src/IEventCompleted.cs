using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace EventNext
{
    public interface IEventCompleted
    {
        void Completed(IEventOutput data);

    }

    public class EventCompleted : IEventCompleted
    {
        public void Completed(IEventOutput data)
        {
            mTaskCompletionSource.TrySetResult(data);
        }

        private TaskCompletionSource<IEventOutput> mTaskCompletionSource = new TaskCompletionSource<IEventOutput>();

        public Task<IEventOutput> GetTask()
        {
            return mTaskCompletionSource.Task;
        }
    }

}
