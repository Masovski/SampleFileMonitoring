using System.Collections.Generic;

namespace SampleFileMonitoring.Common.Interfaces.Services
{
    /// <summary>
    /// Defines an interface for a service that monitors file changes in folders.
    /// </summary>
    /// <remarks>
    /// Provides methods for creating instances of <see cref="IFileMonitor"/> to monitor specific folders, with optional parameters to enable recursive monitoring of subdirectories and to filter by file extensions.
    /// </remarks>
    public interface IFileMonitoringService
    {
        /// <summary>
        /// Monitors files in the specified folder.
        /// </summary>
        /// <param name="folderPath">The path of the folder to monitor.</param>
        /// <returns>An instance of <see cref="IFileMonitor"/> set up to monitor the specified folder.</returns>
        IFileMonitor MonitorFolderFiles(string folderPath);

        /// <summary>
        /// Monitors files in the specified folder, filtering by allowed extensions.
        /// </summary>
        /// <param name="folderPath">The path of the folder to monitor.</param>
        /// <param name="allowedExtensions">The file extensions to monitor.</param>
        /// <returns>An instance of <see cref="IFileMonitor"/> set up to monitor the specified folder and filter by allowed extensions.</returns>
        IFileMonitor MonitorFolderFiles(string folderPath, IEnumerable<string> allowedExtensions);

        /// <summary>
        /// Monitors files in the specified folder, with an option for including subdirectories.
        /// </summary>
        /// <param name="folderPath">The path of the folder to monitor.</param>
        /// <param name="recursive">Whether to include subdirectories in monitoring.</param>
        /// <returns>An instance of <see cref="IFileMonitor"/> set up to monitor the specified folder and optionally its subdirectories.</returns>
        IFileMonitor MonitorFolderFiles(string folderPath, bool recursive);

        /// <summary>
        /// Monitors files in the specified folder, with options for including subdirectories and filtering by allowed extensions.
        /// </summary>
        /// <param name="folderPath">The path of the folder to monitor.</param>
        /// <param name="recursive">Whether to include subdirectories in monitoring.</param>
        /// <param name="allowedExtensions">The file extensions to monitor.</param>
        /// <returns>An instance of <see cref="IFileMonitor"/> set up to monitor the specified folder, optionally its subdirectories, and filter by allowed extensions.</returns>
        IFileMonitor MonitorFolderFiles(string folderPath, bool recursive, IEnumerable<string> allowedExtensions);
    }
}
