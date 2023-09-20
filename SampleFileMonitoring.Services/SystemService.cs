using System.Collections.Concurrent;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using SampleFileMonitoring.Common;
using SampleFileMonitoring.Common.Interfaces.Services;

namespace SampleFileMonitoring.Services
{
    public partial class SystemService : ISystemService
    {
        private readonly int _concurrencyDelayMultiplier;
        private readonly int _maxDegreeOfParallelism;
        private readonly int _fileQueueDelayMultipler;
        private readonly int _maxConcurrentIORequests;

        private readonly ILogger<SystemService> _logger;

        public SystemService(ILogger<SystemService> logger, SystemPerformanceSettings performanceSettings)
        {
            _maxDegreeOfParallelism = performanceSettings.MaxDegreeOfParallelism;
            _maxConcurrentIORequests = performanceSettings.MaxConcurrentIORequests;
            _concurrencyDelayMultiplier = performanceSettings.ConcurrencyDelayMultiplier;
            _fileQueueDelayMultipler = _maxDegreeOfParallelism * _concurrencyDelayMultiplier * 3;
            this._logger = logger;
        }

        public async Task<FileDetails> GetFileDetailsAsync(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            
            return new()
            {
                FileName = fileInfo.Name,
                FileType = fileInfo.Extension,
                Location = fileInfo.DirectoryName,
                Hash = await ComputeFileHashAsync(filePath),
                CreatedAtUtc = $"{fileInfo.CreationTimeUtc:dd/MM/yyyy HH:mm:ss}",
                ModifiedDateUtc = $"{fileInfo.LastWriteTimeUtc:dd/MM/yyyy HH:mm:ss}",
                FileSize = fileInfo.Length
            };
        }

        public async Task<SystemDetails> GetSystemDetailsAsync()
        {
            return await Task.Run(() => new SystemDetails()
            {
                LogonUsername = GetUsername(),
                Domain = GetDomainNameOrComputerName(),
                IPv4Addresses = string.Join(", ", GetLocalIPv4Addresses()),
                OperatingSystemName = GetOSDescription()
            });
        }

        public async Task FindFilesAsync(
            string rootPath,
            bool includeSubdirectories,
            IEnumerable<string> allowedExtensions,
            Func<IEnumerable<string>, Task> batchProcessor,
            int batchSize)
        {
            var normalizedAllowedExtensions = allowedExtensions.Select(e => e.ToLowerInvariant());
            string formattedExtensionList = normalizedAllowedExtensions.Any() ? string.Join(", ", normalizedAllowedExtensions) : "ALL";
            _logger.LogDebug($"Starting file searching in root directory: {rootPath};");
            _logger.LogDebug($"Recursive: {includeSubdirectories};");
            _logger.LogDebug($"Looking for files with these extensions: {formattedExtensionList}");

            var ioLimiter = new SemaphoreSlim(_maxConcurrentIORequests);
            var blockingCollection = new BlockingCollection<string>();

            // Producer Task: Gather files in parallel
            var producerTask = SearchDirectoryAsync(rootPath, includeSubdirectories, normalizedAllowedExtensions, ioLimiter, blockingCollection, batchSize);

            // Consumer Task: Process files in batches
            var consumerTask = ProcessFilesInBatchAsync(blockingCollection, batchProcessor, batchSize);

            await producerTask;
            blockingCollection.CompleteAdding();
            await consumerTask;
        }

        /// <summary>
        /// Searches a given directory and its subdirectories for files that match the specified extensions, 
        /// and adds them to a blocking collection for further processing.
        /// </summary>
        /// <param name="dir">The directory path to begin the search.</param>
        /// <param name="includeSubdirectories">Whether to include subdirectories in the search.</param>
        /// <param name="normalizedAllowedExtensions">A collection of allowed file extensions to filter the search.</param>
        /// <param name="ioLimiter">A semaphore to limit the number of concurrent I/O operations.</param>
        /// <param name="blockingCollection">The collection to which the found files are added.</param>
        /// <param name="batchSize">The number of files to be processed in each batch.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method employs a <see cref="System.Threading.SemaphoreSlim"/> to throttle the I/O operations and 
        /// ensures that a limited number of files are processed concurrently.
        /// Any errors encountered during the search are logged using the `_logger`.
        /// </remarks>
        private async Task SearchDirectoryAsync(
            string dir,
            bool includeSubdirectories,
            IEnumerable<string> normalizedAllowedExtensions,
            SemaphoreSlim ioLimiter,
            BlockingCollection<string> blockingCollection,
            int batchSize)
        {
            await ioLimiter.WaitAsync();

            try
            {
                var fileCount = await AddMatchingFiles(dir, normalizedAllowedExtensions, blockingCollection, batchSize, _fileQueueDelayMultipler * batchSize);
                if (fileCount > 0)
                    _logger.LogDebug($"Found {fileCount} files in {dir}.");

                ioLimiter.Release();

                await Task.Delay(_maxConcurrentIORequests * _concurrencyDelayMultiplier);

                if (includeSubdirectories)
                {
                    var directories = Directory.EnumerateDirectories(dir);

                    var tasks = directories.Select(subDir => SearchDirectoryAsync(subDir, includeSubdirectories, normalizedAllowedExtensions, ioLimiter, blockingCollection, batchSize));
                    await Task.WhenAll(tasks);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex.Message);
            }
            catch (Exception ex)
            {
                ioLimiter.Release();
                _logger.LogError(ex, string.Empty);
            }
        }
    }
}