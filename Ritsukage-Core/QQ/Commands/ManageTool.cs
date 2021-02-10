namespace Ritsukage.QQ.Commands
{
    [CommandGroup, CanWorkIn(WorkIn.Group), LimitMemberRoleType(Sora.Enumeration.EventParamsType.MemberRoleType.Admin)]
    public static class ManageTool
    {
        [Command("公告")]
        public static async void Notice(SoraMessage e, string content) => await e.SoraApi.SendGroupNotice(e.SourceGroup, content);
    }
}
