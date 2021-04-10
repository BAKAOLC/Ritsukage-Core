using Ritsukage.Library.Service;
using System;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Tip Message"), CanWorkIn(WorkIn.Group)]
    public static class TipMessage
    {
        [Command("tip")]
        public static async void AddTip(SoraMessage e, DateTime time, string message, TimeSpan interval, DateTime endTime)
        {
            var now = DateTime.Now;
            if (time < now)
            {
                await e.ReplyToOriginal("提示时间不可设置为过去的时间");
                return;
            }
            else if ((time - now).TotalSeconds < 300)
            {
                await e.ReplyToOriginal("提示时间不可设置为 5 分钟内的目标");
                return;
            }
            else if (interval.TotalSeconds < 60)
            {
                await e.ReplyToOriginal("提示间隔不可以短于 1 分钟");
                return;
            }
            try
            {
                await TipMessageService.AddTipMessage(Library.Data.TipMessage.TipTargetType.QQGroup,
                    e.SourceGroup, time, message, true, interval, endTime);
                await e.ReplyToOriginal("已添加多次提示信息");
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal(ex.Message);
            }
        }

        [Command("tip")]
        public static async void AddTip(SoraMessage e, DateTime time, string message)
        {
            var now = DateTime.Now;
            if (time < now)
            {
                await e.ReplyToOriginal("提示时间不可设置为过去的时间");
                return;
            }
            else if ((time - now).TotalSeconds < 300)
            {
                await e.ReplyToOriginal("提示时间不可设置为 5 分钟内的目标");
                return;
            }
            try
            {
                await TipMessageService.AddTipMessage(Library.Data.TipMessage.TipTargetType.QQGroup, e.SourceGroup, time, message);
                await e.ReplyToOriginal("已添加单次提示信息");
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal(ex.Message);
            }
        }
    }
}
