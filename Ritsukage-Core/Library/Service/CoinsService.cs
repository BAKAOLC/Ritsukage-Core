using Ritsukage.Library.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Library.Service
{
    public static class CoinsService
    {
        private const long DailyFreeCoins = 30;
        private const long BaseCoins = 0;

        public static async Task<UserCoins> GetUserCoins(string type, long userid)
        {
            var date = DateTime.Now.Date;
            var data = type switch
            {
                "qq" => await Database.FindAsync<UserData>(x => x.QQ == userid),
                "discord" => await Database.FindAsync<UserData>(x => x.Discord == userid),
                "bilibili" => await Database.FindAsync<UserData>(x => x.Bilibili == userid),
                _ => throw new("不支持的用户来源：" + type),
            };
            if (data == null)
            {
                data = new()
                {
                    Coins = BaseCoins,
                    FreeCoins = DailyFreeCoins,
                    FreeCoinsDate = date,
                };
                switch (type)
                {
                    case "qq":
                        data.QQ = userid;
                        break;
                    case "discord":
                        data.Discord = userid;
                        break;
                    case "bilibili":
                        data.Bilibili = Convert.ToInt32(userid);
                        break;
                    default:
                        throw new("不支持的用户来源：" + type);
                }

                await Database.InsertAsync(data);
            }
            else
            {
                if (data.FreeCoinsDate != date)
                {
                    data.FreeCoins = DailyFreeCoins;
                    data.FreeCoinsDate = date;
                }

                await Database.UpdateAsync(data);
            }

            return new() { Coins = data.Coins, FreeCoins = data.FreeCoins };
        }

        public static async Task<bool> CheckUserCoins(string type, long userid, long count, bool disableFree)
        {
            var c = await GetUserCoins(type, userid);
            if (disableFree)
                return c.Coins >= count;
            else
                return c.Total >= count;
        }

        public static async Task<UserCoins> AddUserCoins(string type, long userid, long count)
        {
            var date = DateTime.Now.Date;
            var data = type switch
            {
                "qq" => await Database.FindAsync<UserData>(x => x.QQ == userid),
                "discord" => await Database.FindAsync<UserData>(x => x.Discord == userid),
                "bilibili" => await Database.FindAsync<UserData>(x => x.Bilibili == userid),
                _ => throw new("不支持的用户来源：" + type),
            };
            if (data == null)
            {
                data = new()
                {
                    Coins = BaseCoins,
                    FreeCoins = DailyFreeCoins,
                    FreeCoinsDate = date,
                };
                switch (type)
                {
                    case "qq":
                        data.QQ = userid;
                        break;
                    case "discord":
                        data.Discord = userid;
                        break;
                    case "bilibili":
                        data.Bilibili = Convert.ToInt32(userid);
                        break;
                    default:
                        throw new("不支持的用户来源：" + type);
                }

                await Database.InsertAsync(data);
            }
            else
            {
                if (data.FreeCoinsDate != date)
                {
                    data.FreeCoins = DailyFreeCoins;
                    data.FreeCoinsDate = date;
                }
            }

            data.Coins += count;
            await Database.UpdateAsync(data);
            return new() { Coins = data.Coins, FreeCoins = data.FreeCoins };
        }

        public static async Task<UserCoins> RemoveUserCoins(string type, long userid, long count,
            bool disableFree = false)
        {
            var date = DateTime.Now.Date;
            var data = type switch
            {
                "qq" => await Database.FindAsync<UserData>(x => x.QQ == userid),
                "discord" => await Database.FindAsync<UserData>(x => x.Discord == userid),
                "bilibili" => await Database.FindAsync<UserData>(x => x.Bilibili == userid),
                _ => throw new("不支持的用户来源：" + type),
            };
            if (data == null)
            {
                data = new()
                {
                    Coins = BaseCoins,
                    FreeCoins = DailyFreeCoins,
                    FreeCoinsDate = date,
                };
                switch (type)
                {
                    case "qq":
                        data.QQ = userid;
                        break;
                    case "discord":
                        data.Discord = userid;
                        break;
                    case "bilibili":
                        data.Bilibili = Convert.ToInt32(userid);
                        break;
                    default:
                        throw new("不支持的用户来源：" + type);
                }

                await Database.InsertAsync(data);
            }
            else
            {
                if (data.FreeCoinsDate != date)
                {
                    data.FreeCoins = DailyFreeCoins;
                    data.FreeCoinsDate = date;
                }
            }

            if (disableFree)
            {
                data.Coins -= count;
            }
            else
            {
                if (data.FreeCoins >= count)
                {
                    data.FreeCoins -= count;
                }
                else
                {
                    data.Coins -= count - data.FreeCoins;
                    data.FreeCoins = 0;
                }
            }

            await Database.UpdateAsync(data);
            return new() { Coins = data.Coins, FreeCoins = data.FreeCoins };
        }
    }

    public struct UserCoins
    {
        public long Coins { get; set; }
        public long FreeCoins { get; set; }

        public long Total => Coins + FreeCoins;
    }
}