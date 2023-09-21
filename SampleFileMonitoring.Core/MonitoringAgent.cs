using Microsoft.Extensions.Logging;
using SampleFileMonitoring.Common.Interfaces;
using SampleFileMonitoring.Common.Interfaces.Services;
using System.IO;

namespace SampleFileMonitoring.Core
{
    public class MonitoringAgent : IDisposable, IMonitoringAgent
    {
        private readonly ILogger<MonitoringAgent> _logger;
        private readonly IFileMonitoringService _fileMonitoringService;
        private readonly ISystemService _systemService;
        private readonly IProcessingService _processingService;
        private readonly ISet<string> _allowedExtensions;

        private IFileMonitor _fileMonitor;

        public MonitoringAgent(
            ILogger<MonitoringAgent> logger,
            IFileMonitoringService fileMonitoringService,
            ISystemService systemService,
            IProcessingService processingService)
        {
            this._logger = logger;
            _fileMonitoringService = fileMonitoringService;
            _systemService = systemService;
            _processingService = processingService;
            _allowedExtensions = Configuration.Monitoring.AllowedFileExtensions;
        }

        public async Task RunAsync(string rootPath)
        {
            _logger.LogInformation($"Starting new monitoring agent on {rootPath}");
            await RegisterAllFilesUnderDirectory(rootPath);

            _fileMonitor = BeginMonitoring(rootPath);
            RegisterFilesOnFileEvents(_fileMonitor);
        }

        private async Task RegisterAllFilesUnderDirectory(string rootPath)
        {
            _logger.LogInformation($"Started bulk upload for files under {rootPath} (recursive) with extensions: {string.Join(", ", _allowedExtensions)}");

            await _systemService.FindFilesAsync(rootPath, true, Configuration.Monitoring.AllowedFileExtensions, async (files) =>
            {
                await RegisterBulkFilesAsync(files);
            }, Configuration.SystemPerformance.BulkFileProcessorBatchSize);
        }

        public async Task RunAsync()
        {
            await RunAsync(Configuration.Monitoring.DefaultMonitoringPath);
        }

        public void Dispose()
        {
            _fileMonitor?.Dispose();
            GC.SuppressFinalize(this);
        }

        private IFileMonitor BeginMonitoring(string path)
        {
            var monitor = _fileMonitoringService.MonitorFolderFiles(path, true, Configuration.Monitoring.AllowedFileExtensions);
            _logger.LogInformation($"Started monitoring on {path} for files with extensions: {string.Join(", ", _allowedExtensions)}");

            return monitor;
        }

        private void RegisterFilesOnFileEvents(IFileMonitor fileMonitor)
        {
            fileMonitor.FileCreated += async (sender, filePath) => await _processingService.RegisterFileAsync(filePath);
            fileMonitor.FileChanged += async (sender, filePath) => await _processingService.RegisterFileAsync(filePath);
        }

        private async Task RegisterBulkFilesAsync(IEnumerable<string> files)
        {
            await Parallel.ForEachAsync(
                files,
                new ParallelOptions { MaxDegreeOfParallelism = Configuration.SystemPerformance.MaxDegreeOfParallelism },
                async (file, cancellationToken) =>
                {
                    await _processingService.RegisterFileAsync(file);
                });
        }
    }
}