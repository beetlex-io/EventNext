using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventNext.Proxy
{
    public interface IAnyCompletionSource
    {
        void Success(object data);
        void Error(Exception error);
        Task GetTask();
    }

    public class AnyCompletionSource<T> : TaskCompletionSource<T>, IAnyCompletionSource
    {
        public void Success(object data)
        {
            TrySetResult((T)data);
        }

        public void Error(Exception error)
        {
            TrySetException(error);
        }

        public Task GetTask()
        {
            return this.Task;
        }
    }

}
