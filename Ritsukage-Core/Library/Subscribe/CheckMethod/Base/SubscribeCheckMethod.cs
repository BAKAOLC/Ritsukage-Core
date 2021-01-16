using Ritsukage.Library.Subscribe.CheckResult.Base;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.CheckMethod.Base
{
    public abstract class SubscribeCheckMethod
    {
        public abstract Task<SubscribeCheckResult> Check();
    }
}
