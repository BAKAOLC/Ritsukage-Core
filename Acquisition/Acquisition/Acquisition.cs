namespace Acquisition
{
    public abstract class Acquisition
    {
        public Acquisition(string url, string? referer = null, string? filename = null, string? directory = null)
        {
            Url = url;
            Referer = referer;
            Filename = filename ?? Guid.NewGuid().ToString().Replace("-", string.Empty) + ".temp";
            Directory = directory ?? Path.GetTempPath();
        }

        public string Url { get; init; }

        public string? Referer { get; init; }

        public string Filename { get; init; }

        public string Directory { get; init; }
        public event EventHandler<AcquisitionStartedEventArgs> DownloadStarted;
        public event EventHandler<AcquisitionProgressEventArgs> DownloadProgressChanged;
        public event EventHandler<AcquisitionCompletedEventArgs> DownloadFileCompleted;

        public abstract Task StartDownloadAsync();
        public abstract Task CancelAsync();
        public abstract Task WaitForDownloadCompletedAsync();

        protected void OnDownloadStarted(AcquisitionStartedEventArgs info)
        {
            DownloadStarted?.Invoke(this, info);
        }

        protected void OnProgressChanged(AcquisitionProgressEventArgs progress)
        {
            DownloadProgressChanged?.Invoke(this, progress);
        }

        protected void OnCompleted(AcquisitionCompletedEventArgs result)
        {
            DownloadFileCompleted?.Invoke(this, result);
        }
    }
}