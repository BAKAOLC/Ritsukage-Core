using Ritsukage.Tools;
using Sora.Entities.CQCodes;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Image Generator")]
    public static class Choyen
    {
        [Command("5000choyen"), NeedCoins(1)]
        public static async void Normal(SoraMessage e, string line1, string line2)
        {
            await e.Reply(CQCode.CQImage($"https://api.akiraxie.me/5000choyen?upper={Utils.UrlEncode(line1)}&lower={Utils.UrlEncode(line2)}"));
            await e.RemoveCoins(1);
        }
    }
}
