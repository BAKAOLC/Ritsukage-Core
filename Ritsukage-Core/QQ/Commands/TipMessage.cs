using Ritsukage.Library.Service;
using System;
using System.Linq;
using System.Text;
using static Ritsukage.Library.Data.TipMessage;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Tip Message"), CanWorkIn(WorkIn.Group)]
    public static class TipMessage
    {
        [Command("tip")]
        [CommandDescription("添加提示信息")]
        [ParameterDescription(1, "提示时间")]
        [ParameterDescription(2, "提示文本")]
        [ParameterDescription(3, "提示间隔")]
        [ParameterDescription(4, "结束时间")]
        public static async void AddTip(SoraMessage e, DateTime time, string message, TimeSpan interval, DateTime endTime)
        {
            message = SoraMessage.Escape(message);
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
                await TipMessageService.AddTipMessage(TipTargetType.QQGroup,
                    e.SourceGroup, time, message, true, interval, endTime);
                await e.ReplyToOriginal("已添加多次提示信息");
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal(ex.Message);
            }
        }

        [Command("tip")]
        [CommandDescription("添加提示信息")]
        [ParameterDescription(1, "提示时间")]
        [ParameterDescription(2, "提示文本")]
        public static async void AddTip(SoraMessage e, DateTime time, string message)
        {
            message = SoraMessage.Escape(message);
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
                await TipMessageService.AddTipMessage(TipTargetType.QQGroup, e.SourceGroup, time, message);
                await e.ReplyToOriginal("已添加单次提示信息");
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal(ex.Message);
            }
        }

        [Command("tiplist")]
        [CommandDescription("查看本群所有的提示信息")]
        public static async void TipList(SoraMessage e)
        {
            var list = await TipMessageService.GetTipMessages(TipTargetType.QQGroup, e.SourceGroup.Id);
            if (list.Length > 0)
            {
                var sb = new StringBuilder("本群现有的所有的提醒项目：");
                foreach (var tip in list)
                {
                    sb.AppendLine().AppendLine($"[ID:{tip.Id}]");
                    if (tip.Duplicate)
                    {
                        sb.AppendLine($"下一次提醒时间：{tip.TipTime:yyyy-MM-dd HH:mm:ss}");
                        sb.AppendLine($"提醒间隔：{tip.Interval.Days}天{tip.Interval.Hours}时{tip.Interval.Minutes}分{tip.Interval.Seconds}秒");
                        sb.AppendLine($"提醒结束于：{tip.EndTime:yyyy-MM-dd HH:mm:ss}");
                    }
                    else
                        sb.AppendLine($"提醒时间：{tip.TipTime:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine("提醒内容：");
                    sb.Append(tip.Message);
                }
                await e.Reply(sb.ToString());
            }
            else
                await e.Reply("本群目前不存在提示信息");
        }

        [Command("tipremove")]
        [CommandDescription("移除指定的提示信息")]
        [ParameterDescription(1, "提示信息ID")]
        public static async void TipRemove(SoraMessage e, int id)
        {
            var tip = await TipMessageService.GetTipMessageById(id);
            if (tip != null && tip.TargetType == TipTargetType.QQGroup && tip.TargetID == e.SourceGroup.Id)
            {
                await tip.DeleteAsync();
                await e.ReplyToOriginal("操作成功");
                return;
            }
            await e.ReplyToOriginal($"不存在ID为 {id} 的提示消息");
        }
    }
}
