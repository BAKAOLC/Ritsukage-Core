using Ritsukage.Library.Bank;
using System;
using System.Linq;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Bank")]
    public static class BankForex
    {
        [Command("tocny")]
        [CommandDescription("根据汇率计算外币等于多少人民币")]
        [ParameterDescription(1, "外币数值")]
        [ParameterDescription(2, "外币类型")]
        public static async void ToCNY(SoraMessage e, double value, string from)
        {
            try
            {
                var result = await Forex.GetToCNY(from, value);
                if (result.Value != 0)
                    await e.ReplyToOriginal(result.ToString());
                else
                    await e.ReplyToOriginal("暂不支持该币种换算");
            }
            catch
            {
                await e.ReplyToOriginal("操作失败");
            }
        }

        [Command("fromcny")]
        [CommandDescription("根据汇率计算人民币等于多少外币")]
        [ParameterDescription(1, "人民币数值")]
        [ParameterDescription(2, "外币类型")]
        public static async void FromCNY(SoraMessage e, double value, string to)
        {
            try
            {
                var result = await Forex.GetFromCNY(to, value);
                if (result.Value != 0)
                    await e.ReplyToOriginal(result.ToString());
                else
                    await e.ReplyToOriginal("暂不支持该币种换算");
            }
            catch
            {
                await e.ReplyToOriginal("操作失败");
            }
        }

        [Command("currency", "currencylist", "货币类型")]
        [CommandDescription("查询bot支持的货币类型")]
        public static async void CurrencyList(SoraMessage e)
        {
            await e.Reply("#货币类型如下："
                + Environment.NewLine
                + string.Join(Environment.NewLine, Forex.GetForexList().Select(x => $"{x.Key} {x.Value}")));
        }
    }
}