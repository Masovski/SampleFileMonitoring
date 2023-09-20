using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace SampleFileMonitoring.Services
{
    public partial class SystemService
    {
        /// <summary>
        /// Retrieves the username of the currently logged-in user.
        /// </summary>
        /// <returns>A string containing the username of the currently logged-in user.</returns>
        /// <remarks>
        /// This method uses the <see cref="System.Environment.UserName"/> property to fetch the username.
        /// </remarks>
        private static string GetUsername()
        {
            return Environment.UserName;
        }

        /// <summary>
        /// Computes the SHA-256 hash of a given file asynchronously.
        /// </summary>
        /// <param name="filePath">The full path of the file for which the hash is to be computed.</param>
        /// <returns>A <see cref="Task{string}"/> representing the result of the asynchronous operation, containing the computed SHA-256 hash as a lowercase hexadecimal string.</returns>
        /// <remarks>
        /// This method reads the file located at the given path and computes its SHA-256 hash.
        /// The hash is returned as a lowercase hexadecimal string.
        /// It uses <see cref="System.Security.Cryptography.SHA256"/> for the hash computation.
        /// </remarks>
        private static async Task<string> ComputeFileHashAsync(string filePath)
        {
            using var hashAlgorithm = System.Security.Cryptography.SHA256.Create();

            using FileStream fileStream = new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            byte[] hash = await hashAlgorithm.ComputeHashAsync(fileStream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Retrieves the domain name or the computer name of the current machine.
        /// </summary>
        /// <returns>A string containing either the domain name or the computer name, depending on which is applicable.</returns>
        /// <remarks>
        /// This method uses the <see cref="System.Environment.UserDomainName"/> and <see cref="System.Environment.MachineName"/> properties to fetch the domain and computer names.
        /// If the domain name and computer name are the same, the computer name is returned; otherwise, the domain name is returned.
        /// </remarks>
        private static string GetDomainNameOrComputerName()
        {
            string domainName = Environment.UserDomainName;
            string computerName = Environment.MachineName;

            return string.Equals(domainName, computerName) ? computerName : domainName;
        }

        /// <summary>
        /// Retrieves the local IPv4 addresses of the current machine.
        /// </summary>
        /// <returns>An enumerable of strings, each representing an IPv4 address associated with the local machine.</returns>
        /// <remarks>
        /// This method fetches all IP addresses associated with the local machine, filters them to only include IPv4 addresses, and returns them as a collection of strings.
        /// It uses the <see cref="System.Net.Dns.GetHostAddresses"/> and <see cref="System.Net.Dns.GetHostName"/> methods to fetch the IP addresses.
        /// </remarks>
        private static IEnumerable<string> GetLocalIPv4Addresses()
        {
            return Dns.GetHostAddresses(Dns.GetHostName())
            .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
            .Select(address => address.ToString());
        }

        /// <summary>
        /// Gets the operating system description of the current machine.
        /// </summary>
        /// <returns>A string containing the description of the operating system.</returns>
        /// <remarks>
        /// This method uses the <see cref="System.Runtime.InteropServices.RuntimeInformation.OSDescription"/> property to get the OS description.
        /// </remarks>
        private static string GetOSDescription()
        {
            return RuntimeInformation.OSDescription;
        }

        /// <summary>
        /// Asynchronously adds files from a specified directory to a blocking collection if they match the allowed extensions. 
        /// </summary>
        /// <param name="dir">The directory path from which to enumerate files.</param>
        /// <param name="normalizedAllowedExtensions">A collection of allowed file extensions, normalized to lowercase.</param>
        /// <param name="blockingCollection">The blocking collection to which matching files will be added.</param>
        /// <param name="batchSize">The maximum number of files to process in each batch (not directly used in this method but included for uniformity).</param>
        /// <param name="fileQueueDelayInMilliseconds">The delay in milliseconds to apply before adding each file to the queue, for throttling.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. The task result contains the count of files that were added to the blocking collection.</returns>
        /// <remarks>
        /// This method enumerates through all the files in the specified directory, and adds the file paths to a blocking collection if their extensions are in the allowed list.
        /// Throttling is applied based on the specified delay in milliseconds.
        /// </remarks>
        private static async Task<int> AddMatchingFiles(string dir, IEnumerable<string> normalizedAllowedExtensions, BlockingCollection<string> blockingCollection, int batchSize, int fileQueueDelayInMilliseconds)
        {
            int fileCount = 0;
            foreach (var file in Directory.EnumerateFiles(dir))
            {
                await Task.Delay(fileQueueDelayInMilliseconds); // Queue Throttling

                if (normalizedAllowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                {
                    blockingCollection.Add(file);
                    fileCount++;
                }
            }

            return fileCount;
        }

        /// <summary>
        /// Asynchronously processes files in batches, using a blocking collection to consume and process file paths.
        /// </summary>
        /// <param name="blockingCollection">A blocking collection containing the file paths to be processed.</param>
        /// <param name="batchProcessor">A function that processes a batch of files.</param>
        /// <param name="batchSize">The maximum number of files to process in each batch.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method collects file paths into batches and processes them using the provided batch processing function. 
        /// Throttling is applied based on the size of the batch and the time taken for the last batch to complete. 
        /// Any remaining files after all batches are processed will be processed as a final batch.
        /// </remarks>
        private static async Task ProcessFilesInBatchAsync(BlockingCollection<string> blockingCollection, Func<IEnumerable<string>, Task> batchProcessor, int batchSize)
        {
            var currentBatch = new List<string>();
            DateTime lastBatchCompletionTime = DateTime.Now;
            int batchThrottlingDelayInMilliseconds = batchSize * 100;

            foreach (var file in blockingCollection.GetConsumingEnumerable())
            {
                currentBatch.Add(file);

                if (currentBatch.Count >= batchSize)
                {
                    lastBatchCompletionTime = await ProcessBatchWithThrottling(batchProcessor, currentBatch, batchThrottlingDelayInMilliseconds, lastBatchCompletionTime);
                }
            }

            // Process any remaining files
            if (currentBatch.Count > 0)
            {
                await batchProcessor(currentBatch);
            }
        }

        /// <summary>
        /// Asynchronously processes a batch of files, applying throttling based on a specified delay to avoid system overload.
        /// </summary>
        /// <param name="batchProcessor">A function that processes a batch of files.</param>
        /// <param name="currentBatch">The list of file paths currently in the batch to be processed.</param>
        /// <param name="batchThrottlingDelayInMilliseconds">The amount of delay in milliseconds to apply for throttling if the time since the last batch is under the threshold.</param>
        /// <param name="lastBatchCompletionTime">The DateTime when the last batch was completed.</param>
        /// <returns>A <see cref="Task{DateTime}"/> that represents the asynchronous operation. The task result contains the DateTime indicating when the current batch was completed.</returns>
        /// <remarks>
        /// If the time elapsed since the last batch is less than the defined threshold, a delay is applied before processing the next batch.
        /// </remarks>
        private static async Task<DateTime> ProcessBatchWithThrottling(Func<IEnumerable<string>, Task> batchProcessor, List<string> currentBatch, int batchThrottlingDelayInMilliseconds, DateTime lastBatchCompletionTime)
        {
            const int ThrottlingThresholdInMilliseconds = 100;
            var timeSinceLastBatchInMilliseconds = (DateTime.Now - lastBatchCompletionTime).TotalMilliseconds;

            if (timeSinceLastBatchInMilliseconds < ThrottlingThresholdInMilliseconds)
            {
                await Task.Delay(batchThrottlingDelayInMilliseconds);
            }

            await batchProcessor(currentBatch);
            currentBatch.Clear();

            return DateTime.Now;
        }
    }
}
