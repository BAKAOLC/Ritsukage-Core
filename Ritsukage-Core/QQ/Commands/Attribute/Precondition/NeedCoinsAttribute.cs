using Ritsukage.Library.Service;
using Sora.EventArgs.SoraEvent;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    public class NeedCoinsAttribute : PreconditionAttribute
    {
        public long Coins { get; init; }
        public bool DisableFree { get; init; }

        public NeedCoinsAttribute(long coins, bool disableFree = false)
        {
            Coins = coins;
            DisableFree = disableFree;
        }

        public override async Task<bool> CheckPermissionsAsync(BaseSoraEventArgs args)
        {
            long user;
            if (args is GroupMessageEventArgs a)
                user = a.Sender.Id;
            else if (args is PrivateMessageEventArgs b)
                user = b.Sender.Id;
            else
                return false;
            return await CoinsService.CheckUserCoins("qq", user, Coins, DisableFree);
        }

        public override string ToString()
            => $"<Need coins: {Coins}{(DisableFree ? " (not free)" : "")}>";
    }
}
