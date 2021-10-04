using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Ritsukage.Tools.Console;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Services
{
    [Service]
    public class CommandHandling
    {
        readonly CommandService _commands;
        readonly DiscordSocketClient _discord;
        readonly IServiceProvider _services;

        public CommandHandling(IServiceProvider services)
        {
            _services = services;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _discord.MessageReceived += MessageReceivedAsync;
            _commands = services.GetRequiredService<CommandService>();
            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.Log += LogCommandAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage msg) return;
            if (msg.Author.Id == _discord.CurrentUser.Id || msg.Author.IsBot || msg.Author.IsWebhook) return;
            int pos = 0;
            if (msg.HasCharPrefix('+', ref pos))
            {
                _ = _commands.ExecuteAsync(new SocketCommandContext(_discord, msg), pos, _services);
            }
            await Task.CompletedTask;
        }

#pragma warning disable CA1822 // 将成员标记为 static
        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
#pragma warning restore CA1822 // 将成员标记为 static
        {
            if (!command.IsSpecified)
                return;
            if (result.IsSuccess)
                return;
            await context.Channel.SendMessageAsync($"执行指令时发生错误: {result}");
        }

        Task LogCommandAsync(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    ConsoleLog.Error("Discord Commands", $"[{msg.Source}] " + msg.Message.ToString());
                    if (msg.Severity == LogSeverity.Error)
                        ConsoleLog.Error("Discord Commands", $"[{msg.Source}] " + ConsoleLog.ErrorLogBuilder(msg.Exception));
                    break;
                case LogSeverity.Warning:
                    ConsoleLog.Warning("Discord Commands", $"[{msg.Source}] " + msg.Message.ToString());
                    break;
                case LogSeverity.Info:
                    ConsoleLog.Info("Discord Commands", $"[{msg.Source}] " + msg.Message.ToString());
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    ConsoleLog.Debug("Discord Commands", $"[{msg.Source}] " + msg.Message.ToString());
                    break;
                default:
                    break;
            };
            return Task.CompletedTask;
        }
    }
}
