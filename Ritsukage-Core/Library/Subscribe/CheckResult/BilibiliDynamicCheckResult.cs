using Ritsukage.Library.Bilibili.Model;

namespace Ritsukage.Library.Subscribe.CheckResult
{
    public class BilibiliDynamicCheckResult : Base.SubscribeCheckResult
    {
        public User User { get; init; }
        public Dynamic[] Dynamics { get; init; }
    }
}
