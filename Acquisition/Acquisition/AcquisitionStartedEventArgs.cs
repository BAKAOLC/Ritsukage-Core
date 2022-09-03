namespace Acquisition
{
    public class AcquisitionStartedEventArgs : EventArgs
    {
        public string FileName { get; init; }
        public long FileSize { get; init; }

        public AcquisitionStartedEventArgs(string filename, long filesize)
        {
            FileName = filename;
            FileSize = filesize;
        }
    }
}
