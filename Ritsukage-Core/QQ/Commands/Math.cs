﻿using AngouriMath;
using AngouriMath.Extensions;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using Sora.Entities.Segment;
using System;
using System.Linq;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Math")]
    public static class Math
    {
        const string LatexApi = "https://latex.codecogs.com/png.image?";

        [Command("math")]
        [CommandDescription("1", "expr")]
        public static async void Normal(SoraMessage e, string exprString)
        {
            exprString = e.Message.GetText()[6..];
            try
            {
                var expr = MathS.FromString(exprString);
                if (expr == null)
                {
                    await e.ReplyToOriginal("表达式解析错误");
                    return;
                }
                var sb = new StringBuilder();
                sb.Append(expr.ToString());
                if (expr.EvaluableNumerical)
                    sb.AppendLine().Append("= " + expr.EvalNumerical().ToString());
                else if (expr.EvaluableBoolean)
                    sb.AppendLine().Append("= " + expr.EvalBoolean().ToString());
                await e.ReplyToOriginal(sb.ToString());
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal(ex.Message);
                ConsoleLog.Error(nameof(Math), ex.GetFormatString());
            }
        }

        [Command("solve")]
        [CommandDescription("1", "expr")]
        public static async void Solve(SoraMessage e, string exprString)
        {
            exprString = e.Message.GetText()[7..];
            try
            {
                var lines = exprString.Replace("\r", string.Empty)
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim());
                var exprEntity = lines?.Where(x => x.StartsWith("expr>"))
                    ?.Select(x => x[5..].ToEntity());
                var needSolveEntity = lines?.Where(x => x.StartsWith("solve>"))
                    ?.Select(x => x[6..].ToEntity())?.Cast<Entity.Variable>();
                if (exprEntity == null || !exprEntity.Any())
                {
                    await e.ReplyToOriginal("表达式解析错误");
                    return;
                }
                var sb = new StringBuilder();
                if (exprEntity.Count() > 1)
                {
                    var equation = MathS.Equations(exprEntity.Select(x =>
                    {
                        var eq = x.ToString();
                        var index = eq.IndexOf('=');
                        if (index != -1)
                        {
                            var value = eq[(index + 1)..].ToEntity();
                            if (value.ToString() != "0")
                                eq = $"{eq[..index]} - ({value})";
                            else
                                eq = eq[..index];
                        }
                        return eq.ToEntity().Evaled;
                    }));
                    sb.AppendLine("> Expression:")
                        .Append(equation.ToString());
                    if (needSolveEntity != null && needSolveEntity.Any())
                    {
                        sb.AppendLine()
                            .AppendLine($"> Solve: {string.Join(" ", needSolveEntity)}")
                            .AppendLine("> Result:")
                            .Append(equation.Solve(needSolveEntity.ToArray()));
                    }
                }
                else
                {
                    var expr = exprEntity.First();
                    sb.AppendLine("> Expression:").Append(expr.ToString());
                    if (needSolveEntity != null && needSolveEntity.Any())
                    {
                        if (needSolveEntity.Count() > 1)
                        {
                            sb.AppendLine().Append("单表达式不允许进行多元求解");
                        }
                        else
                        {
                            var solve = needSolveEntity.First();
                            sb.AppendLine()
                                .AppendLine($"> Solve: {solve}")
                                .AppendLine("> Result:")
                                .Append(expr.Solve(needSolveEntity.First()));
                        }
                    }
                }
                await e.ReplyToOriginal(sb.ToString());
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal(ex.Message);
                ConsoleLog.Error(nameof(Math), ex.GetFormatString());
            }
        }

        [Command("tolatex")]
        [CommandDescription("1", "expr")]
        public static async void ToLatex(SoraMessage e, string exprString)
        {
            exprString = e.Message.GetText()[9..];
            try
            {
                await e.ReplyToOriginal(exprString.Latexise().ToString());
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal(ex.Message);
                ConsoleLog.Error(nameof(Math), ex.GetFormatString());
            }
        }

        [Command("latex")]
        [CommandDescription("1", "latex")]
        public static async void Latex(SoraMessage e, string latexString)
        {
            latexString = e.Message.GetText()[7..];
            if (!string.IsNullOrWhiteSpace(latexString))
            {
                var file = await DownloadManager.Download(LatexApi + Utils.UrlEncode(latexString));
                if (!string.IsNullOrWhiteSpace(file))
                    await e.ReplyToOriginal(SoraSegment.Image(file));
            }
        }
    }
}