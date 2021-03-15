using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Ritsukage.Tools.Download
{
    public class DownloadTask
    {
        /// <summary>
        /// 目标文件Url
        /// </summary>
        public string Url { get; init; }

        /// <summary>
        /// 文件总大小
        /// </summary>
        public long TotalBytes { get; private set; } = 0;

        /// <summary>
        /// 文件已接收大小
        /// </summary>
        public long TotalReceivedBytes { get; private set; } = 0;

        /// <summary>
        /// 文件接收百分比
        /// </summary>
        public double Progress
        {
            get => TotalBytes != 0 ? (double)TotalReceivedBytes / TotalBytes : 0;
        }

        /// <summary>
        /// 连接UA
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/82.0.4056.0 Safari/537.36 Edg/82.0.431.0";

        /// <summary>
        /// 超时时间
        /// </summary>
        public int TimeOut { get; set; } = 60000;

        /// <summary>
        /// Header设置
        /// </summary>
        public WebHeaderCollection Headers { get; set; } = new();

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Referer
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// Origin
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus Status { get; private set; } = TaskStatus.Initial;

        /// <summary>
        /// 发生的错误
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// 下载管理器
        /// </summary>
        public DownloadManager Manager { get; set; }

        CancellationTokenSource TimerToken;
        readonly Stream DownloadStream;
        readonly Timer Timer;
        long _lastReceivedBytes = 0;

        public DownloadTask(string url)
        {
            Url = url;
            Host = new Uri(Url).Host;
            DownloadStream = new MemoryStream();
            Timer = new(1000) { AutoReset = true };
            Timer.Elapsed += (s, e) =>
            {
                if (TimerToken == null || TimerToken.IsCancellationRequested || Status != TaskStatus.Download)
                    return;
                var db = TotalReceivedBytes - _lastReceivedBytes;
                _lastReceivedBytes = TotalReceivedBytes;
                OnTaskDownloadSpeedUpdated?.Invoke(this, new(db));
            };
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public void Start()
        {
            if (Status == TaskStatus.Initial)
            {
                Status = TaskStatus.Waiting;
                TimerToken = new();
                StartDownload();
                OnTaskWaiting?.Invoke(this);
            }
        }

        /// <summary>
        /// 暂停下载
        /// </summary>
        public void Pause()
        {
            if (Status == TaskStatus.Waiting || Status == TaskStatus.Connect || Status == TaskStatus.Download)
            {
                Status = TaskStatus.Pause;
                TimerToken?.Cancel();
                Timer.AutoReset = false;
                OnTaskPaused?.Invoke(this);
            }
        }

        /// <summary>
        /// 恢复下载
        /// </summary>
        public void Resume()
        {
            if (Status == TaskStatus.Pause || Status == TaskStatus.Error)
            {
                Status = TaskStatus.Waiting;
                TimerToken = new();
                Timer.AutoReset = true;
                StartDownload();
                OnTaskResume?.Invoke(this);
            }
        }

        /// <summary>
        /// 取消下载
        /// </summary>
        public void Cancel()
        {
            if (Status != TaskStatus.Cancel)
            {
                Status = TaskStatus.Cancel;
                TimerToken?.Cancel();
                Timer.AutoReset = false;
                OnTaskCancel?.Invoke(this);
            }
        }

        bool _threadWorking = false;
        void StartDownload()
        {
            if (!_threadWorking)
            {
                _threadWorking = true;
                new Thread(DownloadThread)
                {
                    IsBackground = true
                }.Start();
            }
        }

        HttpWebRequest GetWebRequest()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(Url);
            wr.ServerCertificateValidationCallback = delegate { return true; };
            wr.Host = Host;
            wr.UserAgent = UserAgent;
            wr.Timeout = TimeOut;
            wr.Headers = Headers;
            if (!string.IsNullOrWhiteSpace(Referer))
                wr.Referer = Referer;
            if (!string.IsNullOrWhiteSpace(Origin))
                wr.Headers.Add("Origin", Origin);
            return wr;
        }

        readonly byte[] buffer = new byte[1024];
        int ReceiveData(Stream stream)
        {
            try
            {
                int osize = stream.Read(buffer, 0, buffer.Length);
                if (osize > 0)
                {
                    DownloadStream.Write(buffer, 0, osize);
                    TotalReceivedBytes += osize;
                }
                return osize;
            }
            catch (Exception ex)
            {
                Status = TaskStatus.Error;
                Exception = ex;
                OnTaskError?.Invoke(this, new(ex));
            }
            return -1;
        }

        byte[] PackageData()
        {
            DownloadStream.Seek(0, SeekOrigin.Begin);
            return ((MemoryStream)DownloadStream).ToArray();
        }

        void DownloadThread()
        {
            try
            {
                Status = TaskStatus.Connect;
                var wr = GetWebRequest();
                var response = wr.GetResponse();
                TotalBytes = response.ContentLength;
                Stream st = response.GetResponseStream();
                st.Seek(TotalReceivedBytes, SeekOrigin.Begin);
                Status = TaskStatus.Download;
                Timer.Start();
                int osize = -1;
                while (Status == TaskStatus.Download && (osize = ReceiveData(st)) > 0)
                    OnTaskDownloadProgressUpdated?.Invoke(this, new(TotalReceivedBytes, TotalBytes, Progress));
                st.Close();
                if (Status == TaskStatus.Download && osize == 0)
                {
                    Status = TaskStatus.Finish;
                    TimerToken?.Cancel();
                    OnTaskFinish?.Invoke(this, new(PackageData()));
                }
            }
            catch (Exception ex)
            {
                Status = TaskStatus.Error;
                Exception = ex;
                OnTaskError?.Invoke(this, new(ex));
            }
            finally
            {
                _threadWorking = false;
            }
        }

        #region 事件
        public delegate void EventAsyncCallBackHandler(DownloadTask task);
        public delegate void EventAsyncCallBackHandler<in TEventArgs>(DownloadTask task, TEventArgs eventArgs)
            where TEventArgs : EventArgs;

        public class DownloadProgressEventArgs : EventArgs
        {
            public long TotalReceivedBytes { get; init; }
            public long TotalBytes { get; init; }
            public double Progress { get; init; }

            public DownloadProgressEventArgs(long received, long total, double progress)
            {
                TotalReceivedBytes = received;
                TotalBytes = total;
                Progress = progress;
            }
        }

        public class DownloadSpeedEventArgs : EventArgs
        {
            public long ReceivedBytes { get; init; }

            public DownloadSpeedEventArgs(long bytes)
            {
                ReceivedBytes = bytes;
            }
        }

        public class DownloadResultEventArgs : EventArgs
        {
            public bool IsSuccess { get; init; }
            public byte[] DownloadBytes { get; init; }
            public Exception Exception { get; init; }

            public DownloadResultEventArgs(byte[] bytes)
            {
                IsSuccess = true;
                DownloadBytes = bytes;
            }

            public DownloadResultEventArgs(Exception ex)
            {
                IsSuccess = false;
                Exception = ex;
            }
        }

        public event EventAsyncCallBackHandler OnTaskWaiting;
        public event EventAsyncCallBackHandler OnTaskConnecting;
        public event EventAsyncCallBackHandler<DownloadProgressEventArgs> OnTaskDownloadProgressUpdated;
        public event EventAsyncCallBackHandler<DownloadSpeedEventArgs> OnTaskDownloadSpeedUpdated;
        public event EventAsyncCallBackHandler OnTaskPaused;
        public event EventAsyncCallBackHandler OnTaskResume;
        public event EventAsyncCallBackHandler OnTaskCancel;
        public event EventAsyncCallBackHandler<DownloadResultEventArgs> OnTaskFinish;
        public event EventAsyncCallBackHandler<DownloadResultEventArgs> OnTaskError;
        #endregion
    }

    public enum TaskStatus
    {
        /// <summary>
        /// 初始状态
        /// </summary>
        Initial,
        /// <summary>
        /// 等待开始
        /// </summary>
        Waiting,
        /// <summary>
        /// 连接中
        /// </summary>
        Connect,
        /// <summary>
        /// 下载中
        /// </summary>
        Download,
        /// <summary>
        /// 暂停中
        /// </summary>
        Pause,
        /// <summary>
        /// 已完成
        /// </summary>
        Finish,
        /// <summary>
        /// 已取消
        /// </summary>
        Cancel,
        /// <summary>
        /// 发生错误
        /// </summary>
        Error
    }
}
