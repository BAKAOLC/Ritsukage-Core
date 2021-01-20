namespace Ritsukage.QQ.Commands
{
    [CommandGroup]
    public static class Logistics
    {
        [Command("快递详情"), CanWorkIn(WorkIn.Private)]
        public static async void Normal(SoraMessage e, string id)
        {
            try
            {
                var info = Library.Roll.Model.Logistics.Get(id);
                await e.Reply(info.ToString());
            }
            catch
            {
                await e.Reply("快递详情获取失败，请稍后再试");
            }
        }
    }
}
