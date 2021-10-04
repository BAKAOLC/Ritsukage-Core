using Sora.EventArgs.SoraEvent;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    public class OnlyForSuperUserAttribute : PreconditionAttribute
    {
        public override Task<bool> CheckPermissionsAsync(BaseSoraEventArgs args)
        {
            if (args is GroupMessageEventArgs gm)
                return Task.FromResult(gm.Sender.Id == Program.Config.QQSuperUser);
            else if (args is PrivateMessageEventArgs pm)
                return Task.FromResult(pm.Sender.Id == Program.Config.QQSuperUser);
            else
                return Task.FromResult(false);
        }

        public override string ToString()
            => $"<Limit for super user>";
    }
}
