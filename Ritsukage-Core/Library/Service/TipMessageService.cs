using Ritsukage.Library.Data;
using System;
using System.Threading.Tasks;

namespace Ritsukage.Library.Service
{
    public static class TipMessageService
    {
        public static async Task AddTipMessage(TipMessage.TipTargetType type, long id,
            DateTime time, string message, bool duplicate, TimeSpan interval, DateTime endTime)
        {
            if (time < DateTime.Now)
                throw new ArgumentOutOfRangeException(nameof(time), "不可以使用早于当前的时间");
            if (duplicate && endTime < time)
                throw new ArgumentOutOfRangeException(nameof(time), "结束时间不可以比开始时间早");
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("不可使用空消息进行提醒", nameof(message));
            await new TipMessage()
            {
                TargetType = type,
                TargetID = id,
                TipTime = time,
                Message = message,
                Duplicate = duplicate,
                Interval = interval,
                EndTime = endTime
            }.InsertAsync();
        }

        public static async Task AddTipMessage(TipMessage.TipTargetType type, long id, DateTime time, string message)
            => await AddTipMessage(type, id, time, message, false, TimeSpan.Zero, time);

        public static async Task<TipMessage> GetTipMessageById(int id)
            => await Database.GetAsync<TipMessage>(x => x.Id == id);

        public static async Task<TipMessage[]> GetTipMessages(TipMessage.TipTargetType type, long targetID)
            => await Database.GetArrayAsync<TipMessage>(x => x.TargetType == type && x.TargetID == targetID);

        public static async Task<TipMessage[]> GetTipMessages(TipMessage.TipTargetType type, DateTime now)
            => await Database.GetArrayAsync<TipMessage>(x => x.TargetType == type && x.TipTime <= now);

        public static async Task RefreshTipMessages(DateTime now)
        {
            var needUpdate = await Database.GetArrayAsync<TipMessage>(x => x.TipTime <= now && x.Duplicate && x.EndTime > now);
            foreach (var target in needUpdate)
            {
                while (target.TipTime <= now)
                    target.TipTime += target.Interval;
                if (target.TipTime >= target.EndTime)
                {
                    target.Duplicate = false;
                    if (target.TipTime > target.EndTime)
                        target.TipTime -= target.Interval;
                }
            }
            await Database.UpdateAllAsync(needUpdate);
            await Database.DeleteAll<TipMessage>(x => x.TipTime <= now);
        }
    }
}
