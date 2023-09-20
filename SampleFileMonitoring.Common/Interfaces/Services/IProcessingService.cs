using System;
using System.Threading.Tasks;

namespace SampleFileMonitoring.Common.Interfaces.Services
{
    public interface IProcessingService
    {
        Task RegisterSystemAsync();
        Task RegisterFileAsync(string path);
    }
}