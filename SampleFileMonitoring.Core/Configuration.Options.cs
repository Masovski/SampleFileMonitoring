namespace SampleFileMonitoring.Core
{
    public sealed class MonitoringOptions
    {
        public string DefaultMonitoringPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public ISet<string> AllowedFileExtensions { get; set; } = new HashSet<string> { };
    }

    public sealed class SystemPerformanceOptions
    {
        public int ConcurrencyDelayMultiplier { get; init; } = 5;
        public int MaxConcurrentIORequests { get; set; } = 20;
        public int MaxDegreeOfParallelism { get; set; } = 4;
        public int BulkFileProcessorBatchSize { get; set; } = 10;
    }

    public sealed class ProcessingEndpointsOptions
    {
        public string BaseAddress { get; set; } = "http://localhost:4242";
        public string RelativeRegisterFileEndpoint { get; set; } = "/register/file";
        public string RelativeRegisterSystemEndpointPath { get; set; } = "/register/system";
    }
}
