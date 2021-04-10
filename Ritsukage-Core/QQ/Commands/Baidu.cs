using Ritsukage.Tools;
using Sora.Entities.CQCodes;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Baidu")]
    public static class Baidu
    {
        [Command("翻译")]
        public static async void Translate(SoraMessage e, string text)
        {
            var result = BaiduTranslate.GetTranslateResult(text);
            if (result.Success)
            {
                if (result.OriginalLanguage.Equals(result.TranslateLanguage))
                {
                    await Task.Delay(1000);
                    result = BaiduTranslate.GetTranslateResult(text, result.OriginalLanguage?.Id ?? "zh", "en");
                }
                if (result.Success)
                {
                    await e.ReplyToOriginal(new StringBuilder()
                        .AppendLine($"[{result.OriginalLanguage}=>{result.TranslateLanguage}]")
                        .Append(result.TranslateString).ToString());
                    return;
                }
            }
            await e.ReplyToOriginal(new StringBuilder().Append("翻译失败，")
                .AppendLine(result.Info).Append(result.TipMessage).ToString());
        }
    }
}
