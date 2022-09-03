namespace Acquisition
{
    public class AcquisitionProgressEventArgs : EventArgs
    {
        public long TotalBytes { get; init; }
        public long ReceivedBytes { get; init; }
        public double BytesPerSecondSpeed { get; init; }
        public double AverageBytesPerSecondSpeed { get; init; }
        public double DownloadPercentage { get; init; }
        public TimeSpan DownloadDuration { get; init; }

        public AcquisitionProgressEventArgs(long totalBytes, long receivedBytes, double bytesPerSecondSpeed, TimeSpan downloadDuration)
        {
            TotalBytes = totalBytes;
            ReceivedBytes = receivedBytes;
            BytesPerSecondSpeed = bytesPerSecondSpeed;
            DownloadDuration = downloadDuration;
            AverageBytesPerSecondSpeed = ReceivedBytes / downloadDuration.TotalSeconds;
            DownloadPercentage = (double)ReceivedBytes * 100 / TotalBytes;
        }
    }
}
