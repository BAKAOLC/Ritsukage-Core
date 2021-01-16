using Ritsukage.Library.Bilibili.Model;

namespace Ritsukage.Library.Subscribe.CheckResult
{
    public class BilibiliLiveCheckResult : Base.SubscribeCheckResult
    {
        public BilibiliLiveUpdateType UpdateType;

        public int RoomId { get; init; }

        public string Title { get; init; }

        public string User { get; init; }

        public string Area { get; init; }

        public int Online { get; init; }

        public LiveStatus Status { get; init; }

        public string Cover { get; init; }

        public string Url { get; init; }
    }

    public enum BilibiliLiveUpdateType
    {
        /// <summary>
        /// 没有变化
        /// </summary>
        None,

        /// <summary>
        /// 初始化
        /// </summary>
        Initialization,

        /// <summary>
        /// 更新了直播状态
        /// </summary>
        LiveStatus,

        /// <summary>
        /// 更新了直播标题
        /// </summary>
        Title
    }
}
