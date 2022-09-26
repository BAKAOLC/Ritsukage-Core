namespace Acquisition
{
    public abstract class Acquisition
    {
        public event EventHandler<AcquisitionStartedEventArgs> DownloadStarted;
        public event EventHandler<AcquisitionProgressEventArgs> DownloadProgressChanged;
        public event EventHandler<AcquisitionCompletedEventArgs> DownloadFileCompleted;

        public string Url { get; init; }

        public string Referer { get; init; }

        public string Filename { get; init; }

        public string Directory { get; init; }

        public Acquisition(string url, string referer = null, string filename = null, string directory = null)
        {
            Url = url;
            Referer = referer;
            Filename = filename ?? Guid.NewGuid().ToString().Replace("-", string.Empty) + ".temp";
            Directory = directory ?? Path.GetTempPath();
        }

        public abstract Task StartDownloadAsync();
        public abstract Task CancelAsync();
        public abstract Task WaitForDownloadCompleted();

        protected void OnDownloadStarted(AcquisitionStartedEventArgs info)
        {
            this.DownloadStarted?.Invoke(this, info);
        }

        protected void OnProgressChanged(AcquisitionProgressEventArgs progress)
        {
            this.DownloadProgressChanged?.Invoke(this, progress);
        }

        protected void OnCompleted(AcquisitionCompletedEventArgs result)
        {
            this.DownloadFileCompleted?.Invoke(this, result);
        }
    }
}