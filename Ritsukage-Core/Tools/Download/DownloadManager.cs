using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ritsukage.Tools.Download
{
    public class DownloadManager
    {
        /// <summary>
        /// 最大同时下载任务
        /// </summary>
        public int MaxThread { get; set; } = 5;

        /// <summary>
        /// 正在下载的任务
        /// </summary>
        public readonly List<DownloadTask> Downloading = new();

        readonly ConcurrentQueue<DownloadTask> QueueList = new();

        /// <summary>
        /// 添加新的下载任务
        /// </summary>
        public DownloadTask Add(DownloadTask task)
        {
            QueueList.Enqueue(task);
            return task;
        }

        /// <summary>
        /// 添加新的下载任务
        /// </summary>
        public DownloadTask Add(string url)
            => Add(new DownloadTask(url));

        /// <summary>
        /// 正在下载
        /// </summary>
        public bool Working { get; private set; } = false;

        Thread _managerThread;

        public void Start()
        {
            if (Working)
                return;
            Working = true;
            foreach (var task in Downloading)
                task.Resume();
            if (_managerThread == null)
                (_managerThread = new(ManagerThread) { IsBackground = true }).Start();
        }

        public void Stop()
        {
            if (!Working)
                return;
            Working = false;
            foreach (var task in Downloading)
                task.Pause();
            if (_managerThread != null)
            {
                _managerThread.Interrupt();
                _managerThread = null;
            }
        }

        void ManagerThread()
        {
            while (Working)
            {
                if (Downloading.Count < MaxThread)
                {
                    if (QueueList.TryDequeue(out var task))
                    {
                        task.Start();
                        task.OnTaskFinish += (s, e) =>
                        {
                            if (Downloading.Contains(task))
                                Downloading.Remove(task);
                        };
                        task.OnTaskError += (s, e) =>
                        {
                            if (Downloading.Contains(task))
                                Downloading.Remove(task);
                        };
                        Downloading.Add(task);
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }
    }
}
