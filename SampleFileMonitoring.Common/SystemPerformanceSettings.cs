namespace SampleFileMonitoring.Common
{
    public class SystemPerformanceSettings
    {
        public int MaxDegreeOfParallelism { get; set; }

        public int MaxConcurrentIORequests { get; set; }

        public int ConcurrencyDelayMultiplier { get; set; }
    }
}
