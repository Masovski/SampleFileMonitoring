namespace SampleFileMonitoring.Common
{
    public class FileRenamedArgs
    {
        public FileRenamedArgs(string oldFullPath, string newFullPath)
        {
            OldFullPath = oldFullPath;
            NewFullPath = newFullPath;
        }

        public string OldFullPath { get; private set; }

        public string NewFullPath { get; private set; }
    }
}
