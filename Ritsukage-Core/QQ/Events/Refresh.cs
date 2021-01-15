using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;

#pragma warning disable IDE0042 // 析构变量声明
namespace Ritsukage.QQ.Events
{
    [EventGroup]
    public static class Refresh
    {
        [Event(typeof(ConnectEventArgs))]
        public static async void OnClientConnect(object sender, ConnectEventArgs args)
        {
            var f = await args.SoraApi.GetFriendList();
            if (f.apiStatus == APIStatusType.OK)
                FriendList.RefreshList(args.LoginUid, f.friendList);
            var g = await args.SoraApi.GetGroupList();
            if (g.apiStatus == APIStatusType.OK)
            {
                GroupList.RefreshList(args.LoginUid, g.groupList);
                foreach (var group in g.groupList)
                {
                    var gm = await args.SoraApi.GetGroupMemberList(group.GroupId);
                    if (gm.apiStatus == APIStatusType.OK)
                        GroupList.RefreshMemberList(group.GroupId, gm.groupMemberList);
                }
            }
        }

        [Event(typeof(FriendAddEventArgs))]
        public static async void OnFriendAdd(object sender, FriendAddEventArgs args)
        {
            var f = await args.SoraApi.GetFriendList();
            if (f.apiStatus == APIStatusType.OK)
                FriendList.RefreshList(args.LoginUid, f.friendList);
        }

        [Event(typeof(GroupAdminChangeEventArgs))]
        public static async void OnGroupAdminChange(object sender, GroupAdminChangeEventArgs args)
        {
            var g = await args.SoraApi.GetGroupMemberInfo(args.SourceGroup.Id, args.Sender.Id);
            if (g.apiStatus == APIStatusType.OK)
                GroupList.UpdateMember(g.memberInfo.GroupId, g.memberInfo);
        }

        [Event(typeof(GroupCardUpdateEventArgs))]
        public static async void OnGroupCardUpdate(object sender, GroupCardUpdateEventArgs args)
        {
            var g = await args.SoraApi.GetGroupMemberInfo(args.SourceGroup.Id, args.User.Id);
            if (g.apiStatus == APIStatusType.OK)
                GroupList.UpdateMember(g.memberInfo.GroupId, g.memberInfo);
        }

        [Event(typeof(GroupMemberChangeEventArgs))]
        public static async void OnGroupMemberChange(object sender, GroupMemberChangeEventArgs args)
        {
            var g = await args.SoraApi.GetGroupMemberList(args.SourceGroup.Id);
            if (g.apiStatus == APIStatusType.OK)
                GroupList.RefreshMemberList(args.SourceGroup.Id, g.groupMemberList);
        }
    }
}
#pragma warning restore IDE0042 // 析构变量声明
