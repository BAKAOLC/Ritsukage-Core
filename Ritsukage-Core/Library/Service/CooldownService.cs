using Ritsukage.Library.Data;
using Ritsukage.Tools;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Library.Service
{
    public static class CooldownService
    {
        public static async Task<bool> CheckCooldown(string type,
            long userid, string tag, int seconds, bool group = false)
        {
            DateTime date = DateTime.Now;
            switch (type)
            {
                case "qq":
                    {
                        CooldownQQ data = await Database.FindAsync<CooldownQQ>(
                            x
                            => x.QQ == userid
                            && x.Tag == tag
                            && x.IsGroup == group);
                        if (data == null)
                        {
                            data = new()
                            {
                                IsGroup = group,
                                Tag = tag,
                                QQ = userid,
                                LastUsed = Utils.BaseUTC
                            };
                            await Database.InsertAsync(data);
                            return true;
                        }
                        else
                        {
                            return (date - data.LastUsed).TotalSeconds > seconds;
                        }
                    }
                case "discord":
                    {
                        CooldownDiscord data = await Database.FindAsync<CooldownDiscord>(
                            x
                            => x.Discord == userid
                            && x.Tag == tag
                            && x.IsChannel == group);
                        if (data == null)
                        {
                            data = new()
                            {
                                IsChannel = group,
                                Tag = tag,
                                Discord = userid,
                                LastUsed = Utils.BaseUTC
                            };
                            await Database.InsertAsync(data);
                            return true;
                        }
                        else
                        {
                            return (date - data.LastUsed).TotalSeconds > seconds;
                        }
                    }
                default:
                    throw new Exception("不支持的用户来源：" + type);
            }
        }

        public static async Task UpdateCooldown(string type, long userid, string tag, bool group = false)
        {
            DateTime date = DateTime.Now;
            switch (type)
            {
                case "qq":
                    {
                        CooldownQQ data = await Database.FindAsync<CooldownQQ>(
                            x
                            => x.QQ == userid
                            && x.Tag == tag
                            && x.IsGroup == group);
                        if (data == null)
                        {
                            data = new()
                            {
                                IsGroup = group,
                                Tag = tag,
                                QQ = userid,
                                LastUsed = date
                            };
                            await Database.InsertAsync(data);
                        }
                        else
                        {
                            data.LastUsed = date;
                            await Database.UpdateAsync(data);
                        }
                    }
                    break;
                case "discord":
                    {
                        CooldownDiscord data = await Database.FindAsync<CooldownDiscord>(
                            x
                            => x.Discord == userid
                            && x.Tag == tag
                            && x.IsChannel == group);
                        if (data == null)
                        {
                            data = new()
                            {
                                IsChannel = group,
                                Tag = tag,
                                Discord = userid,
                                LastUsed = Utils.BaseUTC
                            };
                            await Database.InsertAsync(data);
                        }
                        else
                        {
                            data.LastUsed = date;
                            await Database.UpdateAsync(data);
                        }
                    }
                    break;
                default:
                    throw new Exception("不支持的用户来源：" + type);
            }
        }
    }
}
