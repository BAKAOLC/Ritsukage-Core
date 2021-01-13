using Sora.EventArgs.SoraEvent;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Commands
{
    public class OnlyForGroupAttribute : PreconditionAttribute
    {
        public long[] Groups { get; init; }

        public OnlyForGroupAttribute(params long[] groups)
        {
            Groups = groups;
        }

        public override Task<bool> CheckPermissionsAsync(BaseSoraEventArgs args)
        {
            if (args is GroupMessageEventArgs a)
                return Task.FromResult(Groups.Where(x => x == a.SourceGroup.Id)?.Count() > 0);
            else
                return Task.FromResult(false);
        }
    }
}
