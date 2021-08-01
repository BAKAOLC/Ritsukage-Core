using Discord.WebSocket;
using Ritsukage.Library.Service;
using System;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public static class Extension
    {
        public static async Task<UserCoins> GetCoins(this SocketUser user)
            => await CoinsService.GetUserCoins("discord", Convert.ToInt64(user.Id));

        public static async Task<bool> CheckCoins(this SocketUser user, long count, bool disableFree = false)
            => await CoinsService.CheckUserCoins("discord", Convert.ToInt64(user.Id), count, disableFree);

        public static async Task<UserCoins> AddCoins(this SocketUser user, long count)
            => await CoinsService.AddUserCoins("discord", Convert.ToInt64(user.Id), count);

        public static async Task<UserCoins> RemoveCoins(this SocketUser user, long count, bool disableFree = false)
            => await CoinsService.RemoveUserCoins("discord", Convert.ToInt64(user.Id), count, disableFree);

        public static async Task<bool> CheckCooldown(this SocketUser user, string tag, int seconds)
            => await CooldownService.CheckCooldown("discord", Convert.ToInt64(user.Id), tag, seconds, false);

        public static async Task UpdateCooldown(this SocketUser user, string tag)
            => await CooldownService.UpdateCooldown("discord", Convert.ToInt64(user.Id), tag, false);

        public static async Task<bool> CheckCooldown(this ISocketMessageChannel channel, string tag, int seconds)
            => await CooldownService.CheckCooldown("discord", Convert.ToInt64(channel.Id), tag, seconds, true);

        public static async Task UpdateCooldown(this ISocketMessageChannel channel, string tag)
            => await CooldownService.UpdateCooldown("discord", Convert.ToInt64(channel.Id), tag, true);
    }
}
