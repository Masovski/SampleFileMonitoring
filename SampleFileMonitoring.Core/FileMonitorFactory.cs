using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SampleFileMonitoring.Common.Interfaces;

namespace SampleFileMonitoring.Core
{
    public class FileMonitorFactory : IFileMonitorFactory
    {
        private readonly ILogger<FileMonitorFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        public FileMonitorFactory(ILogger<FileMonitorFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _logger.LogDebug($"Initializing the {nameof(FileMonitorFactory)} class.");
            this._serviceProvider = serviceProvider;
        }

        public IFileMonitor Create()
        {
            var fileMonitor = new FolderFileMonitor(
                    _serviceProvider.GetRequiredService<ILogger<FolderFileMonitor>>()
                );

            _logger.LogDebug($"{nameof(FileMonitorFactory)} has created a new {fileMonitor.GetType().Name}");

            return fileMonitor;
        }
    }
}
