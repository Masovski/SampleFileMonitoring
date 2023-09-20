using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleFileMonitoring.Common.Interfaces.Services
{
    /// <summary>
    /// Provides various system-related services.
    /// </summary>
    public interface ISystemService
    {
        /// <summary>
        /// Retrieves detailed information about the system asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains the <see cref="SystemDetails"/> of the system.</returns>
        Task<SystemDetails> GetSystemDetailsAsync();

        /// <summary>
        /// Retrieves detailed information about a specified file asynchronously.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
        /// The task result contains the <see cref="FileDetails"/> of the file.</returns>
        Task<FileDetails> GetFileDetailsAsync(string filePath);

        /// <summary>
        /// Finds files under a given root path asynchronously and processes them in batches.
        /// </summary>
        /// <param name="rootPath">The root directory where the file search starts.</param>
        /// <param name="includeSubdirectories">Whether to include files in subdirectories.</param>
        /// <param name="allowedExtensions">List of allowed file extensions.</param>
        /// <param name="batchProcessor">A function that processes a batch of found files. Takes an IEnumerable of file paths as an argument.</param>
        /// <param name="batchSize">The size of each batch to process.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task FindFilesAsync(string rootPath, bool includeSubdirectories, IEnumerable<string> allowedExtensions, Func<IEnumerable<string>, Task> batchProcessor, int batchSize);
    }
}