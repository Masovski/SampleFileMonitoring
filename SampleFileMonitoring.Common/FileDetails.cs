namespace SampleFileMonitoring.Common
{
    public class FileDetails
    {
        public string FileName { get; set; }

        public string FileType { get; set; }

        public string Location { get; set; }

        public string Hash { get; set; }

        public string CreatedAtUtc { get; set; }

        public string ModifiedDateUtc { get; set; }

        public long FileSize { get; set; }
    }
}