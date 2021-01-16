using Ritsukage.Tools.Console;
using Sora.Entities.Info;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ritsukage.QQ
{
    public static class FriendList
    {
        static readonly ConcurrentDictionary<long, ConcurrentDictionary<long, FriendInfo>> Record = new();

        static readonly object _lock = new();

        public static async void RefreshList(long bot, List<FriendInfo> list)
        {
            ConsoleLog.Debug("QQ Friend List", $"Update list for bot {bot} with {list.Count} friend(s)");
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    ConcurrentDictionary<long, FriendInfo> data = new();
                    foreach (var f in list)
                        data.TryAdd(f.UserId, f);
                    Record[bot] = data;
                }
            });
        }

        public static ConcurrentDictionary<long, FriendInfo> GetList(long bot)
        {
            if (Record.TryGetValue(bot, out var list))
                return list;
            else
                return null;
        }

        public static FriendInfo GetInfo(long bot, long target)
        {
            lock (_lock)
            {
                var list = GetList(bot);
                if (list != null && list.TryGetValue(target, out var data))
                    return data;
                else
                    return new FriendInfo();
            }
        }

        public static bool Add(long bot, FriendInfo target)
        {
            var list = GetList(bot);
            if (list == null)
                return false;
            return list.TryAdd(target.UserId, target);
        }

        public static bool Update(long bot, FriendInfo target)
        {
            var list = GetList(bot);
            if (list == null)
                return false;
            if (!list.ContainsKey(target.UserId))
                return list.TryAdd(target.UserId, target);
            list[target.UserId] = target;
            return true;
        }

        public static bool Remove(long bot, long target)
        {
            var list = GetList(bot);
            if (list == null)
                return false;
            return list.TryRemove(target, out _);
        }
    }
}
