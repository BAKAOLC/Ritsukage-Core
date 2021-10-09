using Downloader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ritsukage.Tools.Console;
using System.ComponentModel;
using System.Net;

namespace Ritsukage.Tools
{
    public static class DownloadManager
    {
        public const string CacheFolder = "CacheFolder";
        public const string CacheRecordFile = "CacheRecord";

        struct CacheData
        {
            [JsonProperty("url")]
            public string Url { get; init; }

            [JsonProperty("path")]
            public string Path { get; init; }

            [JsonProperty("time")]
            public DateTime CacheTime { get; init; }

            [JsonProperty("to")]
            public DateTime ClearTime { get; init; }

            [JsonIgnore]
            public bool Exists => File.Exists(Path);

            public CacheData(string url, string path, DateTime cacheTime, int keepTime)
            {
                Url = url;
                Path = path;
                CacheTime = cacheTime;
                ClearTime = cacheTime.AddSeconds(keepTime);
            }

            public void Delete()
            {
                if (Exists)
                    File.Delete(Path);
            }
        }

        /// <summary>
        /// 获取新的缓存文件名
        /// </summary>
        public static string CacheFileName
            => Path.GetFullPath(Path.Combine(CacheFolder, Guid.NewGuid().ToString() + ".cache"));

        static readonly List<string> DownloadingList = new();

        static readonly ConcurrentDictionary<string, CacheData> CacheDataList = new();

        static bool _init = false;

        static void Init()
        {
            if (_init) return;
            _init = true;

            if (!Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);

            if (File.Exists(CacheRecordFile))
            {
                CacheDataList.Clear();

                var array = JArray.Parse(File.ReadAllText(CacheRecordFile));
                foreach (var data in array)
                {
                    var cache = data.ToObject<CacheData>();
                    if (cache.Exists)
                        CacheDataList.TryAdd(cache.Url, cache);
                }

                foreach (var file in Directory.GetFiles(CacheFolder))
                    if (!CacheDataList.Any(x => x.Value.Path == Path.GetFullPath(file)))
                        File.Delete(file);
            }

            Save();

            StartCacheCleanupThread();
        }

        static void Save()
        {
            lock (CacheDataList)
            {
                File.WriteAllText(CacheRecordFile, JsonConvert.SerializeObject(CacheDataList.Values));
            }
        }

        static void DebugLog(string text)
            => ConsoleLog.Debug("Downloader", text);

        public static async Task<string> GetCache(string url)
        {
            while (DownloadingList.Contains(url))
                await Task.Delay(100);

            if (CacheDataList.TryGetValue(url, out var cache))
            {
                if (cache.Exists)
                    return cache.Path;
                else
                {
                    CacheDataList.TryRemove(url, out _);
                    Save();
                }
            }

            return null;
        }

        public static async Task<string> Download(string url, string referer = null, int keepTime = 3600,
            Action<DownloadStartedEventArgs> DownloadStartedAction = null,
            Action<DownloadProgressChangedEventArgs> DownloadProgressChangedAction = null,
            Action<DownloadFileCompletedEventArgs> DownloadFileCompletedAction = null, int UpdateInfoDelay = 1000)
        {
            Init();

            #region 检查缓存
            var _cache = await GetCache(url);
            if (!string.IsNullOrEmpty(_cache))
                return _cache;
            #endregion

            DownloadingList.Add(url);

            #region 下载
            Stream stream = null;
            var task = new DownloadTask(url, referer)
            {
                UpdateInfoDelay = UpdateInfoDelay
            };
            task.DownloadStarted += (s, e) =>
            {
                DebugLog($"Start to download file from {url} ({e.FileSize} bytes)");
                DownloadStartedAction?.Invoke(e);
            };
            task.DownloadProgressChanged += (s, e) =>
            {
                DebugLog($"Downloading {url}... {e.ReceivedBytes}/{e.TotalBytes} ({e.DownloadPercentage:F2}%)");
                DownloadProgressChangedAction?.Invoke(e);
            };
            task.DownloadFileCompleted += (s, e) =>
            {
                if (e.Status == DownloadTaskStatus.Error)
                {
                    if (e.Exception != null)
                        DebugLog($"Download {url} failed." + Environment.NewLine + e.Exception.GetFormatString(true));
                    else
                        DebugLog($"Download {url} failed with unknown exception.");
                }
                else if (e.Status == DownloadTaskStatus.Cancelled)
                    DebugLog($"Download {url} cancelled.");
                else
                    DebugLog($"Download {url} completed.");
                DownloadFileCompletedAction?.Invoke(e);
            };
            await task.WaitForDownloadCompleted();
            stream = task.FileStream;
            if (task.FileSize == -1 || task.Status != DownloadTaskStatus.Completed)
            {
                try
                {
                    stream = await Utils.GetFileAsync(url, referer);
                }
                catch (Exception ex)
                {
                    ConsoleLog.Error("Download Manager", "简易下载再次失败" + Environment.NewLine + ex.GetFormatString());
                }
                if (stream == null)
                {
                    DownloadingList.Remove(url);
                    return null;
                }
            }
            #endregion

            #region 储存
            var file = CacheFileName;
            stream.SaveToFile(file);
            stream.Dispose();
            CacheDataList.TryAdd(url, new CacheData(url, file, DateTime.Now, keepTime));
            Save();
            #endregion

            DownloadingList.Remove(url);
            return file;
        }

        public static Task<string[]> Download(string[] urls, string referer = null, int keepTime = 3600)
        {
            var result = new string[urls.Length];
            var tasks = new Task<string>[urls.Length];
            for (int i = 0; i < urls.Length; i++)
                tasks[i] = Download(urls[i], referer, keepTime);
            Task.WaitAll(tasks);
            for (int i = 0; i < urls.Length; i++)
                result[i] = tasks[i].Result;
            return Task.FromResult(result);
        }

        const int SaveBufferSize = 4096;
        static void SaveToFile(this Stream stream, string path)
        {
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var buffer = new byte[SaveBufferSize];
            using var fileStream = File.OpenWrite(path);
            int osize;
            stream.Seek(0, SeekOrigin.Begin);
            while ((osize = stream.Read(buffer, 0, SaveBufferSize)) > 0)
                fileStream.Write(buffer, 0, osize);
            fileStream.Close();
            fileStream.Dispose();
        }

        const int ClearCacheDelay = 1000;
        static void StartCacheCleanupThread()
        {
            new Thread(() =>
            {
                while (true)
                {
                    var now = DateTime.Now;
                    var list = CacheDataList.Where(x => x.Value.ClearTime < now).ToArray();
                    foreach (var data in list)
                    {
                        if (data.Value.Exists)
                            data.Value.Delete();
                        CacheDataList.TryRemove(data.Key, out _);
                    }
                    Save();
                    Thread.Sleep(ClearCacheDelay);
                }
            })
            {
                IsBackground = true
            }.Start();
        }
    }

    public class DownloadTask
    {
        public static readonly DownloadConfiguration DefaultDownloadConfig = new DownloadConfiguration()
        {
            BufferBlockSize = 4096,
            ChunkCount = 5,
            OnTheFlyDownload = false,
            ParallelDownload = true
        };

        public static int DefaultUpdateInfoDelay = 1000;

        public DownloadTaskStatus Status { get; private set; }
        public Exception Exception { get; private set; }

        public string Url { get; init; }
        public DownloadService Service { get; init; }
        public DownloadConfiguration DownloadConfig { get; init; }
        public RequestConfiguration RequestConfig
        {
            get => DownloadConfig.RequestConfiguration;
            set => DownloadConfig.RequestConfiguration = value;
        }
        public int UpdateInfoDelay = DefaultUpdateInfoDelay;

        public DateTime BeginTime { get; private set; }
        public DateTime DownloadStartedTime { get; private set; }
        public DateTime DownloadCompletedTime { get; private set; }
        public string FileName { get; private set; } = string.Empty;
        public long FileSize { get; private set; } = -1;
        public Stream FileStream { get; private set; }

        public event EventHandler<DownloadStartedEventArgs> DownloadStarted;
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<DownloadFileCompletedEventArgs> DownloadFileCompleted;

        public DownloadTask(string url, string referer = null, string cookie = null, DownloadConfiguration config = null)
        {
            Url = url;
            DownloadConfig = config ?? DefaultDownloadConfig.Clone() as DownloadConfiguration;
            RequestConfig.AutomaticDecompression = DecompressionMethods.All;
            RequestConfig.AllowAutoRedirect = true;
            RequestConfig.UserAgent = Utils.GetUserAgent("pc");
            if (!string.IsNullOrEmpty(referer))
                RequestConfig.Referer = referer;
            if (!string.IsNullOrEmpty(cookie))
                RequestConfig.Headers["cookie"] = cookie;
            Service = new DownloadService(DownloadConfig);
            Service.DownloadStarted += DownloadStartedEventHandler;
            Service.DownloadProgressChanged += DownloadProgressChangedEventHandler;
            Service.DownloadFileCompleted += DownloadFileCompletedEventHandler;
        }

        public void StartAsync()
        {
            switch (Status)
            {
                case DownloadTaskStatus.Connecting:
                case DownloadTaskStatus.Downloading:
                case DownloadTaskStatus.Completed:
                    return;
            }
            Status = DownloadTaskStatus.Connecting;
            StartDownload();
        }

        public void CancelAsync()
        {
            switch (Status)
            {
                case DownloadTaskStatus.Connecting:
                case DownloadTaskStatus.Downloading:
                    Service.CancelAsync();
                    break;
            }
        }

        Task _downloadTask;
        void StartDownload()
        {
            FileStream = null;
            Exception = null;
            BeginTime = DateTime.Now;
            DownloadStartedTime = default;
            DownloadCompletedTime = default;
            var task = Service.DownloadFileTaskAsync(Url);
            _downloadTask = Task.Run(async () =>
            {
                Stream stream = null;
                try
                {
                    FileStream = stream = await task;
                }
                catch (Exception ex)
                {
                    Status = DownloadTaskStatus.Error;
                    Exception = ex;
                    ConsoleLog.Error("Download Task",
                        "下载文件时发生错误："
                        + Url
                        + Environment.NewLine
                        + ex.GetFormatString(true));
                }
                TimeSpan duration = TimeSpan.Zero;
                if (DownloadStartedTime != default)
                    duration = DateTime.Now - DownloadStartedTime;
                DownloadFileCompleted?.Invoke(Service, new DownloadFileCompletedEventArgs(Status, duration, stream, Exception));
            });
        }

        public async Task WaitForDownloadCompleted()
        {
            switch (Status)
            {
                case DownloadTaskStatus.Completed:
                    return;
                case DownloadTaskStatus.WaitToStart:
                case DownloadTaskStatus.Cancelled:
                case DownloadTaskStatus.Error:
                    StartDownload();
                    break;
            }
            await _downloadTask;
        }

        DateTime _lastUpdateTime;

        void DownloadStartedEventHandler(object sender, Downloader.DownloadStartedEventArgs eventArgs)
        {
            var now = DateTime.Now;
            Status = DownloadTaskStatus.Downloading;
            DownloadStartedTime = now;
            _lastUpdateTime = now;
            FileName = eventArgs.FileName;
            FileSize = eventArgs.TotalBytesToReceive;
            DownloadStarted?.Invoke(Service, new DownloadStartedEventArgs(eventArgs.FileName, eventArgs.TotalBytesToReceive));
        }

        void DownloadProgressChangedEventHandler(object sender, Downloader.DownloadProgressChangedEventArgs eventArgs)
        {
            var now = DateTime.Now;
            var dt = (now - _lastUpdateTime).TotalMilliseconds;
            if (dt >= UpdateInfoDelay)
            {
                _lastUpdateTime = now;
                DownloadProgressChanged?.Invoke(Service, new DownloadProgressChangedEventArgs(eventArgs.TotalBytesToReceive, eventArgs.ReceivedBytesSize,
                    eventArgs.BytesPerSecondSpeed, eventArgs.AverageBytesPerSecondSpeed, now - DownloadStartedTime));
            }
        }

        void DownloadFileCompletedEventHandler(object sender, AsyncCompletedEventArgs eventArgs)
        {
            DownloadCompletedTime = DateTime.Now;
            Status = DownloadTaskStatus.Completed;
            if (eventArgs.Cancelled)
                Status = DownloadTaskStatus.Cancelled;
            if (eventArgs.Error != null)
                Status = DownloadTaskStatus.Error;
        }

        public delegate void EventHandler<in TEventArgs>(DownloadService service, TEventArgs eventArgs)
            where TEventArgs : EventArgs;
    }

    public enum DownloadTaskStatus
    {
        WaitToStart,
        Connecting,
        Downloading,
        Completed,
        Cancelled,
        Error
    }

    public class DownloadStartedEventArgs : EventArgs
    {
        public string FileName { get; init; }
        public long FileSize { get; init; }

        public DownloadStartedEventArgs(string filename, long filesize)
        {
            FileName = filename;
            FileSize = filesize;
        }
    }

    public class DownloadProgressChangedEventArgs : EventArgs
    {
        public long TotalBytes { get; init; }
        public long ReceivedBytes { get; init; }
        public double BytesPerSecondSpeed { get; init; }
        public double AverageBytesPerSecondSpeed { get; init; }
        public double DownloadPercentage { get; init; }
        public TimeSpan DownloadDuration { get; init; }

        public DownloadProgressChangedEventArgs(long totalBytes, long receivedBytes, double bytesPerSecondSpeed, double averageBytesPerSecondSpeed, TimeSpan downloadDuration)
        {
            TotalBytes = totalBytes;
            ReceivedBytes = receivedBytes;
            BytesPerSecondSpeed = bytesPerSecondSpeed;
            AverageBytesPerSecondSpeed = averageBytesPerSecondSpeed;
            DownloadPercentage = (double)ReceivedBytes * 100 / TotalBytes;
            DownloadDuration = downloadDuration;
        }
    }

    public class DownloadFileCompletedEventArgs : EventArgs
    {
        public DownloadTaskStatus Status { get; init; }

        public Exception Exception { get; init; }

        public Stream FileStream { get; init; }

        public TimeSpan DownloadDuration { get; init; }

        public DownloadFileCompletedEventArgs(DownloadTaskStatus status, TimeSpan downloadDuration, Stream fileStream = null, Exception exception = null)
        {
            Status = status;
            FileStream = fileStream;
            Exception = exception;
            DownloadDuration = downloadDuration;
        }
    }
}
