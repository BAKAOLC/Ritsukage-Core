using Ritsukage.Tools;
using System;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class HHSH
    {
        [Command("guess")]
        [CommandDescription("猜测指定的缩写字符的原本意义", "API接口来自 https://lab.magiconch.com/nbnhhsh")]
        [ParameterDescription(1, "缩写字符")]
        public static async void Normal(SoraMessage e, string origin)
        {
            try
            {
                var trans = NBNHHSH.Get(origin);
                if (trans.Length > 0)
                {
                    await e.ReplyToOriginal($"{origin} 的意思可能为" + Environment.NewLine + string.Join(" | ", trans));
                    return;
                }
            }
            catch
            { }
            await e.ReplyToOriginal($"{origin} 未能成功获取到猜测内容");
        }
    }
}