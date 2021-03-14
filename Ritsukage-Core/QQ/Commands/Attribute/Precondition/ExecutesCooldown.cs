using Ritsukage.Library.Service;
using Sora.EventArgs.SoraEvent;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    public class ExecutesCooldown : PreconditionAttribute
    {
        public string Tag { get; init; }
        public int Seconds { get; init; }
        public bool IsGroup { get; init; }

        public ExecutesCooldown(string tag, int seconds, bool isGroup = false)
        {
            Tag = tag;
            Seconds = seconds;
            IsGroup = isGroup;
        }

        public override async Task<bool> CheckPermissionsAsync(BaseSoraEventArgs args)
        {
            if (!IsGroup)
            {
                long user;
                if (args is GroupMessageEventArgs a)
                    user = a.Sender.Id;
                else if (args is PrivateMessageEventArgs b)
                    user = b.Sender.Id;
                else
                    return false;
                return await CooldownService.CheckCooldown("qq", user, Tag, Seconds, false);
            }
            else if (args is GroupMessageEventArgs a)
                return await CooldownService.CheckCooldown("qq", a.SourceGroup.Id, Tag, Seconds, true);
            else
                return true;
        }
    }
}
