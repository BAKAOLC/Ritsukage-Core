using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Text;
using static Ritsukage.QQ.SoraMessage.AdditionalMethod;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils"), ExecutesCooldown("utils.choose", 10, true)]
    public static class Choose
    {
        static readonly Rand rnd = new();
        static bool _init = false;

        [Command("choose", "抉择")]
        [CommandDescription("从给定的列表中随机选择一项输出")]
        [ParameterDescription(1, "指定的选择列表")]
        public static async void ChooseOne(SoraMessage e, params string[] c)
        {
            if (!_init)
            {
                _init = true;
                rnd.Seed(Convert.ToUInt32(DateTime.UtcNow.Millisecond));
            }
            if (c.Length <= 1)
            {
                await e.ReplyToOriginal("参数不合法，请至少给出2项选择项");
                return;
            }
            await e.ReplyToOriginal("#抉择：", ToSoraSegment(c[rnd.Int(0, c.Length - 1)]));
            await e.UpdateGroupCooldown("utils.choose");
        }

        [Command("multiplechoice", "多重抉择")]
        [CommandDescription("从给定的列表中随机选择多项输出")]
        [ParameterDescription(1, "需要的输出个数")]
        [ParameterDescription(2, "指定的选择列表")]
        public static async void ChooseMutiple(SoraMessage e, int num = 0, params string[] c)
        {
            if (!_init)
            {
                _init = true;
                rnd.Seed(Convert.ToUInt32(DateTime.UtcNow.Millisecond));
            }
            if (num <= 0)
            {
                await e.ReplyToOriginal("参数不合法，需求数量必须至少为1");
                return;
            }
            else if (c.Length <= num)
            {
                await e.ReplyToOriginal("参数不合法，给出的选择项必须大于需求数量");
                return;
            }
            var lst = c.ToList();
            var choose = new string[num];
            for (var i = 0; i < num; i++)
            {
                var n = rnd.Int(0, lst.Count - 1);
                choose[i] = lst[n];
                lst.RemoveAt(n);
            }
            var sb = new StringBuilder();
            sb.Append("#抉择：");
            foreach (var s in choose)
                sb.AppendLine().Append("  ").Append(s);
            await e.ReplyToOriginal(ToSoraSegment(sb.ToString()));
            await e.UpdateGroupCooldown("utils.choose");
        }

        [Command("memberchoice", "成员抉择")]
        [CommandDescription("从群成员列表中随机选出指定数量的成员")]
        [ParameterDescription(1, "需要的输出个数")]
        public static async void ChooseMember(SoraMessage e, int num = 1)
        {
            if (!_init)
            {
                _init = true;
                rnd.Seed(Convert.ToUInt32(DateTime.UtcNow.Millisecond));
            }
            if (num <= 0)
            {
                await e.ReplyToOriginal("参数不合法，需求数量必须至少为1");
                return;
            }
            else if (num > 20)
            {
                await e.ReplyToOriginal("参数不合法，需求数量不应超过20，如有需求请使用其它工具");
                return;
            }
            try
            {
                var lst = (await e.SourceGroup.GetGroupMemberList()).groupMemberList;
                if (lst.Count <= num)
                {
                    await e.ReplyToOriginal("参数不合法，选择数量应小于群成员数量");
                    return;
                }
                var choose = new string[num];
                for (var i = 0; i < num; i++)
                {
                    var n = rnd.Int(0, lst.Count - 1);
                    choose[i] = $"{(string.IsNullOrEmpty(lst[n].Card) ? lst[n].Nick : lst[n].Card)}({lst[n].UserId})";
                    lst.RemoveAt(n);
                }
                var sb = new StringBuilder();
                sb.Append("#群成员抉择：");
                foreach (var s in choose)
                    sb.AppendLine().Append("  ").Append(s);
                await e.ReplyToOriginal(sb.ToString());
                await e.UpdateGroupCooldown("utils.choose");
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal("获取失败，", ex.GetFormatString());
            }
        }
    }
}
