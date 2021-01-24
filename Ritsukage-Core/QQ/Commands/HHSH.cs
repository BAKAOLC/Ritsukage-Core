using Ritsukage.Tools;
using System;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup]
    public static class HHSH
    {
        [Command("guess")]
        public static async void Normal(SoraMessage e, string origin)
        {
            try
            {
                var trans = NBNHHSH.Get(origin);
                if (trans.Length > 0)
                {
                    await e.Reply($"{origin} 的意思可能为" + Environment.NewLine + string.Join(" ", trans));
                    return;
                }
            }
            catch
            { }
            await e.Reply($"{origin} 未能成功获取到猜测内容");
        }
    }
}