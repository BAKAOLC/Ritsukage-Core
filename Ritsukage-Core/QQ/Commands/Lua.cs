using NLua;
using Ritsukage.Library.Lua;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class Lua
    {
        class LuaStdOutput
        {
            readonly StringBuilder sb = new();
            bool first = true;

            public LuaStdOutput() { }
            public LuaStdOutput(LuaEnv lua) => RegisterToLuaEnv(lua);

            public void Print(params object[] args)
            {
                for (int i = 0; i < args.Length; i++)
                    args[i] = args[i]?.ToString() ?? "nil";
                if (first)
                    first = false;
                else
                    sb.AppendLine();
                sb.Append(string.Join("\t", args));
            }

            public void RegisterToLuaEnv(LuaEnv lua)
            {
                lua.RegisterFunction("print", this, GetType().GetMethod(nameof(Print)));
                lua.DoString("local o=print;print=function(...)o(...)end");
            }

            public override string ToString()
                => sb.ToString();
        }

        static async void Process(SoraMessage e, LuaEnv lua, string code, int limitWorkTime = 10, int limitOutputLength = 0)
        {
            string errormsg = null;
            LuaStdOutput std = new(lua);
            var luaTask = Task.Run(() =>
            {
                LuaFunction luaFunction = null;
                try
                {
                    luaFunction = lua.LoadString("return " + code, "lua");
                }
                catch (Exception)
                {
                    try
                    {
                        luaFunction = lua.LoadString(code, "lua");
                    }
                    catch (Exception ex)
                    {
                        errormsg = ex.Message + ex.StackTrace;
                    }
                }
                try
                {
                    var result = luaFunction?.Call();
                    if (result?.Length > 0)
                        std.Print(result);
                }
                catch (Exception ex)
                {
                    errormsg = ex.Message + ex.StackTrace;
                }
            });
            var index = Task.WaitAny(luaTask, Task.Delay(TimeSpan.FromSeconds(limitWorkTime)));
            switch (index)
            {
                case 0:
                    var stdout = std.ToString();
                    if (limitOutputLength > 0)
                        stdout = stdout[..System.Math.Min(stdout.Length, limitOutputLength)];
                    if (!string.IsNullOrWhiteSpace(errormsg))
                        stdout += Environment.NewLine + errormsg;
                    if (string.IsNullOrWhiteSpace(stdout))
                        await e.ReplyToOriginal("#代码执行完毕，无任何输出");
                    else
                        await e.ReplyToOriginal("#Lua输出：", Environment.NewLine, stdout);
                    break;
                case 1:
                    await e.ReplyToOriginal($"代码执行已超过{limitWorkTime}秒，任务已强制结束");
                    break;
            }
            luaTask?.Dispose();
        }

        [Command("lua")]
        [CommandDescription("执行lua代码")]
        [ParameterDescription(1, "代码")]
        public static void Normal(SoraMessage e, string code)
        {
            code = SoraMessage.Escape(e.Message.RawText[5..]);
            using var lua = new LuaEnv(false, true);
            LuaEnv.SetUpSecureEnvironment(lua);
            try
            {
                Process(e, lua, code, 10, 100);
            }
            catch
            {
                //ignore
            }
            finally
            {
                lua?.Close();
                lua?.Dispose();
            }
        }

        [Command("slua"), OnlyForSuperUser]
        [CommandDescription("执行lua代码")]
        [ParameterDescription(1, "代码")]
        public static void Admin(SoraMessage e, string code)
        {
            code = SoraMessage.Escape(e.Message.RawText[6..]);
            using var lua = new LuaEnv(true, true);
            lua["MessageObject"] = e;
            try
            {
                Process(e, lua, code, 120, 1000);
            }
            catch
            {
                //ignore
            }
            finally
            {
                lua?.Close();
                lua?.Dispose();
            }
        }
    }
}