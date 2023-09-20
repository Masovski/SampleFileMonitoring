using System;
using System.Threading.Tasks;

namespace SampleFileMonitoring.Common.Interfaces
{
    /// <summary>
    /// An interface for creating new monitoring agents
    /// </summary>
    public interface IMonitoringAgent : IDisposable
    {
        /// <summary>
        /// Runs the agent with default settings asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RunAsync();

        /// <summary>
        /// Runs the agent with a specified path asynchronously.
        /// </summary>
        /// <param name="path">The path where the agent operates.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task RunAsync(string path);
    }
}