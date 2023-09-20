using SampleFileMonitoring.Common.Extensions;
using SampleFileMonitoring.Common.Interfaces;
using SampleFileMonitoring.Common.Interfaces.Services;

namespace SampleFileMonitoring.Services.Monitoring
{
    public class FileMonitoringService : IFileMonitoringService
    {
        private readonly IFileMonitorFactory _fileMonitorFactory;

        public FileMonitoringService(IFileMonitorFactory fileMonitorFactory)
        {
            _fileMonitorFactory = fileMonitorFactory;
        }

        public IFileMonitor MonitorFolderFiles(string folderPath)
        {
            return MonitorFolderFiles(folderPath, false);
        }

        public IFileMonitor MonitorFolderFiles(string folderPath, bool recursive)
        {
            return MonitorFolderFiles(folderPath, recursive, Enumerable.Empty<string>());
        }

        public IFileMonitor MonitorFolderFiles(string folderPath, IEnumerable<string> allowedExtensions)
        {
            return MonitorFolderFiles(folderPath, false, allowedExtensions);
        }

        public IFileMonitor MonitorFolderFiles(string folderPath, bool recursive, IEnumerable<string> allowedExtensions)
        {
            var fileMonitor = _fileMonitorFactory.Create();

            fileMonitor.SetPath(folderPath);
            fileMonitor.IncludeSubdirectories(recursive);
            fileMonitor.ApplyExtensionFilters(allowedExtensions);
            fileMonitor.Start();

            return fileMonitor;
        }
    }
}
