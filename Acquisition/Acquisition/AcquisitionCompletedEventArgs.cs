namespace Acquisition
{
    public class AcquisitionCompletedEventArgs : EventArgs
    {
        public AcquisitionResult Status { get; init; }

        public Exception Exception { get; init; }

        public Stream FileStream { get; init; }

        public TimeSpan DownloadDuration { get; init; }

        public AcquisitionCompletedEventArgs(AcquisitionResult status, TimeSpan downloadDuration, Stream fileStream = null, Exception exception = null)
        {
            Status = status;
            FileStream = fileStream;
            Exception = exception;
            DownloadDuration = downloadDuration;
        }
    }
}
