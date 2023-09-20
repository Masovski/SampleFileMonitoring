using System.Collections.Generic;

namespace SampleFileMonitoring.Common.Interfaces
{
    /// <summary>
    /// Provides functionality for filtering file extensions.
    /// </summary>
    public interface IFileExtensionFilter
    {
        /// <summary>
        /// Applies the filter to allow only specific file extensions.
        /// </summary>
        /// <param name="extensions">The list of extensions to allow.</param>
        /// <returns>A boolean indicating if the filter was successfully applied.</returns>
        bool FilterByAllowedExtensions(IEnumerable<string> extensions);
    }
}