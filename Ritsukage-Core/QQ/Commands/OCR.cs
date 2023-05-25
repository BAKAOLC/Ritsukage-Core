using Ritsukage.Library.OCRSpace;
using Ritsukage.Library.OCRSpace.Enum;
using System;
using System.Linq;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("OCR"), OnlyForSuperUser]
    public static class OCR
    {
        static OCRSpaceApi Api;

        static readonly ApiHost Host = ApiHost.Asia;
        static readonly OCREngine Engine = OCREngine.Engine5;

        [Command("ocr")]
        [CommandDescription("执行OCR")]
        public static async void Normal(SoraMessage e)
        {
            if (string.IsNullOrWhiteSpace(Program.Config.OCRSpaceToken))
                return;
            if (Api == null)
            {
                Api = new(Program.Config.OCRSpaceToken, Host, Engine);
            }
            try
            {
                var imgs = e.Message.GetAllImage();
                if (!imgs.Any())
                {
                    await e.ReplyToOriginal("未检测到任何图像");
                    return;
                }
                var response = await Api.DoOCR(imgs.First().Url, Language.chs);
                if (response.OCRExitCode == OCRExitCode.Success)
                {
                    await e.ReplyToOriginal("[OCR]", Environment.NewLine,
                        string.Join(Environment.NewLine, response.ParsedResults.Select(x => x.ParsedText)));
                }
            }
            catch
            {
                await e.ReplyToOriginal("[OCR]", Environment.NewLine, "解析时发生错误");
            }
        }
    }
}