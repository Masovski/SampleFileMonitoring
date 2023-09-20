using Newtonsoft.Json;

namespace SampleFileMonitoring.Services.DTO
{
    internal class FileDetailsDTO
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("fileType")]
        public string FileType { get; set; }

        [JsonProperty("fileLocation")]
        public string Location { get; set; }

        [JsonProperty("fileHash")]
        public string Hash { get; set; }

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; }

        [JsonProperty("modifiedAt")]
        public string ModifiedDate { get; set; }

        [JsonProperty("fileSize")]
        public long FileSize { get; set; }
    }
}
