using Discord.WebSocket;
using Ritsukage.Library.Service;
using System;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public static class Exceptions
    {
        public static async Task<UserCoins> GetCoins(this SocketUser user)
            => await CoinsService.GetUserCoins("discord", Convert.ToInt64(user.Id));

        public static async Task<bool> CheckCoins(this SocketUser user, long count, bool disableFree = false)
            => await CoinsService.CheckUserCoins("discord", Convert.ToInt64(user.Id), count, disableFree);

        public static async Task<UserCoins> AddCoins(this SocketUser user, long count)
            => await CoinsService.AddUserCoins("discord", Convert.ToInt64(user.Id), count);

        public static async Task<UserCoins> RemoveCoins(this SocketUser user, long count, bool disableFree = false)
            => await CoinsService.RemoveUserCoins("discord", Convert.ToInt64(user.Id), count, disableFree);
    }
}
