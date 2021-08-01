using Ritsukage.Tools.Console;
using Sora.Entities.Info;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ritsukage.QQ
{
    public static class GroupList
    {
        static readonly ConcurrentDictionary<long, ConcurrentDictionary<long, GroupInfo>> Record = new();

        static readonly ConcurrentDictionary<long, ConcurrentDictionary<long, GroupMemberInfo>> RecordSub = new();

        public static async void RefreshList(long bot, List<GroupInfo> list)
        {
            ConsoleLog.Debug("QQ Group List", $"Update list for bot {bot} with {list.Count} group(s)");
            await Task.Run(() =>
            {
                ConcurrentDictionary<long, GroupInfo> data = new();
                foreach (var f in list)
                    data.TryAdd(f.GroupId, f);
                Record[bot] = data;
            });
        }

        public static async void RefreshMemberList(long group, List<GroupMemberInfo> list)
        {
            ConsoleLog.Debug("QQ Group List", $"Update list for group {group} with {list.Count} member(s)");
            await Task.Run(() =>
            {
                ConcurrentDictionary<long, GroupMemberInfo> data = new();
                foreach (var f in list)
                    data.TryAdd(f.GroupId, f);
                RecordSub[group] = data;
            });
        }

        public static ConcurrentDictionary<long, GroupInfo> GetList(long bot)
        {
            if (Record.TryGetValue(bot, out var list))
                return list;
            else
                return null;
        }

        public static ConcurrentDictionary<long, GroupMemberInfo> GetMemberList(long group)
        {
            if (RecordSub.TryGetValue(group, out var list))
                return list;
            else
                return null;
        }

        public static GroupInfo GetInfo(long bot, long target)
        {
            var list = GetList(bot);
            if (list != null && list.TryGetValue(target, out var data))
                return data;
            else
                return new GroupInfo();
        }

        public static GroupMemberInfo GetMemberInfo(long group, long target)
        {
            var list = GetMemberList(group);
            if (list != null && list.TryGetValue(target, out var data))
                return data;
            else
                return new GroupMemberInfo();
        }

        public static bool Add(long bot, GroupInfo target)
        {
            var list = GetList(bot);
            if (list == null)
                return false;
            return list.TryAdd(target.GroupId, target);
        }

        public static bool Update(long bot, GroupInfo target)
        {
            var list = GetList(bot);
            if (list == null)
                return false;
            if (!list.ContainsKey(target.GroupId))
                return list.TryAdd(target.GroupId, target);
            list[target.GroupId] = target;
            return true;
        }

        public static bool Remove(long bot, long target)
        {
            var list = GetList(bot);
            if (list == null)
                return false;
            return list.Remove(target, out _);
        }

        public static bool HasGroup(long bot, long group)
        {
            var list = GetList(bot);
            if (list == null)
                return false;
            return list.TryGetValue(group, out _);
        }

        public static bool HasMember(long group, long target)
        {
            var list = GetMemberList(group);
            if (list == null)
                return false;
            return list.TryGetValue(target, out _);
        }

        public static bool AddMember(long group, GroupMemberInfo target)
        {
            var list = GetMemberList(group);
            if (list == null)
                return false;
            return list.TryAdd(target.UserId, target);
        }

        public static bool UpdateMember(long group, GroupMemberInfo target)
        {
            var list = GetMemberList(group);
            if (list == null)
                return false;
            if (!list.ContainsKey(target.UserId))
                return list.TryAdd(target.UserId, target);
            list[target.UserId] = target;
            return true;
        }

        public static bool RemoveMember(long group, long target)
        {
            var list = GetMemberList(group);
            if (list == null)
                return false;
            return list.Remove(target, out _);
        }
    }
}
