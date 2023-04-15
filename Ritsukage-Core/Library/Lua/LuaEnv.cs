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

        public static void SetUpSecureEnvironment(LuaEnv env)
        {
            env["dofile"] = null;
            env["require"] = null;
            env["luanet"] = null;
            env["io"] = null;
            if (env["os"] != null)
            {
                env["os.execute"] = null;
                env["os.remove"] = null;
                env["os.rename"] = null;
                env["os.tmpname"] = null;
            }
        }
    }
}
