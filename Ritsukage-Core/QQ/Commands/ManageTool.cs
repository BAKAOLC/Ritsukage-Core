namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Manage Tools"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(Sora.Enumeration.EventParamsType.MemberRoleType.Admin)]
    public static class ManageTool
    {
        [Command("公告")]
        [CommandDescription("发布文本公告")]
        [ParameterDescription(1, "正文")]
        public static async void Notice(SoraMessage e, string content) => await e.SoraApi.SendGroupNotice(e.SourceGroup, content);
    }
}
