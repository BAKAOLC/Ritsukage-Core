using System;
using System.Linq;
using System.Text;

namespace Ritsukage.Library.Lua
{
    public class LuaEnv : NLua.Lua
    {
        public LuaEnv(bool enableCSharpCLR = false, bool enableLuaStandardLib = true)
            : base(enableLuaStandardLib)
        {
            State.Encoding = Encoding.UTF8;
            if (enableCSharpCLR)
                LoadCLRPackage();
        }

        private static readonly string[] vars_global = new[]
            { "dofile", "io", "loadfile", "luanet", "package", "require" };

        private static readonly string[] vars_debug = new[] { "getregistry", "getuservalue", "setuservalue" };
        private static readonly string[] vars_os = new[] { "execute", "getenv", "remove", "rename", "tmpname" };

        public static void SetUpSecureEnvironment(LuaEnv env)
        {
            RemoveVariables(env, vars_global);
            RemoveTableVariables(env, "debug", vars_debug);
            RemoveTableVariables(env, "os", vars_os);
        }

        private static void RemoveVariables(LuaEnv env, params string[] keys)
        {
            Array.ForEach(keys, key => env[key] = null);
        }

        private static void RemoveTableVariables(LuaEnv env, string table, params string[] keys)
        {
            if (env[table] != null)
                RemoveVariables(env, keys.Select(x => $"{table}.{x}").ToArray());
        }
    }
}