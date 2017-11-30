using System;
using System.Collections.Generic;
using System.Threading;

namespace Unleash.Scheduling
{
    /// <inheritdoc />
    /// <summary>
    /// Task manager for scheduling tasks on a background thread. 
    /// </summary>
    public interface IUnleashScheduledTaskManager : IDisposable
    {
        /// <summary>
        /// Configures a set of tasks to execute in the background.
        /// </summary>
        /// <param name="tasks">Tasks to be executed</param>
        /// <param name="cancellationToken">Cancellation token which will be passed during shutdown (Dispose).</param>
        void Configure(IEnumerable<IUnleashScheduledTask> tasks, CancellationToken cancellationToken);
    }
}