using Ritsukage.QQ.Events;
using Ritsukage.Tools.Console;
using Sora.Entities.Base;
using Sora.Entities.Info;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Service
{
    [EventGroup, Service]
    public static class GroupFileManager
    {
        #region 储存结构定义
        public class GroupFileFolderBase
        {
            public readonly List<GroupFileFolder> Folders = new();
            public readonly List<GroupFile> Files = new();

            public int FolderCount => Folders.Count;
            public int FileCount => Files.Count;
        }

        public class GroupFileRootFolder : GroupFileFolderBase
        {
            public GroupFileRootFolder(List<GroupFolderInfo> folders = null, List<GroupFileInfo> files = null)
            {
                if (folders != null)
                    foreach (var data in folders)
                        Folders.Add(GroupFileFolder.ConvertFromInfo(data));
                if (files != null)
                    foreach (var data in files)
                        Files.Add(GroupFile.ConvertFromInfo(data));
            }
        }

        public class GroupFileFolder : GroupFileFolderBase
        {
            public string Id { get; init; }
            public string Name { get; init; }
            public DateTime CreateTime { get; init; }
            public long CreatorUserId { get; init; }
            public string CreatorUserName { get; init; }

            public static GroupFileFolder ConvertFromInfo(GroupFolderInfo info,
                List<GroupFolderInfo> folders = null, List<GroupFileInfo> files = null)
            {
                var folder = new GroupFileFolder()
                {
                    Id = info.Id,
                    Name = info.Name,
                    CreateTime = info.CreateTime,
                    CreatorUserId = info.CreatorUserId,
                    CreatorUserName = info.CreatorUserName
                };
                if (folders != null)
                    foreach (var data in folders)
                        folder.Folders.Add(ConvertFromInfo(data));
                if (files != null)
                    foreach (var data in files)
                        folder.Files.Add(GroupFile.ConvertFromInfo(data, folder));
                return folder;
            }
        }

        public class GroupFile
        {
            public string Id { get; init; }
            public string Name { get; init; }
            public int BusId { get; init; }
            public long Size { get; init; }
            public DateTime UploadTime { get; init; }
            public DateTime DeadTime { get; init; }
            public DateTime ModifyTime { get; init; }
            public int DownloadCount { get; init; }
            public long UploadUserId { get; init; }
            public string UploadUserName { get; init; }

            public GroupFileFolderBase Folder { get; init; }

            public static GroupFile ConvertFromInfo(GroupFileInfo info, GroupFileFolderBase folder = null)
                => new()
                {
                    Id = info.Id,
                    Name = info.Name,
                    BusId = info.BusId,
                    Size = info.Size,
                    UploadTime = info.UploadTime,
                    DeadTime = info.DeadTime,
                    ModifyTime = info.ModifyTime,
                    DownloadCount = info.DownloadCount,
                    UploadUserId = info.UploadUserId,
                    UploadUserName = info.UploadUserName,
                    Folder = folder
                };
        }
        #endregion

        #region 属性
        static readonly ConcurrentDictionary<long, GroupFileRootFolder> Files = new();

        static readonly object _lock = new object();

        static readonly List<long> Waiting = new();
        static readonly List<long> Updating = new();

        static readonly Dictionary<long, SoraApi> ApiRecord = new();
        #endregion

        #region 公开方法
        public static Task Init()
        {
            new Thread(UpdateThread) { IsBackground = true }.Start();
            return Task.CompletedTask;
        }

        public static GroupFileFolderBase GetFileList(long group, string folder = null)
        {
            if (Files.TryGetValue(group, out var root))
            {
                if (!string.IsNullOrWhiteSpace(folder))
                    return root.Folders.FirstOrDefault(x => folder.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                else
                    return root;
            }
            return null;
        }

        public static GroupFile FindFile(long group, string folder = null, Func<GroupFile, bool> predicate = null)
        {
            GroupFile result = null;
            var root = GetFileList(group, folder);
            if (root != null)
            {
                result = root.Files.FirstOrDefault(predicate);
                if (result == null)
                {
                    foreach (var subFolder in root.Folders)
                    {
                        result = FindFile(group, subFolder.Name, predicate);
                        if (result != null)
                        {
                            break;
                        }
                    }
                }
            }
            return result;
        }

        public static List<GroupFile> FindFiles(long group, string folder = null, Func<GroupFile, bool> predicate = null)
        {
            List<GroupFile> result = new();
            var root = GetFileList(group, folder);
            if (root != null)
            {
                var files = root.Files.Where(predicate);
                if (files.Any())
                    foreach (var file in files)
                        result.Add(file);
                foreach (var subFolder in root.Folders)
                {
                    var subFiles = FindFiles(group, subFolder.Name, predicate);
                    if (subFiles.Any())
                        foreach (var file in subFiles)
                            result.Add(file);
                }
            }
            return result;
        }

        public static async Task WaitForGroupFileDictionaryUpdated(long group)
        {
            bool isWaiting = false;
            lock (_lock)
            {
                if (Waiting.Contains(group))
                    isWaiting = true;
            }
            if (isWaiting)
            {
                await Task.Run(() =>
                {
                    while (Updating.Count == 0 || (!Updating.Contains(group) && Waiting.Contains(group)))
                        Thread.Sleep(100);
                    while (!(Updating.Count == 0 || Updating.Contains(group)))
                        Thread.Sleep(100);
                }).ContinueWith(x =>
                {
                    while (Updating.Contains(group))
                        Thread.Sleep(100);
                });
            }
        }

        public static async Task RequestUpdateGroupFileList(SoraApi api, long group, bool wait = false)
        {
            lock (_lock)
            {
                if (!Waiting.Contains(group))
                {
                    Waiting.Add(group);
                    ApiRecord[group] = api;
                }
            }
            if (wait)
                await WaitForGroupFileDictionaryUpdated(group);
        }

        [Event(typeof(ConnectEventArgs))]
        public static async void OnClientConnect(object sender, ConnectEventArgs args)
        {
            (var status, var groups) = await args.SoraApi.GetGroupList();
            if (status.RetCode == ApiStatusType.Ok)
                foreach (var group in groups)
                    await RequestUpdateGroupFileList(args.SoraApi, group.GroupId);
        }

        [Event(typeof(FileUploadEventArgs))]
        public static async void OnFileUpload(object sender, FileUploadEventArgs args)
        {
            ConsoleLog.Debug(nameof(GroupFileManager), $"有成员上传新的群文件  {args.FileInfo.Name}  上传者 {args.Sender.Id}");
            await RequestUpdateGroupFileList(args.SoraApi, args.SourceGroup.Id);
        }
        #endregion

        #region 私有方法
        static void UpdateThread()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (Waiting.Count != 0)
                {
                    foreach (var group in Waiting.ToArray())
                    {
                        if (!Updating.Contains(group))
                        {
                            Waiting.Remove(group);
                            Updating.Add(group);
                            Task.Run(async () =>
                            {
                                ConsoleLog.Debug(nameof(GroupFileManager), $"开始更新群文件列表，目标群: {group}");
                                await InternalUpdateGroupFileList(ApiRecord[group], group);
                                ConsoleLog.Debug(nameof(GroupFileManager), $"更新群文件列表结束，目标群: {group}");
                                Updating.Remove(group);
                            });
                        }
                    }
                }
            }
        }

        static async Task<bool> InternalUpdateGroupFileList(SoraApi api, long group)
        {
            GroupFileRootFolder root;
            lock (_lock)
            {
                if (!Files.TryGetValue(group, out root))
                {
                    Files.TryAdd(group, root = new GroupFileRootFolder());
                }
            }
            (var status, var files, var folders) = await api.GetGroupRootFiles(group);
            if (status.RetCode == ApiStatusType.Ok)
            {
                root.Folders.Clear();
                root.Files.Clear();
                foreach (var file in files)
                {
                    root.Files.Add(GroupFile.ConvertFromInfo(file, root));
                }
                foreach (var folder in folders)
                {
                    (var _status, var _files, var _folders) = await api.GetGroupFilesByFolder(group, folder.Id);
                    if (_status.RetCode == ApiStatusType.Ok)
                    {
                        root.Folders.Add(GroupFileFolder.ConvertFromInfo(folder, _folders, _files));
                    }
                    else
                    {
                        ConsoleLog.Error(nameof(LuaSTGGroupEvent), "文件列表更新失败");
                        return false;
                    }
                }
            }
            else
            {
                ConsoleLog.Error(nameof(LuaSTGGroupEvent), "文件列表更新失败");
                return false;
            }
            return true;
        }
        #endregion
    }
}
