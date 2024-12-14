using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using AriaNet;

namespace Acquisition.Aria
{
    public class AriaHttpAcquisition(
        string url,
        string? referer = null,
        string? filename = null,
        string? directory = null)
        : Acquisition(url, referer, filename, directory)
    {
        private string? _gid;
        private bool _isDownloading;

        public override async Task StartDownloadAsync()
        {
            if (_isDownloading) return;
            _isDownloading = true;

            var param = new Dictionary<string, string>
            {
                {
                    "user-agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/82.0.4056.0 Safari/537.36 Edg/82.0.431.0"
                },
                { "out", Filename },
                { "dir", Directory },
                { "max-connection-per-server", "8" },
                { "max-tries", "100" },
                { "max-download-limit", _maxDownloadSpeed.ToString() },
                { "auto-file-renaming", "false" },
                { "allow-overwrite", "true" },
            };
            if (!string.IsNullOrEmpty(Referer))
                param.Add("referer", Referer);
            if (_manager != null)
                await _manager.AddUri([Url], param).ContinueWith(async t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        _isDownloading = false;
                        if (t.Exception != null)
                            OnCompleted(new(AcquisitionResult.Error, TimeSpan.Zero, null, t.Exception));
                        return;
                    }

                    _gid = await t.ConfigureAwait(false);
                    var begin = DateTime.Now;
                    var first = true;
                    var _ = Task.Run(async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                var status = await _manager.GetStatus(_gid).ConfigureAwait(false);
                                if (first)
                                {
                                    first = false;
                                    OnDownloadStarted(new(Filename, long.Parse(status.TotalLength)));
                                }

                                var timespan = DateTime.Now - begin;
                                switch (status.Status)
                                {
                                    case "complete":
                                        var fileStream = File.OpenRead(Path.Combine(Directory, Filename));
                                        var ms = new MemoryStream();
                                        var buffer = new byte[4096];
                                        int oSize;
                                        while ((oSize = fileStream.Read(buffer, 0, 4096)) > 0)
                                            ms.Write(buffer, 0, oSize);
                                        fileStream.Close();
                                        await fileStream.DisposeAsync().ConfigureAwait(false);
                                        ms.Seek(0, SeekOrigin.Begin);
                                        _isDownloading = false;
                                        OnCompleted(new(AcquisitionResult.Success, timespan, ms));
                                        return;
                                    case "removed":
                                        _isDownloading = false;
                                        OnCompleted(new(AcquisitionResult.Cancelled, timespan));
                                        return;
                                    case "error":
                                        _isDownloading = false;
                                        OnCompleted(new(AcquisitionResult.Error, timespan));
                                        return;
                                    default:
                                        var total = long.Parse(status.TotalLength);
                                        var received = long.Parse(status.CompletedLength);
                                        var speed = long.Parse(status.DownloadSpeed);
                                        OnProgressChanged(new(total, received, speed, timespan));
                                        break;
                                }
                            }
                            catch
                            {
                                // ignored
                            }

                            Thread.Sleep(500);
                        }
                    });
                }).ConfigureAwait(false);
        }

        public override async Task CancelAsync()
        {
            if (!_isDownloading) return;
            _isDownloading = false;
            if (_gid != null && _manager != null)
                await _manager.RemoveTask(_gid, true).ConfigureAwait(false);
        }

        public override async Task WaitForDownloadCompletedAsync()
        {
            if (!_isDownloading) return;
            await Task.Run(() =>
            {
                while (_isDownloading)
                    Thread.Sleep(500);
            }).ConfigureAwait(false);
        }

        #region Resource

        private const string ExeFile = "aria2c.exe";

        private static bool CheckResource()
        {
            if (File.Exists(ExeFile))
                return File.ReadAllBytes(ExeFile).SequenceEqual(Resource.aria2c);
            return false;
        }

        private static void ReleaseResource()
        {
            File.WriteAllBytes(ExeFile, Resource.aria2c);
        }

        #endregion

        #region Initialize

        private static Process? _ariaProcess;
        private static AriaManager? _manager;
        private static long _maxDownloadSpeed;
        private static bool _init;

        private static readonly IPEndPoint DefaultLoopbackEndpoint = new(IPAddress.Loopback, 0);

        private static int GetAvailablePort()
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(DefaultLoopbackEndpoint);
            return ((IPEndPoint)socket.LocalEndPoint!).Port;
        }

        public static Task InitializeAsync(long maxDownloadSpeed = 0, string? proxy = null)
        {
            _maxDownloadSpeed = maxDownloadSpeed;

            if (_init) return Task.CompletedTask;
            _init = true;

            if (_ariaProcess is { HasExited: false }) return Task.CompletedTask;
            var stray = Process.GetProcessesByName("aria2c");
            foreach (var process in stray)
                try
                {
                    process.Kill();
                }
                catch
                {
                    // ignored
                }

            var rng = new Random();
            var secret = BitConverter.ToString(MD5.Create()
                .ComputeHash(Encoding.UTF8.GetBytes($"{rng.Next()}{rng.Next()}{rng.Next()}{rng.Next()}")));

            if (!CheckResource())
                ReleaseResource();

            var ariaPort = GetAvailablePort();
            var ariaHost = $"http://localhost:{ariaPort}/jsonrpc";
            var ariaArgs =
                $"--enable-rpc --rpc-secret={secret} --rpc-listen-port={ariaPort} --log=\"aria2.log\" --log-level=notice --max-connection-per-server=8 --auto-file-renaming=false --allow-overwrite=true";
            if (!string.IsNullOrEmpty(proxy)) ariaArgs += $" --all-proxy={proxy}";
            var startInfo = new ProcessStartInfo(ExeFile, ariaArgs)
            {
                UseShellExecute = false,
            };

            _ariaProcess = Process.Start(startInfo);
            Thread.Sleep(400);
            if (_ariaProcess == null)
                throw new("ariaProcess was null.");
            if (_ariaProcess.HasExited)
                throw new("ariaProcess has exited.");
            _manager = new(secret, ariaHost);

            return Task.CompletedTask;
        }

        public static async Task UnInitializeAsync()
        {
            if (!_init) return;
            _init = false;

            if (_ariaProcess is { HasExited: false })
            {
                try
                {
                    if (_manager != null) await _manager.Shutdown().ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }

                Thread.Sleep(1000);

                if (!_ariaProcess.HasExited)
                    _ariaProcess.Kill();
            }
        }

        #endregion
    }
}