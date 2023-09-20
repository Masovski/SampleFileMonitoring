using Microsoft.Extensions.Logging;
using SampleFileMonitoring.Common;
using SampleFileMonitoring.Common.Interfaces;

namespace SampleFileMonitoring.Core
{
    /// <summary>
    /// Provides a monitoring service for tracking changes in a filesystem folder, including file creation, modification, deletion, and renaming.
    /// </summary>
    /// <remarks>
    /// The class implements the <see cref="IFileMonitor"/>, <see cref="ISupportSubdirectories"/>, and <see cref="IFileExtensionFilter"/> interfaces, providing capabilities to filter files based on their extensions and to include subdirectories in the monitoring process.
    /// </remarks>
    internal class FolderFileMonitor : IFileMonitor, ISupportSubdirectories, IFileExtensionFilter
    {
        private FileSystemWatcher _fileSystemWatcher;
        private readonly ISet<string> _allowedExtensions;
        private readonly object _lock = new();
        private readonly ILogger _logger;

        private readonly Dictionary<string, DateTime> _lastRead;
        private readonly TimeSpan _debounceTime;

        public event EventHandler<string> FileCreated;
        public event EventHandler<string> FileDeleted;
        public event EventHandler<string> FileChanged;
        public event EventHandler<FileRenamedArgs> FileRenamed;

        internal FolderFileMonitor(ILogger<FolderFileMonitor> logger)
        {
            _fileSystemWatcher = new()
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _allowedExtensions = new HashSet<string>();
            _logger = logger;
            _lastRead = new Dictionary<string, DateTime>();
            _debounceTime = TimeSpan.FromSeconds(2); // Adjust as necessary
        }

        public void Start()
        {
            _logger.LogInformation("Monitor: Starting...");
            lock (_lock)
            {
                _fileSystemWatcher.Created += OnFileCreated;
                _fileSystemWatcher.Changed += OnFileChanged;
                _fileSystemWatcher.Renamed += OnFileRenamed;
                _fileSystemWatcher.Deleted += OnFileDeleted;
                _fileSystemWatcher.Error += OnError;

                _fileSystemWatcher.EnableRaisingEvents = true;
                _logger.LogInformation("Monitor: Started.");
            }
        }

        public void IncludeSubdirectories()
        {
            Console.WriteLine("Monitor: Including subdirectories (recursive)");
            _fileSystemWatcher.IncludeSubdirectories = true;
        }

        public void Stop()
        {
            lock (_lock)
            {
                _logger.LogInformation("Monitor: Stopping...");
                if (_fileSystemWatcher == null)
                {
                    return;
                }

                _fileSystemWatcher.EnableRaisingEvents = false;
                _fileSystemWatcher.Created -= OnFileCreated;
                _fileSystemWatcher.Changed -= OnFileChanged;
                _fileSystemWatcher.Deleted -= OnFileDeleted;
                _fileSystemWatcher.Renamed -= OnFileRenamed;
                _fileSystemWatcher.Error -= OnError;
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Monitor: Disposing...");
            Stop();
            _fileSystemWatcher?.Dispose();
            _fileSystemWatcher = null;
        }

        public void SetPath(string path)
        {
            _fileSystemWatcher.Path = path;
        }

        public bool FilterByAllowedExtensions(IEnumerable<string> extensions)
        {
            foreach (string extension in extensions)
            {
                _allowedExtensions.Add(extension.ToLowerInvariant());
            }

            return true;
        }

        /// <summary>
        /// Event handler for the <see cref="FileSystemWatcher"/>'s FileCreated event.
        /// </summary>
        /// <param name="sender">The source of the event, typically an instance of <see cref="System.IO.FileSystemWatcher"/>.</param>
        /// <param name="e">Event data containing information about the created file.</param>
        /// <remarks>
        /// This method logs the creation of the new file using the logger. It also checks if the created file meets certain validation criteria, such as existing at the specified path and having an allowed extension. If these conditions are met, the FileCreated event is invoked.
        /// </remarks>
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            WhenFileIsValid(e.FullPath, () =>
            {
                _logger.LogDebug("Monitor: File created: " + e.FullPath);
                FileCreated?.Invoke(this, e.FullPath);
            });
        }

        /// <summary>
        /// Event handler for the <see cref="FileSystemWatcher"/>'s FileChanged event.
        /// </summary>
        /// <param name="sender">The source of the event, typically an instance of <see cref="System.IO.FileSystemWatcher"/>.</param>
        /// <param name="e">Event data containing information about the changed file.</param>
        /// <remarks>
        /// This method logs the change in the file using the logger. It then checks if the changed file meets certain validation criteria, such as existing at the specified path and having an allowed extension. If these conditions are met, the FileChanged event is invoked.
        /// </remarks>
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            WhenFileIsValid(e.FullPath, () =>
            {
                _logger.LogDebug("Monitor: File changed: " + e.FullPath);
                FileChanged?.Invoke(this, e.FullPath);
            });
        }

        /// <summary>
        /// Event handler for the FileSystemWatcher's FileDeleted event.
        /// </summary>
        /// <param name="sender">The source of the event, typically an instance of <see cref="System.IO.FileSystemWatcher"/>.</param>
        /// <param name="e">Event data containing information about the deleted file.</param>
        /// <remarks>
        /// This method logs the deletion of the file using the logger. It then checks if the deleted file meets certain validation criteria, such as having been located at the specified path and having an allowed extension. If these conditions are met, the FileDeleted event is invoked.
        /// </remarks>
        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            WhenFileIsValid(e.FullPath, () =>
            {
                _logger.LogDebug("Monitor: File deleted: " + e.FullPath);
                FileDeleted?.Invoke(this, e.FullPath);
            });
        }

        /// <summary>
        /// Event handler for the FileSystemWatcher's FileRenamed event.
        /// </summary>
        /// <param name="sender">The source of the event, typically an instance of <see cref="System.IO.FileSystemWatcher"/>.</param>
        /// <param name="e">Event data containing information about the renamed file.</param>
        /// <remarks>
        /// This method logs the renaming of the file using the logger. It then checks if the renamed file meets certain validation criteria, such as existing at the new specified path and having an allowed extension. If these conditions are met, the FileRenamed event is invoked with additional data including the old and new file paths.
        /// </remarks>
        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            WhenFileIsValid(e.FullPath, () =>
            {
                _logger.LogDebug("Monitor: File renamed: " + e.FullPath);
                FileRenamed?.Invoke(this, new FileRenamedArgs(e.OldFullPath, e.FullPath));
            });
        }

        /// <summary>
        /// Event handler for the FileSystemWatcher's Error event.
        /// </summary>
        /// <param name="sender">The source of the event, typically an instance of <see cref="System.IO.FileSystemWatcher"/>.</param>
        /// <param name="e">Event data containing information about the error.</param>
        /// <remarks>
        /// This method logs any errors that occur during the file monitoring process using the logger. The logged information includes the exception details.
        /// </remarks>

        private void OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogError(e.GetException(), string.Empty);
        }

        /// <summary>
        /// Executes the provided callback when the specified file has a valid extension and exists on the file system.
        /// </summary>
        /// <param name="filePath">The full path to the file to check for validity.</param>
        /// <param name="callback">The callback to execute if the file is valid.</param>
        /// <remarks>
        /// The method checks for file existence and whether its extension is allowed.
        /// If both conditions are met, it delegates to another check for the file's readability before executing the callback.
        /// </remarks>
        private void WhenFileIsValid(string filePath, Action callback)
        {
            string extension = Path.GetExtension(filePath);
            if (!File.Exists(filePath) || !IsAllowedExtension(extension))
            {
                return;
            }

            WhenFileCanBeReadAsync(filePath, callback);
        }

        /// <summary>
        /// Ensures that the specified file can be read, then executes the provided callback.
        /// </summary>
        /// <param name="filePath">The full path to the file to check for readability.</param>
        /// <param name="callback">The callback to execute when the file is ready to be read.</param>
        /// <param name="retryDelayInMilliseconds">The delay in milliseconds between retries to check if the file can be read. Default value is 1000 milliseconds.</param>
        /// <remarks>
        /// The method uses a debounce mechanism to avoid rapidly repeating read checks on the same file.
        /// If a recent check occurred for the specified file, subsequent checks are ignored until a debounce interval elapses.
        /// The method polls the file for readiness, retrying at the specified interval, and calls the callback once the file is ready.
        /// </remarks>
        private async Task WhenFileCanBeReadAsync(string filePath, Action callback, int retryDelayInMilliseconds = 1000)
        {
            lock (_lastRead)
            {
                if (_lastRead.TryGetValue(filePath, out DateTime lastReadTime))
                {
                    if (lastReadTime - DateTime.Now < _debounceTime)
                    {
                        return; // Debouncing: if event occurred recently, ignore it
                    }

                    _lastRead[filePath] = DateTime.Now;
                }
                else
                {
                    _lastRead.Add(filePath, DateTime.Now);
                }
            }

            await Task.Run(async () =>
            {
                while (!IsFileReady(filePath))
                {
                    await Task.Delay(retryDelayInMilliseconds);
                }
            }).ContinueWith(t => callback());
        }

        /// <summary>
        /// Determines if the specified file is ready to be accessed.
        /// </summary>
        /// <param name="filename">The full path to the file to check.</param>
        /// <returns><c>true</c> if the file can be accessed; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// This method attempts to open the file with exclusive access. If the operation succeeds, it indicates the file is not locked by another process and is ready for access.
        /// If the file is currently in use or locked by another process, an exception is thrown, and the method returns <c>false</c>.
        /// </remarks>
        private static bool IsFileReady(string filename)
        {
            try
            {
                using FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                return inputStream != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether a given file extension is allowed based on the current set of allowed extensions.
        /// </summary>
        /// <param name="extension">The file extension to check.</param>
        /// <returns>
        /// Returns <c>true</c> if the extension is allowed or if the list of allowed extensions is empty; otherwise, returns <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If no extensions have been specified in <c>_allowedExtensions</c>, all extensions are considered allowed.
        /// </remarks>
        private bool IsAllowedExtension(string extension)
        {
            if (_allowedExtensions.Any())
            {
                return _allowedExtensions.Contains(extension.ToLowerInvariant());
            }

            return true;
        }
    }
}
