using Ritsukage.Library.Graphic;
using Ritsukage.Tools;
using Sora.Entities.Segment;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class QRCode
    {
        [Command("qrcode"), NeedCoins(2)]
        [CommandDescription("生成QRcode")]
        [ParameterDescription(1, "内容")]
        public static async void Generate(SoraMessage e, string content)
        {
            var qr = QRCodeTool.Generate(content);
            var path = qr.ToBase64File();
            await e.ReplyToOriginal(SoraSegment.Image(path));
            await e.RemoveCoins(2);
        }
    }
}