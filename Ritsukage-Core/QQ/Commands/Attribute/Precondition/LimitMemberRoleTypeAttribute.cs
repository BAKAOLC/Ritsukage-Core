using Sora.Enumeration.EventParamsType;
using Sora.EventArgs.SoraEvent;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    public class LimitMemberRoleTypeAttribute : PreconditionAttribute
    {
        public MemberRoleType Role { get; init; }

        public LimitMemberRoleTypeAttribute(MemberRoleType role)
        {
            Role = role;
        }

        public override Task<bool> CheckPermissionsAsync(BaseSoraEventArgs args)
        {
            if (args is GroupMessageEventArgs a)
                return Task.FromResult(a.Sender.Id == Program.Config.QQSuperUser || GroupList.GetMemberInfo(a.SourceGroup.Id, a.Sender.Id).Role >= Role);
            else
                return Task.FromResult(false);
        }
    }
}
