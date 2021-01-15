using Sora.EventArgs.SoraEvent;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    public class OnlyForUserAttribute : PreconditionAttribute
    {
        public long[] Users { get; init; }

        public OnlyForUserAttribute(params long[] users)
        {
            Users = users;
        }

        public override Task<bool> CheckPermissionsAsync(BaseSoraEventArgs args)
        {
            if (args is GroupMessageEventArgs a1)
                return Task.FromResult(Users.Where(x => x == a1.Sender.Id)?.Count() > 0);
            else if (args is PrivateMessageEventArgs a2)
                return Task.FromResult(Users.Where(x => x == a2.Sender.Id)?.Count() > 0);
            else
                return Task.FromResult(false);
        }
    }
}
