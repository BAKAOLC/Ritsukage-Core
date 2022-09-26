using AriaNet;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Acquisition.Aria
{
    public class AriaHttpAcquisition : Acquisition
    {
        #region Resource
        const string ExeFile = "aria2c.exe";

        static bool CheckResource()
        {
            if (File.Exists(ExeFile))
                return File.ReadAllBytes(ExeFile).SequenceEqual(Resource.aria2c);
            return false;
        }

        static void ReleaseResource()
            => File.WriteAllBytes(ExeFile, Resource.aria2c);
        #endregion

        #region Initialize
        static Process ariaProcess;
        static AriaManager manager;
        static long maxDownloadSpeed;
        static bool _init = false;

        private static readonly IPEndPoint DefaultLoopbackEndpoint = new(IPAddress.Loopback, port: 0);
        static int GetAvailablePort()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(DefaultLoopbackEndpoint);
            return ((IPEndPoint)socket.LocalEndPoint).Port;
        }

        public static async Task InitializeAsync(long maxDownloadSpeed = 0, string proxy = null)
        {
            AriaHttpAcquisition.maxDownloadSpeed = maxDownloadSpeed;

            if (_init) return;
            _init = true;

            if (ariaProcess == null || ariaProcess.HasExited)
            {
                var stray = Process.GetProcessesByName("aria2c");
                foreach (var process in stray)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                    }
                }

                var rng = new Random();
                var secret = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes($"{rng.Next()}{rng.Next()}{rng.Next()}{rng.Next()}")));

                if (!CheckResource())
                    ReleaseResource();

                var ariaPath = ExeFile;
                var ariaPort = GetAvailablePort();
                var ariaHost = $"http://localhost:{ariaPort}/jsonrpc";
                var ariaArgs = $"--enable-rpc --rpc-secret={secret} --rpc-listen-port={ariaPort} --log=\"aria2.log\" --log-level=notice --max-connection-per-server=8 --auto-file-renaming=false --allow-overwrite=true";
                if (!string.IsNullOrEmpty(proxy))
                {
                    ariaArgs += $" --all-proxy={proxy}";
                }
                var startInfo = new ProcessStartInfo(ariaPath, ariaArgs)
                {
                    UseShellExecute = false,
                };

                ariaProcess = Process.Start(startInfo);
                Thread.Sleep(400);
                if (ariaProcess == null)
                    throw new Exception("ariaProcess was null.");
                else if (ariaProcess.HasExited)
                    throw new Exception("ariaProcess has exited.");
                manager = new AriaManager(secret, ariaHost);
            }
        }

        public static async Task UnInitializeAsync()
        {
            if (!_init) return;
            _init = false;

            if (ariaProcess is { HasExited: false })
            {
                try
                {
                    await manager.Shutdown();
                }
                catch (Exception)
                {
                    // ignored
                }

                Thread.Sleep(1000);

                if (!ariaProcess.HasExited)
                    ariaProcess.Kill();
            }
        }
        #endregion

        string Gid;
        bool IsDownloading = false;

        public AriaHttpAcquisition(string url, string referer = null, string filename = null, string directory = null)
            : base(url, referer, filename, directory)
        { }

        public override async Task StartDownloadAsync()
        {
            if (IsDownloading) return;
            IsDownloading = true;

            var param = new Dictionary<string, string>()
            {
                {"user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/82.0.4056.0 Safari/537.36 Edg/82.0.431.0"},
                {"out", Filename},
                {"dir", Directory},
                {"max-connection-per-server", "8"},
                {"max-tries", "100"},
                {"max-download-limit", maxDownloadSpeed.ToString()},
                {"auto-file-renaming", "false"},
                {"allow-overwrite", "true"},
            };
            if (!string.IsNullOrEmpty(Referer))
                param.Add("referer", Referer);
            await manager.AddUri(new List<string>() { Url }, param).ContinueWith(async t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    IsDownloading = false;
                    OnCompleted(new AcquisitionCompletedEventArgs(AcquisitionResult.Error, TimeSpan.Zero, null, t.Exception));
                    return;
                }
                Gid = t.Result;
                DateTime begin = DateTime.Now;
                bool first = true;
                var _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            var status = await manager.GetStatus(Gid);
                            if (first)
                            {
                                first = false;
                                OnDownloadStarted(new AcquisitionStartedEventArgs(Filename, long.Parse(status.TotalLength)));
                            }
                            var timespan = DateTime.Now - begin;
                            switch (status.Status)
                            {
                                case "complete":
                                    var fileStream = File.OpenRead(Path.Combine(Directory, Filename));
                                    var ms = new MemoryStream();
                                    var buffer = new byte[4096];
                                    int osize;
                                    while ((osize = fileStream.Read(buffer, 0, 4096)) > 0)
                                        ms.Write(buffer, 0, osize);
                                    fileStream.Close();
                                    fileStream.Dispose();
                                    ms.Seek(0, SeekOrigin.Begin);
                                    IsDownloading = false;
                                    OnCompleted(new AcquisitionCompletedEventArgs(AcquisitionResult.Success, timespan, ms));
                                    return;
                                case "removed":
                                    IsDownloading = false;
                                    OnCompleted(new AcquisitionCompletedEventArgs(AcquisitionResult.Cancelled, timespan));
                                    return;
                                case "error":
                                    IsDownloading = false;
                                    OnCompleted(new AcquisitionCompletedEventArgs(AcquisitionResult.Error, timespan));
                                    return;
                                default:
                                    var total = long.Parse(status.TotalLength);
                                    var received = long.Parse(status.CompletedLength);
                                    var speed = long.Parse(status.DownloadSpeed);
                                    OnProgressChanged(new AcquisitionProgressEventArgs(total, received, speed, timespan));
                                    break;
                            }
                        }
                        catch
                        {
                        }
                        Thread.Sleep(500);
                    }
                });
            });
        }

        public override async Task CancelAsync()
        {
            if (!IsDownloading) return;
            IsDownloading = false;
            await manager.RemoveTask(Gid, true);
        }

        public override async Task WaitForDownloadCompleted()
        {
            if (!IsDownloading) return;
            await Task.Run(() =>
            {
                while (IsDownloading)
                    Thread.Sleep(500);
            });
        }
    }
}