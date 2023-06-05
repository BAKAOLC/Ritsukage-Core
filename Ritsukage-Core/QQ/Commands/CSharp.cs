using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils"), OnlyForSuperUser]
    public static class CSharp
    {
        static readonly ScriptOptions Options = BuildScriptOptions();

        [Command("csharp")]
        [CommandDescription("执行C#代码")]
        [ParameterDescription(1, "代码")]
        public async static void Admin(SoraMessage e, string code)
        {
            code = SoraMessage.Escape(e.Message.RawText[8..]);
            try
            {
                await CSharpScript.RunAsync(code, Options, e);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error(nameof(CSharp), ex);
                await e.ReplyToOriginal(ex.GetFormatString());
            }
        }

        static ScriptOptions BuildScriptOptions()
        {
            var list = new List<Assembly>
            {
                Assembly.GetAssembly(typeof(CSharp))
            };
            GetReferanceAssemblies(Assembly.GetExecutingAssembly(), list);
            return ScriptOptions.Default
            .WithFileEncoding(Encoding.UTF8)
            .WithReferences(list);
        }

        static void GetReferanceAssemblies(Assembly assembly, List<Assembly> list = null)
        {
            foreach (var a in assembly.GetReferencedAssemblies())
            {
                var ass = Assembly.Load(a);
                if (!list.Contains(ass))
                {
                    list.Add(ass);
                    GetReferanceAssemblies(ass, list);
                }
            };
        }
    }
}