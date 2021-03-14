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
                        var t = await Database.Data.Table<CooldownQQ>().ToArrayAsync();
                        CooldownQQ data = t.Where(x => x.QQ == userid && x.Tag == tag
                        && x.IsGroup == group).FirstOrDefault();
                        if (data == null)
                        {
                            data = new()
                            {
                                IsGroup = group,
                                Tag = tag,
                                QQ = userid,
                                LastUsed = Utils.BaseUTC
                            };
                            await Database.Data.InsertAsync(data);
                            return true;
                        }
                        else
                        {
                            return (date - data.LastUsed).TotalSeconds > seconds;
                        }
                    }
                case "discord":
                    {
                        var t = await Database.Data.Table<CooldownDiscord>().ToArrayAsync();
                        CooldownDiscord data = t.Where(x => x.Discord == userid && x.Tag == tag
                        && x.IsChannel == group).FirstOrDefault();
                        if (data == null)
                        {
                            data = new()
                            {
                                IsChannel = group,
                                Tag = tag,
                                Discord = userid,
                                LastUsed = Utils.BaseUTC
                            };
                            await Database.Data.InsertAsync(data);
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
                        var t = await Database.Data.Table<CooldownQQ>().ToArrayAsync();
                        CooldownQQ data = t.Where(x => x.QQ == userid && x.Tag == tag
                        && x.IsGroup == group).FirstOrDefault();
                        if (data == null)
                        {
                            data = new()
                            {
                                IsGroup = group,
                                Tag = tag,
                                QQ = userid,
                                LastUsed = date
                            };
                            await Database.Data.InsertAsync(data);
                        }
                        else
                        {
                            data.LastUsed = date;
                            await Database.Data.UpdateAsync(data);
                        }
                    }
                    break;
                case "discord":
                    {
                        var t = await Database.Data.Table<CooldownDiscord>().ToArrayAsync();
                        CooldownDiscord data = t.Where(x => x.Discord == userid && x.Tag == tag
                        && x.IsChannel == group).FirstOrDefault();
                        if (data == null)
                        {
                            data = new()
                            {
                                IsChannel = group,
                                Tag = tag,
                                Discord = userid,
                                LastUsed = Utils.BaseUTC
                            };
                            await Database.Data.InsertAsync(data);
                        }
                        else
                        {
                            data.LastUsed = date;
                            await Database.Data.UpdateAsync(data);
                        }
                    }
                    break;
                default:
                    throw new Exception("不支持的用户来源：" + type);
            }
        }
    }
}
