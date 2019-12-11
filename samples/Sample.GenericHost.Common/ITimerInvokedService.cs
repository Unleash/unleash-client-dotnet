using System.Threading;
using System.Threading.Tasks;

namespace Sample.GenericHost
{
    public interface ITimerInvokedService
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }

    public interface ITimerInvokedService<out TState> : ITimerInvokedService
        where TState : class
    {
        TState State { get; }
    }
}
