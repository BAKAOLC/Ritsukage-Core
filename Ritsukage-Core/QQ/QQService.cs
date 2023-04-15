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
using System.Threading;
using System.Threading.Tasks;

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

        readonly ConcurrentDictionary<long, Guid> Connection = new();
        public long[] GetBotList() => Connection.Select(x => x.Key)?.ToArray();
        public SoraApi GetSoraApi(long bot)
        {
            if (Connection.TryGetValue(bot, out var guid))
                return Server.GetApi(guid);
            return null;
        }

        public async void Start()
        {
            try
            {
                await Server.StartService();
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

        void CombineEvent(SoraWebsocketServer server)
        {
            #region Server Connection Event
            server.ConnManager.OnOpenConnectionAsync += async (s, e) =>
            {
                Connection[e.SelfId] = s;
                ConsoleLog.Debug("Socket", $"New connection created with {e.SelfId} {e.Role}");
                await Task.CompletedTask;
            };
            server.ConnManager.OnCloseConnectionAsync += async (s, e) =>
            {
                Connection.TryRemove(e.SelfId, out _);
                ConsoleLog.Debug("Socket", $"Connection closed with {e.SelfId} {e.Role}");
                await Task.CompletedTask;
            };
            #endregion
            #region Base Event
            server.Event.OnClientConnect += (s, e) =>
            {
                ConsoleLog.Info("Socket", $"[{e.LoginUid}] Client type: {e.ClientType} {e.ClientVersionCode} connected.");
                Connection[e.LoginUid] = e.ConnId;
                return ValueTask.CompletedTask;
            };
            #endregion
            #region Message Event
            server.Event.OnGroupMessage += async (s, e) =>
            {
                if (e.IsAnonymousMessage)
                    ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Receive({e.Message.MessageId})]{Environment.NewLine}[Group:{e.SourceGroup.Id}] <匿名>{e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");
                else
                    ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Receive({e.Message.MessageId})]{Environment.NewLine}[Group:{e.SourceGroup.Id}] {e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");

                if (!Connection.ContainsKey(e.SenderInfo.UserId))
                    await Task.Run(() => CommandManager.ReceiveMessage(e));
            };
            server.Event.OnPrivateMessage += async (s, e) =>
            {
                ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Receive({e.Message.MessageId})]{Environment.NewLine}{e.SenderInfo.Nick}({e.SenderInfo.UserId}): {e.Message}");

                if (!Connection.ContainsKey(e.SenderInfo.UserId))
                    await Task.Run(() => CommandManager.ReceiveMessage(e));
            };
            server.Event.OnSelfGroupMessage += (s, e) =>
            {
                if (e.IsAnonymousMessage)
                    ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Send({e.Message.MessageId})]{Environment.NewLine}[Group:{e.SourceGroup.Id}] <匿名>{e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");
                else
                    ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Send({e.Message.MessageId})]{Environment.NewLine}[Group:{e.SourceGroup.Id}] {e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");
                return ValueTask.CompletedTask;
            };
            server.Event.OnSelfPrivateMessage += (s, e) =>
            {
                ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Receive({e.Message.MessageId})]{Environment.NewLine}{e.SenderInfo.Nick}({e.SenderInfo.UserId}): {e.Message}");
                return ValueTask.CompletedTask;
            };
            #endregion
            #region Event Manager
            server.Event.OnClientConnect += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnClientStatusChangeEvent += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnFileUpload += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnFriendAdd += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnFriendRecall += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnFriendRequest += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnGroupAdminChange += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnGroupCardUpdate += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnGroupMemberChange += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnGroupMemberMute += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnGroupMessage += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnGroupPoke += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnGroupRecall += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnGroupRequest += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnHonorEvent += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnLuckyKingEvent += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnTitleUpdate += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnEssenceChange += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnOfflineFileEvent += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnPrivateMessage += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnSelfGroupMessage += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e, true));
            server.Event.OnSelfPrivateMessage += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e, true));
            #endregion
        }
    }
}
