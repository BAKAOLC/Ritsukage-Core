namespace Acquisition
{
    public class AcquisitionCompletedEventArgs(
        AcquisitionResult status,
        TimeSpan downloadDuration,
        Stream? fileStream = null,
        Exception? exception = null)
        : EventArgs
    {
        public AcquisitionResult Status { get; init; } = status;

        public Exception? Exception { get; init; } = exception;

        public Stream? FileStream { get; init; } = fileStream;

        public TimeSpan DownloadDuration { get; init; } = downloadDuration;
    }
}