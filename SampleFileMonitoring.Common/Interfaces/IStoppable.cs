namespace SampleFileMonitoring.Common.Interfaces
{
    /// <summary>
    /// Provides an interface for components that can be stopped.
    /// </summary>
    /// <remarks>
    /// Implementing classes should provide logic in the <see cref="Stop"/> method to halt the component's operation and release any necessary resources.
    /// </remarks>
    public interface IStoppable
    {
        /// <summary>
        /// Stops the component, ceasing its operation.
        /// </summary>
        /// <remarks>
        /// Place clean-up logic and shutdown routines in this method.
        /// </remarks>
        void Stop();
    }
}