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

namespace Ritsukage.Tools.Download
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

            [JsonIgnore]
            public bool Exists => File.Exists(Path);

            public CacheData(string url, string path, DateTime cacheTime)
            {
                Url = url;
                Path = path;
                CacheTime = cacheTime;
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
            => File.WriteAllText(CacheRecordFile, JsonConvert.SerializeObject(CacheDataList.Values));

        static void DebugLog(string text)
            => ConsoleLog.Debug("Downloader", text);

        public static async Task<string> Download(string url, string referer = null)
        {
            Init();

            #region 检查缓存
            while (DownloadingList.Contains(url))
                await Task.Delay(100);

            if (CacheDataList.TryGetValue(url, out var cache))
            {
                if (cache.Exists)
                    return cache.Path;
                else
                    CacheDataList.TryRemove(url, out _);
            }
            #endregion

            DownloadingList.Add(url);

            #region 下载
            var config = new DownloadConfiguration()
            {
                BufferBlockSize = 4096,
                ChunkCount = 5,
                OnTheFlyDownload = false,
                ParallelDownload = true
            };
            if (!string.IsNullOrWhiteSpace(referer))
            {
                config.RequestConfiguration = new RequestConfiguration()
                {
                    Referer = referer
                };
            };
            var downloader = new DownloadService(config);
            downloader.DownloadStarted += (s, e)
                => DebugLog($"Start to download file from {url} ({e.TotalBytesToReceive} bytes)");
            DateTime _lastUpdate = DateTime.Now;
            downloader.DownloadProgressChanged += (s, e) =>
            {
                var now = DateTime.Now;
                if ((now - _lastUpdate).TotalSeconds > 3)
                {
                    DebugLog($"Downloading {url}... {e.ReceivedBytesSize}/{e.TotalBytesToReceive} ({e.ProgressPercentage:F2}%)");
                    _lastUpdate = now;
                }
            };
            downloader.DownloadFileCompleted += (s, e) =>
            {
                if (e.Error != null)
                    DebugLog($"Download {url} failed." + Environment.NewLine + e.Error.GetFormatString(true));
                else
                    DebugLog($"Download {url} completed.");
            };
            using var stream = await downloader.DownloadFileTaskAsync(url);
            if (stream == null)
                return null;
            stream.Seek(0, SeekOrigin.Begin);
            #endregion

            #region 储存
            var file = CacheFileName;
            stream.SaveToFile(file);
            CacheDataList.TryAdd(url, new CacheData(url, file, DateTime.Now));
            Save();
            #endregion

            DownloadingList.Remove(url);
            return file;
        }

        const int SaveBufferSize = 4096;
        static void SaveToFile(this Stream stream, string path)
        {
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var buffer = new byte[SaveBufferSize];
            var osize = 0;
            using var fileStream = File.OpenWrite(path);
            while ((osize = stream.Read(buffer, 0, SaveBufferSize)) > 0)
                fileStream.Write(buffer, 0, osize);
            fileStream.Close();
            fileStream.Dispose();
        }

        const int ClearCacheDelay = 1000 * 60 * 60;
        static void StartCacheCleanupThread()
        {
            new Thread(() =>
            {
                while (true)
                {
                    var now = DateTime.Now;
                    var date = now.Date.AddHours(now.Hour - 1);
                    var list = CacheDataList.Where(x => x.Value.CacheTime < date).Select(x => x.Key).ToArray();
                    foreach (var data in list)
                        CacheDataList.TryRemove(data, out _);
                    Thread.Sleep(ClearCacheDelay);
                }
            })
            {
                IsBackground = true
            }.Start();
        }
    }
}
