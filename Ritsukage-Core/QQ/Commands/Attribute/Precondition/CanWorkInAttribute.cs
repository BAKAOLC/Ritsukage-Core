using Sora.EventArgs.SoraEvent;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    public class CanWorkInAttribute : PreconditionAttribute
    {
        public WorkIn CanWork { get; init; } = WorkIn.All;

        public CanWorkInAttribute(WorkIn type)
        {
            CanWork = type;
        }

        public override Task<bool> CheckPermissionsAsync(BaseSoraEventArgs args) => CanWork switch
        {
            WorkIn.Group => Task.FromResult(args is GroupMessageEventArgs),
            WorkIn.Private => Task.FromResult(args is PrivateMessageEventArgs),
            _ => Task.FromResult(true),
        };
    }

    public enum WorkIn
    {
        Group,
        Private,
        All
    }
}
