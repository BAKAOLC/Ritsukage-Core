using Discord;
using Discord.WebSocket;
using Ritsukage.Library.Data;
using Ritsukage.Library.Subscribe.CheckMethod;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.QQ;
using Ritsukage.Tools.Console;
using Sora.Entities.CQCodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.Listener
{
    public class BilibiliDynamicListener : Base.SubscribeListener
    {
        const string type = "bilibili dynamic";

        readonly List<BilibiliDynamicCheckMethod> Checker = new();

        public override async void RefreshListener()
        {
            var list = await Database.GetArrayAsync<SubscribeList>(x => x.Type == type);
            if (list == null)
            {
                Checker.Clear();
                return;
            }
            foreach (var c in Checker.ToArray())
            {
                if (!list.Where(x => x.Target == c.UserId.ToString()).Any())
                    Checker.Remove(c);
            }
            foreach (var l in list)
            {
                if (!Checker.Where(x => l.Target == x.UserId.ToString()).Any())
                {
                    if (int.TryParse(l.Target, out var id))
                    {
                        ConsoleLog.Debug("Subscribe", $"Start subscribe listener: {type} {id}");
                        Checker.Add(new(id));
                    }
                }
            }
        }

        public override async void Listen()
        {
            await Task.Run(async () =>
            {
                foreach (var c in Checker.ToArray())
                {
                    Broadcast(await c.Check());
                    await Task.Delay(10 * 1000);
                }
            });
        }

        public override async void Broadcast(CheckResult.Base.SubscribeCheckResult result)
        {
            if (result.Updated && result is BilibiliDynamicCheckResult b)
            {
                ConsoleLog.Debug("Subscribe", $"Boardcast updated info for {type} {b.User.Id}");
                var records = await Database.GetArrayAsync<SubscribeList>(x => x.Type == type && x.Target == b.User.Id.ToString());
                if (records != null && records.Length > 0)
                {
                    if (Program.Config.QQ)
                    {
                        var qqmsg = await GetQQMessageChain(b);
                        var bots = Program.QQServer.GetBotList();
                        var qqgroups = records.Where(x => x.Platform == "qq group")?.Select(x => x.Listener)?.ToArray();
                        if (qqgroups != null && qqgroups.Length > 0)
                        {
                            foreach (var qqgroup in qqgroups)
                            {
                                if (long.TryParse(qqgroup, out var group))
                                {
                                    ConsoleLog.Debug("Subscribe", $"Boardcast updated info for group {group}");
                                    foreach (var bot in bots)
                                    {
                                        _ = Task.Factory.StartNew(async () =>
                                        {
                                            var api = Program.QQServer.GetSoraApi(bot);
                                            if (await api.CheckHasGroup(group))
                                            {
                                                ConsoleLog.Debug("Subscribe", $"Boardcast updated info for group {group} with bot {bot}");
                                                foreach (var m in qqmsg)
                                                    await api.SendGroupMessage(group, m);
                                            }
                                        });
                                    }
                                }
                            }
                        }
                    }
                    if (Program.Config.Discord && Program.DiscordServer.Client.ConnectionState == ConnectionState.Connected)
                    {
                        var dcmsg = await GetDiscordMessageChain(b);
                        var channels = records.Where(x => x.Platform == "discord channel")?.Select(x => x.Listener)?.ToArray();
                        if (channels != null && channels.Length > 0)
                        {
                            foreach (var id in channels)
                            {
                                if (ulong.TryParse(id, out var cid))
                                {
                                    _ = Task.Factory.StartNew(async () =>
                                      {
                                          ConsoleLog.Debug("Subscribe", $"Boardcast updated info to discord channel {cid}");
                                          try
                                          {
                                              var channel = (SocketTextChannel)Program.DiscordServer.Client.GetChannel(cid);
                                              foreach (var m in dcmsg)
                                              {
                                                  await channel?.SendMessageAsync(m);
                                              }
                                          }
                                          catch
                                          {
                                          }
                                      });
                                }
                            }
                        }
                    }
                }
            }
        }

        static async Task<object[][]> GetQQMessageChain(BilibiliDynamicCheckResult result)
        {
            List<object[]> records = new();
            foreach (var dynamic in result.Dynamics)
            {
                ArrayList msg = new();
                foreach (var pic in dynamic.Pictures)
                {
                    msg.Add(CQCode.CQImage(pic));
                    msg.Add(Environment.NewLine);
                }
                msg.Add(dynamic.BaseToString());
                records.Add(msg.ToArray());
                await Task.Delay(3000);
            }
            return records.ToArray();
        }

        static async Task<string[]> GetDiscordMessageChain(BilibiliDynamicCheckResult result)
        {
            List<string> records = new();
            foreach (var dynamic in result.Dynamics)
            {
                records.Add(dynamic.ToString());
                await Task.Delay(3000);
            }
            return records.ToArray();
        }
    }
}
