namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Math")]
    public static class CalcOperator
    {
        [Command("calc")]
        public static async void CalcMath(SoraMessage e, string expr)
        {
            try
            {
                double result = Tools.CalcTool.GetExprValue(expr.Replace(" ", ""));
                await e.ReplyToOriginal($"{expr} = {result}");
            }
            catch
            {
                await e.ReplyToOriginal("操作失败");
            }
        }
    }
}