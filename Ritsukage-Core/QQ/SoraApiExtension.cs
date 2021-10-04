using Sora.Entities.Base;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.QQ
{
    public static class SoraApiExtension
    {
        public static async Task<bool> CheckHasGroup(this SoraApi api, long group)
            => Convert.ToBoolean((await api.GetGroupList())
                .groupList?.Where(x => x.GroupId == group).Any());

        public static async Task<bool> CheckHasFriend(this SoraApi api, long user)
            => Convert.ToBoolean((await api.GetFriendList())
                .friendList?.Where(x => x.UserId == user).Any());

        public static async Task<bool> CheckGroupHasUser(this SoraApi api, long group, long user)
            => Convert.ToBoolean((await api.GetGroupMemberList(group))
                .groupMemberList?.Where(x => x.UserId == user).Any());
    }
}
