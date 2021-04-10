using Ritsukage.QQ.Commands;
using Ritsukage.QQ.Events;
using Ritsukage.QQ.Service;
using Ritsukage.Tools.Console;
using Sora.Entities.Base;
using Sora.Net;
using Sora.OnebotModel;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ritsukage.QQ
{
    public class QQService
    {
        public SoraWSServer Server { get; private set; }

        public QQService(ServerConfig config)
        {
            CommandManager.Init();
            EventManager.Init();
            ServiceManager.Init();
            Server = new(config);
            CombineEvent(Server);
        }

        readonly ConcurrentDictionary<long, SoraApi> Apis = new();
        public long[] GetBotList() => Apis.Select(x => x.Key)?.ToArray();
        public SoraApi GetSoraApi(long bot)
        {
            if (Apis.TryGetValue(bot, out var api))
                return api;
            return null;
        }

        public void Start()
        {
            new Thread(async () =>
            {
                try
                {
                    await Server.StartServer();
                }
                catch (Exception e)
                {
                    ConsoleLog.Error("Sora", ConsoleLog.ErrorLogBuilder(e));
                }
            })
            {
                IsBackground = true
            }.Start();
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

        void CombineEvent(SoraWSServer server)
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

                if (!Apis.ContainsKey(e.SenderInfo.UserId))
                    await Task.Run(() => CommandManager.ReceiveMessage(e));
            };
            server.Event.OnPrivateMessage += async (s, e) =>
            {
                ConsoleLog.Info(e.EventName, $"[{e.LoginUid}] {e.SenderInfo.Nick}({e.SenderInfo.UserId}): {e.Message}");

                if (!Apis.ContainsKey(e.SenderInfo.UserId))
                    await Task.Run(() => CommandManager.ReceiveMessage(e));
            };
            server.Event.OnSelfMessage += async (s, e) =>
            {
                if (e.IsAnonymousMessage)
                    ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Group:{e.SourceGroup.Id}] <匿名>{e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");
                else
                    ConsoleLog.Info(e.EventName, $"[{e.LoginUid}][Group:{e.SourceGroup.Id}] {e.SenderInfo.Card}({e.SenderInfo.UserId}): {e.Message}");
                await ValueTask.CompletedTask;
            };
            #endregion
            #region Event Manager
            server.Event.OnClientConnect += (s, e) =>
            {
                Apis[e.LoginUid] = e.SoraApi;
                ConsoleLog.Debug("Sora", $"Update api for bot {e.LoginUid}");
                return ValueTask.CompletedTask;
            };
            server.Event.OnClientConnect += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
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
            server.Event.OnOfflineFileEvent += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            server.Event.OnPrivateMessage += async (s, e) => await Task.Run(() => EventManager.Trigger(s, e));
            #endregion
        }
    }
}
