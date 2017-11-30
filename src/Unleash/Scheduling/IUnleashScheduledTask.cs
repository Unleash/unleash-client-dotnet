using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unleash.Scheduling
{
    /// <summary>
    /// A scheduled task that executes in the background at given intervals.
    /// </summary>
    public interface IUnleashScheduledTask
    {
        /// <summary>
        /// Executes the task
        /// </summary>
        /// <param name="cancellationToken">Cancellation token passed into the task.</param>
        /// <returns></returns>
        Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the name of the task
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the interval of which the task should be executed.
        /// </summary>
        TimeSpan Interval { get; }

        /// <summary>
        /// Gets a flag indicating that the task should run during startup.
        /// </summary>
        bool ExecuteDuringStartup { get; }
    }
}