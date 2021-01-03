using Newtonsoft.Json;
using Ritsukage.Commands;
using Sora.Server;
using Sora.Tool;
using System;
using System.Threading.Tasks;

namespace Ritsukage
{
    class Program
    {
        public static SoraWSServer Server { get; private set; }

        static async Task Main(string[] args)
        {
            Console.Title = "Ritsukage Core";
            ConsoleLog.Info("Main", "Loading...");
            await Launch();
            Shutdown();
            ConsoleLog.Info("Main", "程序主逻辑已结束，按任意键结束程序");
            Console.ReadKey();
        }

        static async Task Launch()
        {
            var cfg = Config.LoadConfig();
#if DEBUG
            ConsoleLog.SetLogLevel(Fleck.LogLevel.Debug);
            ConsoleLog.Debug("Main", "当前正在使用Debug模式");
#else
            if (cfg.IsDebug)
            {
                ConsoleLog.SetLogLevel(Fleck.LogLevel.Debug);
                ConsoleLog.Debug("Main", "当前正在使用Debug模式");
            }
            else
                ConsoleLog.SetLogLevel(Fleck.LogLevel.Info);
#endif
            ConsoleLog.Debug("Main", "Config:\r\n" + JsonConvert.SerializeObject(cfg, Formatting.Indented));
            var config = new ServerConfig()
            {
                Location = cfg.Host,
                Port = cfg.Port,
                AccessToken = cfg.AccessToken,
                HeartBeatTimeOut = cfg.HeartBeatTimeOut
            };
            try
            {
                Server = new(config);
                CombineEvent(Server);
                CommandManager.RegisterAllCommands();
                await Server.StartServer();
            }
            catch
            {
            }
        }

        static void Shutdown()
        {
            try
            {
                Server?.Dispose();
            }
            catch
            {
            }
        }

        static void CombineEvent(SoraWSServer server)
        {
            #region Server Connection Event
            server.ConnManager.OnOpenConnectionAsync += async (s, e) =>
            {
                ConsoleLog.Debug("Socket", $"New connection created from {s.ClientIpAddress}:{s.ClientPort} with {e.SelfId} {e.Role}");
                await Task.CompletedTask;
            };
            server.ConnManager.OnHeartBeatTimeOut += async (s, e) =>
            {
                ConsoleLog.Debug("Socket", $"Heartbeat timeout from {s.ClientIpAddress}:{s.ClientPort} with {e.SelfId} {e.Role}");
                await Task.CompletedTask;
            };
            server.ConnManager.OnCloseConnectionAsync += async (s, e) =>
            {
                ConsoleLog.Debug("Socket", $"Connection closed from {s.ClientIpAddress}:{s.ClientPort} with {e.SelfId} {e.Role}");
                await Task.CompletedTask;
            };
            server.Event.OnClientConnect += async (s, e) =>
            {
                ConsoleLog.Info("Socket", $"[{e.LoginUid}] Client type: {e.ClientType} {e.ClientVersionCode} connected.");
                await Task.CompletedTask;
            };
            #endregion
            #region Message Event
            server.Event.OnGroupMessage += async (s, e) =>
            {
                if (e.IsAnonymousMessage)
                    ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Group:{e.SourceGroup.Id}] <匿名>{e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");
                else
                    ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Group:{e.SourceGroup.Id}] {e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");
                try
                {
                    await Task.Run(() => CommandManager.ReceiveMessage(e));
                }
                catch (Exception ex)
                {
                    ConsoleLog.ErrorLogBuilder(ex);
                }
            };
            server.Event.OnPrivateMessage += async (s, e) =>
            {
                ConsoleLog.Info(e.EventName, $"[{e.LoginUid}] {e.SenderInfo.Nick}({e.SenderInfo.UserId}): {e.Message}");
                try
                {
                    await Task.Run(() => CommandManager.ReceiveMessage(e));
                }
                catch (Exception ex)
                {
                    ConsoleLog.ErrorLogBuilder(ex);
                }
            };
            #endregion
        }
    }
}