namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Math")]
    public static class CalcOperator
    {
        [Command("calc")]
        [CommandDescription("进行\"简单\"的数学计算")]
        [ParameterDescription(1, "表达式")]
        public static async void CalcMath(SoraMessage e, string expr)
        {
            expr = SoraMessage.Escape(expr);
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