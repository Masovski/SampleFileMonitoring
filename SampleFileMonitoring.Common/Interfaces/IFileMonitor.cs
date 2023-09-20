using System;

namespace SampleFileMonitoring.Common.Interfaces
{
    /// <summary>
    /// Provides an interface for monitoring file system events. The actual scope of monitoring
    /// (e.g., single file, multiple directories, cloud folders) is determined by the implementing class.
    /// </summary>
    /// <remarks>
    /// This interface inherits from <see cref="IDisposable"/>. Make sure to dispose of it properly to release resources.
    /// </remarks>
    public interface IFileMonitor : IDisposable, IStartable, IStoppable
    {
        /// <summary>
        /// Occurs when a new file is created within the monitored scope.
        /// </summary>
        /// <remarks>
        /// The string parameter represents the path to the created file.
        /// </remarks>
        event EventHandler<string> FileCreated;

        /// <summary>
        /// Occurs when a file within the monitored scope is changed.
        /// </summary>
        /// <remarks>
        /// The string parameter represents the path to the changed file.
        /// </remarks>
        event EventHandler<string> FileChanged;

        /// <summary>
        /// Occurs when a file within the monitored scope is deleted.
        /// </summary>
        /// <remarks>
        /// The string parameter represents the path to the deleted file.
        /// </remarks>
        event EventHandler<string> FileDeleted;

        /// <summary>
        /// Occurs when a file within the monitored scope is renamed.
        /// </summary>
        /// <remarks>
        /// This event uses <see cref="FileRenamedArgs"/> to provide additional details about the rename operation.
        /// </remarks>
        event EventHandler<FileRenamedArgs> FileRenamed;

        /// <summary>
        /// Sets the path or identifier for the scope to be monitored.
        /// </summary>
        /// <param name="path">The path or identifier for the monitoring scope.</param>
        /// <remarks>
        /// Implementing classes should use this method to specify the scope that should be monitored.
        /// The interpretation of the 'path' parameter depends on the implementation, allowing for local folders, files, or cloud-based virtual folders.
        /// </remarks>
        void SetPath(string path);
    }
}