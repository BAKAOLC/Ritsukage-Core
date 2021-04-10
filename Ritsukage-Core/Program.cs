using Newtonsoft.Json;
using Ritsukage.Discord;
using Ritsukage.Library.Data;
using Ritsukage.Library.Subscribe;
using Ritsukage.QQ;
using Ritsukage.Tools.Console;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Ritsukage
{
    class Program
    {
        public static QQService QQServer { get; private set; }
        public static DiscordAPP DiscordServer { get; private set; }

        public static Config Config { get; private set; }

        public static bool Working = false;

        static DateTime LaunchTime;

#pragma warning disable IDE0060 // 删除未使用的参数
        static void Main(string[] args)
#pragma warning restore IDE0060 // 删除未使用的参数
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var directory = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "crash-report"));
                var now = DateTime.Now;
                File.WriteAllText(
                    Path.Combine(directory.FullName, $"crash-{now:yyyyMMdd-HHmmss-ffff}.log"),
                    new StringBuilder()
                    .AppendLine($"[启动于 {LaunchTime:yyyy-MM-dd HH:mm:ss.ffff}]")
                    .AppendLine($"[崩溃于 {now:yyyy-MM-dd HH:mm:ss.ffff}]")
                    .AppendLine($"[工作时长 {now - LaunchTime}]")
                    .Append(ConsoleLog.ErrorLogBuilder((Exception)args.ExceptionObject))
                    .ToString());
            };
            LaunchTime = DateTime.Now;
            Console.Title = "Ritsukage Core";
            ConsoleLog.Info("Main", "Loading...");
            Launch();
            while (Working)
            {
                UpdateTitle();
                Thread.Sleep(100);
            }
            Shutdown();
            ConsoleLog.Info("Main", "程序主逻辑已结束，按任意键结束程序");
            Console.ReadKey();
        }

        static void UpdateTitle() => Console.Title = $"Ritsukage Core | 启动于 {LaunchTime:yyyy-MM-dd HH:mm:ss} | 运行时长 {DateTime.Now - LaunchTime}"
            + (Config.IsDebug ? " | DEBUG MODE" : string.Empty);

        static void Launch()
        {
            var cfg = Config = Config.LoadConfig();
#if DEBUG
            ConsoleLog.SetLogLevel(LogLevel.Debug);
            ConsoleLog.Debug("Main", "当前正在使用Debug模式");
#else
            if (cfg.IsDebug)
            {
                ConsoleLog.SetLogLevel(LogLevel.Debug);
                ConsoleLog.Debug("Main", "当前正在使用Debug模式");
            }
            else
                ConsoleLog.SetLogLevel(LogLevel.Info);
#endif
            ConsoleLog.Debug("Main", "Config:\r\n" + JsonConvert.SerializeObject(cfg, Formatting.Indented));

            ConsoleLog.Info("Main", "初始化数据库中……");
            Database.Init(cfg.DatabasePath);
            ConsoleLog.Info("Main", "数据库已装载");

            ConsoleLog.Info("Main", "订阅系统启动中……");
            SubscribeManager.Init();
            ConsoleLog.Info("Main", "订阅系统已装载");

            if (!string.IsNullOrWhiteSpace(Config.Roll_Api_Id) && !string.IsNullOrWhiteSpace(Config.Roll_Api_Secret))
            {
                Library.Roll.RollApi.Init(Config.Roll_Api_Id, Config.Roll_Api_Secret);
                ConsoleLog.Info("Main", "Roll Api 已初始化");
            }

            if (cfg.Discord)
            {
                Working = true;
                ConsoleLog.Info("Main", "已启用Discord功能");
                new Thread(() =>
                {
                    try
                    {
                        DiscordServer = new(cfg.DiscordToken);
                        DiscordServer.Start();
                    }
                    catch (Exception ex)
                    {
                        ConsoleLog.Fatal("Main", "Discord功能启动失败");
                        ConsoleLog.Error("Main", ConsoleLog.ErrorLogBuilder(ex));
                        Working = false;
                    }
                })
                {
                    IsBackground = true
                }.Start();
            }

            if (cfg.QQ)
            {
                Working = true;
                ConsoleLog.Info("Main", "已启用QQ功能");
                new Thread(() =>
                {
                    try
                    {
                        QQServer = new(new()
                        {
                            Location = cfg.Host,
                            Port = cfg.Port,
                            AccessToken = cfg.AccessToken,
                            HeartBeatTimeOut = cfg.HeartBeatTimeOut,
                            EnableSoraCommandManager = false
                        });
                        QQServer.Start();
                    }
                    catch (Exception ex)
                    {
                        ConsoleLog.Fatal("Main", "QQ功能启动失败");
                        ConsoleLog.Error("Main", ConsoleLog.ErrorLogBuilder(ex));
                        Working = false;
                    }
                })
                {
                    IsBackground = true
                }.Start();
            }
        }

        static void Shutdown()
        {
            try
            {
                QQServer?.Stop();
            }
            catch
            {
            }
            try
            {
                DiscordServer?.Stop();
            }
            catch
            {
            }
        }
    }
}