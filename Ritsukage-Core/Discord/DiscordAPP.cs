using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Ritsukage.Discord.Services;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Ritsukage.Discord
{
    public class DiscordAPP
    {
        public DiscordSocketClient Client { get; private set; }
        public IServiceProvider Service { get; private set; }
        public CommandService Command { get; private set; }

        readonly string _token;

        public DiscordAPP(string token)
        {
            _token = token;
            Service = ConfigureServices();
            Client = Service.GetRequiredService<DiscordSocketClient>();
            Client.Log += LogClientAsync;
            Client.Ready += ReadyAsync;
            Client.MessageReceived += MessageReceivedAsync;
            Command = Service.GetRequiredService<CommandService>();
            _ = Service.GetRequiredService<CommandHandling>().InitializeAsync();
        }

        public void Start() => new Thread(RunThread)
        {
            IsBackground = true
        }.Start();

        public void Stop()
        {
            Client?.LogoutAsync();
            Client?.Dispose();
        }

        async void RunThread()
        {
            bool repeat = true;
            while (repeat)
            {
                try
                {
                    repeat = false;
                    ConsoleLog.Info("Discord", "开始尝试登陆");
                    await Client.LoginAsync(TokenType.Bot, _token);
                    await Client.StartAsync();
                }
                catch (Exception e)
                {
                    repeat = true;
                    ConsoleLog.Error("Discord", ConsoleLog.ErrorLogBuilder(e));
                    ConsoleLog.Info("Discord", "已断开连接，五秒后将重新登陆");
                    Thread.Sleep(5000);
                }
            }
        }

        static IServiceProvider ConfigureServices()
        {
            var discordSocketClient = new DiscordSocketClient(new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All,
#if DEBUG
                    LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Info
#endif
            });
            var interactionService = new InteractionService(discordSocketClient, new()
            {
#if DEBUG
                    LogLevel = LogSeverity.Debug
#else
                LogLevel = LogSeverity.Info
#endif
            });
            var map = new ServiceCollection()
                .AddSingleton(discordSocketClient)
                .AddSingleton(new CommandService(new()
                {
#if DEBUG
                    LogLevel = LogSeverity.Debug,
#else
                    LogLevel = LogSeverity.Info,
#endif
                    CaseSensitiveCommands = false,
                }))
                .AddSingleton(interactionService)
                .AddSingleton<Tools.Rand>();
            Type[] types = Assembly.GetEntryAssembly().GetExportedTypes();
            Type[] cosType = types.Where(t => Attribute.GetCustomAttributes(t, true).Where(a => a is ServiceAttribute).Any())?.ToArray() ?? Array.Empty<Type>();
            foreach (var service in cosType)
                map.AddSingleton(service);
            var provider = map.BuildServiceProvider();
            foreach (var service in cosType)
                provider.GetRequiredService(service);
            return provider;
        }

        Task LogClientAsync(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    ConsoleLog.Fatal("Discord", $"[{msg.Source}] " + msg.Message.ToString());
                    if (msg.Severity == LogSeverity.Error)
                        ConsoleLog.Error("Discord", $"[{msg.Source}] " + ConsoleLog.ErrorLogBuilder(msg.Exception));
                    break;
                case LogSeverity.Warning:
                    ConsoleLog.Warning("Discord", $"[{msg.Source}] " + msg.Message.ToString());
                    break;
                case LogSeverity.Info:
                    ConsoleLog.Info("Discord", $"[{msg.Source}] " + msg.Message.ToString());
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    ConsoleLog.Debug("Discord", $"[{msg.Source}] " + msg.Message.ToString());
                    break;
                default:
                    break;
            };
            return Task.CompletedTask;
        }

        Task ReadyAsync()
        {
            ConsoleLog.Info("Discord", "连接成功，BOT账户：" + Client.CurrentUser);
            return Task.CompletedTask;
        }

        Task MessageReceivedAsync(SocketMessage msg)
        {
            if (msg is SocketUserMessage sum)
            {
                var mc = new SocketCommandContext(Client, sum);
                if (msg.Author.Id != Client.CurrentUser.Id)
                    ConsoleLog.Info("Discord", $"{mc.Guild} > {mc.Channel} > {sum.Author}: " + sum.ToString());
            }
            else if (msg is SocketSystemMessage ssm)
            {
                ConsoleLog.Info("Discord", "系统消息: " + ssm.ToString());
            }
            return Task.CompletedTask;
        }
    }
}