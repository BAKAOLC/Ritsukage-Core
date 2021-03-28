namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class Logistics
    {
        [Command("快递详情"), CanWorkIn(WorkIn.Private), NeedCoins(15)]
        public static async void Normal(SoraMessage e, string id)
        {
            try
            {
                var info = Library.Roll.Model.Logistics.Get(id);
                await e.Reply(info.ToString());
                await e.RemoveCoins(15);
            }
            catch
            {
                await e.ReplyToOriginal("快递详情获取失败，请稍后再试");
            }
        }
    }
}
