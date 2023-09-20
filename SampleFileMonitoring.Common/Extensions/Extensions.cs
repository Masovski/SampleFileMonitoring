using SampleFileMonitoring.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SampleFileMonitoring.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="IFileMonitor"/> interface.
    /// </summary>
    public static class FileMonitorExtensions
    {
        /// <summary>
        /// Configures an <see cref="IFileMonitor"/> to include or exclude subdirectories when supported.
        /// </summary>
        /// <param name="fileMonitor">The <see cref="IFileMonitor"/> instance to configure.</param>
        /// <param name="includeSubdirectories">If set to <c>true</c>, subdirectories will be included; otherwise they will be excluded.</param>
        /// <remarks>
        /// This method will only affect <see cref="IFileMonitor"/> instances that also implement <see cref="ISupportSubdirectories"/>.
        /// </remarks>
        public static void IncludeSubdirectories(this IFileMonitor fileMonitor, bool includeSubdirectories)
        {
            if (includeSubdirectories && fileMonitor is ISupportSubdirectories recursiveFolderMonitor)
            {
                recursiveFolderMonitor.IncludeSubdirectories();
            }
        }

        /// <summary>
        /// Applies file extension filters to an <see cref="IFileMonitor"/>.
        /// </summary>
        /// <param name="fileMonitor">The <see cref="IFileMonitor"/> instance to configure.</param>
        /// <param name="extensions">A collection of file extensions to filter by.</param>
        /// <remarks>
        /// This method will only affect <see cref="IFileMonitor"/> instances that also implement <see cref="IFileExtensionFilter"/>.
        /// </remarks>
        public static void ApplyExtensionFilters(this IFileMonitor fileMonitor, IEnumerable<string> extensions)
        {
            if (extensions.Any() && fileMonitor is IFileExtensionFilter extensionFilterApplier)
            {
                extensionFilterApplier.FilterByAllowedExtensions(extensions);
            }
        }
    }
}
