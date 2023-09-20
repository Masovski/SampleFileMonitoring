using Newtonsoft.Json;

namespace SampleFileMonitoring.Services.DTO
{
    internal class SystemDetailsDTO
    {
        [JsonProperty("logonUser")]
        public string LogonUsername { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("ipaddr")]
        public string IPv4Address { get; set; }

        [JsonProperty("operatingSystem")]
        public string OperatingSystemName { get; set; }
    }
}
