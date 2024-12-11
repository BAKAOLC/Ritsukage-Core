using Ritsukage.QQ.Commands;
using Ritsukage.QQ.Events;
using Ritsukage.QQ.Service;
using Ritsukage.Tools.Console;
using Sora;
using Sora.Entities.Base;
using Sora.Net;
using Sora.Net.Config;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sora.EventArgs.SoraEvent;

namespace Ritsukage.QQ
{
    public class QQService
    {
        public SoraWebsocketServer Server { get; private set; }

        public QQService(ServerConfig config)
        {
            CommandManager.Init();
            EventManager.Init();
            ServiceManager.Init();
            Server = (SoraWebsocketServer)SoraServiceFactory.CreateService(config, _ => { });
            CombineEvent(Server);
        }

        private readonly ConcurrentDictionary<long, Guid> _connection = [];

        public long[] GetBotList()
        {
            return _connection.Select(x => x.Key)?.ToArray();
        }

        public SoraApi GetSoraApi(long bot)
        {
            return _connection.TryGetValue(bot, out var guid) ? Server.GetApi(guid) : null;
        }

        public async void Start()
        {
            try
            {
                await Server.StartService().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ConsoleLog.Error("Sora", ConsoleLog.ErrorLogBuilder(e));
            }
        }

        public void Stop()
        {
            try
            {
                Server?.Dispose();
            }
            catch
            {
            }
        }

        private void CombineEvent(SoraWebsocketServer server)
        {
            #region Server Connection Event

            server.ConnManager.OnOpenConnectionAsync += async (s, e) =>
            {
                _connection[e.SelfId] = s;
                ConsoleLog.Debug("Socket", $"New connection created with {e.SelfId} {e.Role}");
                await Task.CompletedTask.ConfigureAwait(false);
            };
            server.ConnManager.OnCloseConnectionAsync += async (s, e) =>
            {
                _connection.TryRemove(e.SelfId, out _);
                ConsoleLog.Debug("Socket", $"Connection closed with {e.SelfId} {e.Role}");
                await Task.CompletedTask.ConfigureAwait(false);
            };

            #endregion

            #region Base Event

            server.Event.OnClientConnect += (s, e) =>
            {
                ConsoleLog.Info("Socket",
                    $"[{e.LoginUid}] Client type: {e.ClientType} {e.ClientVersionCode} connected.");
                _connection[e.LoginUid] = e.ConnId;
                return ValueTask.CompletedTask;
            };

            #endregion

            #region Message Event

            server.Event.OnGroupMessage += async (s, e) =>
            {
                ConsoleLog.Info(e.EventName,
                    e.IsAnonymousMessage
                        ? $"[{e.LoginUid}][Receive({e.Message.MessageId})]{Environment.NewLine}[Group:{e.SourceGroup.Id}] <匿名>{e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}"
                        : $"[{e.LoginUid}][Receive({e.Message.MessageId})]{Environment.NewLine}[Group:{e.SourceGroup.Id}] {e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");

                if (!_connection.ContainsKey(e.SenderInfo.UserId))
                    await Task.Run(() =>
                    {
                        try
                        {
                            CommandManager.ReceiveMessage(e);
                        }
                        catch (Exception ex)
                        {
                            ConsoleLog.Error("Command Manager", new StringBuilder()
                                .AppendLine("Command Manager Error")
                                .AppendLine($"Event Type\t: {e.GetType()}")
                                .AppendLine($"Event Info\t: {e}")
                                .Append($"Exception\t: {ex.GetFormatString(true)}"));

                            var sb = new StringBuilder()
                                .AppendLine("处理消息时发生异常：")
                                .AppendLine(ex.Message)
                                .Append("请向 BOT 开发反馈此问题");
                        }
                    }).ConfigureAwait(false);
            };
            server.Event.OnPrivateMessage += async (s, e) =>
            {
                ConsoleLog.Info(e.EventName,
                    $"[{e.LoginUid}][Receive({e.Message.MessageId})]{Environment.NewLine}{e.SenderInfo.Nick}({e.SenderInfo.UserId}): {e.Message}");

                if (!_connection.ContainsKey(e.SenderInfo.UserId))
                    await Task.Run(() =>
                    {
                        try
                        {
                            CommandManager.ReceiveMessage(e);
                        }
                        catch (Exception ex)
                        {
                            ConsoleLog.Error("Command Manager", new StringBuilder()
                                .AppendLine("Command Manager Error")
                                .AppendLine($"Event Type\t: {e.GetType()}")
                                .AppendLine($"Event Info\t: {e}")
                                .Append($"Exception\t: {ex.GetFormatString(true)}"));
                        }
                    }).ConfigureAwait(false);
            };
            server.Event.OnSelfGroupMessage += (s, e) =>
            {
                ConsoleLog.Info(e.EventName,
                    e.IsAnonymousMessage
                        ? $"[{e.LoginUid}][Send({e.Message.MessageId})]{Environment.NewLine}[Group:{e.SourceGroup.Id}] <匿名>{e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}"
                        : $"[{e.LoginUid}][Send({e.Message.MessageId})]{Environment.NewLine}[Group:{e.SourceGroup.Id}] {e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");
                return ValueTask.CompletedTask;
            };
            server.Event.OnSelfPrivateMessage += (s, e) =>
            {
                ConsoleLog.Info(e.EventName,
                    $"[{e.LoginUid}][Receive({e.Message.MessageId})]{Environment.NewLine}{e.SenderInfo.Nick}({e.SenderInfo.UserId}): {e.Message}");
                return ValueTask.CompletedTask;
            };

            #endregion

            #region Event Manager

            server.Event.OnClientConnect += async (s, e) => await Task.Run(() =>
            {
                try
                {
                    EventManager.Trigger(s, e);
                }
                catch (Exception ex)
                {
                    ConsoleLog.Error("Event Manager", new StringBuilder()
                        .AppendLine("Event Manager Error")
                        .AppendLine($"Event Type\t: {e.GetType()}")
                        .AppendLine($"Event Info\t: {e}")
                        .Append($"Exception\t: {ex.GetFormatString(true)}"));
                }
            }).ConfigureAwait(false);
            server.Event.OnClientStatusChangeEvent += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnFileUpload += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnFriendAdd += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnFriendRecall += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnFriendRequest += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnGroupAdminChange += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnGroupCardUpdate += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnGroupMemberChange += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnGroupMemberMute += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnGroupMessage += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnGroupPoke += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnGroupRecall += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnGroupRequest += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnHonorEvent += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnLuckyKingEvent += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnTitleUpdate += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnEssenceChange += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnOfflineFileEvent += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnPrivateMessage += async (s, e) => await TriggerEvent(s, e).ConfigureAwait(false);
            server.Event.OnSelfGroupMessage += async (s, e) => await TriggerEvent(s, e, true).ConfigureAwait(false);
            server.Event.OnSelfPrivateMessage += async (s, e) => await TriggerEvent(s, e, true).ConfigureAwait(false);

            #endregion
        }

        private Task TriggerEvent(string s, BaseSoraEventArgs e, bool fromSelf = false)
        {
            return Task.Run(() =>
            {
                try
                {
                    EventManager.Trigger(s, e, fromSelf);
                }
                catch (Exception ex)
                {
                    ConsoleLog.Error("Event Manager", new StringBuilder()
                        .AppendLine("Event Manager Error")
                        .AppendLine($"Event Type\t: {e.GetType()}")
                        .AppendLine($"Event Info\t: {e}")
                        .Append($"Exception\t: {ex.GetFormatString(true)}"));
                }
            });
        }
    }
}