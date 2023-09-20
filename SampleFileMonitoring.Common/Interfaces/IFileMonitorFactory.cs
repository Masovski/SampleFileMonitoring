namespace SampleFileMonitoring.Common.Interfaces
{
    /// <summary>
    /// Provides an interface for creating instances of <see cref="IFileMonitor"/>.
    /// </summary>
    /// <remarks>
    /// This factory allows for the creation of <see cref="IFileMonitor"/> instances, facilitating dependency injection and testing.
    /// </remarks>
    public interface IFileMonitorFactory
    {
        /// <summary>
        /// Creates a new instance of an <see cref="IFileMonitor"/>.
        /// </summary>
        /// <returns>A new instance of an object implementing <see cref="IFileMonitor"/>.</returns>
        /// <remarks>
        /// Use this method to obtain a new <see cref="IFileMonitor"/> instance.
        /// </remarks>
        IFileMonitor Create();
    }
}