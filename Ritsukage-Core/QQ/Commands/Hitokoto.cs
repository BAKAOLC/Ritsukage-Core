namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class Hitokoto
    {
        [Command("一言", "hitokoto")]
        public static async void Normal(SoraMessage e)
        {
            try
            {
                await e.Reply(Tools.Hitokoto.Get().ToString());
            }
            catch
            {
                await e.Reply("一言获取失败，请稍后再试");
            }
        }

        [Command("毒一言", "anotherhitokoto")]
        public static async void Another(SoraMessage e)
        {
            try
            {
                await e.Reply(Tools.Hitokoto.GetAnother());
            }
            catch
            {
                await e.Reply("毒一言获取失败，请稍后再试");
            }
        }
    }
}
