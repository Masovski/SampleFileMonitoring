namespace SampleFileMonitoring.Common.Interfaces
{
    /// <summary>
    /// Defines functionality for including subdirectories in monitoring operations.
    /// </summary>
    public interface ISupportSubdirectories
    {
        /// <summary>
        /// Configures the monitor to include subdirectories in its operations.
        /// </summary>
        void IncludeSubdirectories();
    }
}