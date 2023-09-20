namespace SampleFileMonitoring.Common.Interfaces
{
    /// <summary>
    /// Provides an interface for components that can be started.
    /// </summary>
    /// <remarks>
    /// Implementing classes should provide logic in the <see cref="Start"/> method to initialize and start the component's operation.
    /// </remarks>
    public interface IStartable
    {
        /// <summary>
        /// Starts the component, initializing and commencing its operation.
        /// </summary>
        /// <remarks>
        /// Place initialization logic and start-up routines in this method.
        /// </remarks>
        void Start();
    }
}