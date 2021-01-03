using Ritsukage.Tools;
using Sora.EventArgs.SoraEvent;
using System;

namespace Ritsukage.Commands
{
    [CommandGroup]
    public static class Bilibili
    {
        [Command]
        [CommandArgumentErrorCallback("AVBVConverterFallback")]
        public static async void AV2BV(BaseSoraEventArgs e, long av)
        {
            string msg;
            try
            {
                msg = $"[Bilibili][AV→BV] {av} → {BilibiliAVBVConverter.ToBV(av)}";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }

        [Command]
        [CommandArgumentErrorCallback("AVBVConverterFallback")]
        public static async void BV2AV(BaseSoraEventArgs e, string bv)
        {
            string msg;
            try
            {
                msg = $"[Bilibili][BV→AV] {bv} → {BilibiliAVBVConverter.ToAV(bv)}";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }

        public static async void AVBVConverterFallback(BaseSoraEventArgs e, Exception ex = null)
        {
            string msg;
            if (ex is ArgumentOutOfRangeException)
                msg = $"未填写参数，请重新输入";
            else
                msg = $"错误的参数指定，请检查参数是否正确({ex.Message})";
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }
    }
}
