using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Globalization;

namespace Ritsukage.Library.Bilibili.Model
{
    public class LiveRoom
    {
        #region 属性
        /// <summary>
        /// 用户uid
        /// </summary>
        public int UserId;
        /// <summary>
        /// 房间号
        /// </summary>
        public int Id;
        /// <summary>
        /// 短房间号
        /// </summary>
        public int ShortId;
        /// <summary>
        /// 主分区ID
        /// </summary>
        public int ParentAreaId;
        /// <summary>
        /// 主分区名称
        /// </summary>
        public string ParentAreaName;
        /// <summary>
        /// 分区ID
        /// </summary>
        public int AreaId;
        /// <summary>
        /// 分区名称
        /// </summary>
        public string AreaName;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title;
        /// <summary>
        /// 介绍
        /// </summary>
        public string Description;
        /// <summary>
        /// 用户封面
        /// </summary>
        public string UserCoverUrl;
        /// <summary>
        /// 关键帧
        /// </summary>
        public string KeyFrame;
        /// <summary>
        /// Tag列表
        /// </summary>
        public string[] Tags;
        /// <summary>
        /// 直播状态
        /// </summary>
        public LiveStatus LiveStatus;
        /// <summary>
        /// 开播时间
        /// </summary>
        public DateTime LiveStartTime;
        /// <summary>
        /// 人气值
        /// </summary>
        public int Online;

        public string Url { get => "https://live.bilibili.com/" + Id; }
        #endregion

        #region 方法
        public User GetUserInfo() => User.Get(UserId);

        public override string ToString()
            => string.IsNullOrWhiteSpace(UserCoverUrl) ? KeyFrame : UserCoverUrl + "\n"
                + "标题：" + Title + "\n"
                + "直播间ID：" + Id + "\n"
                + "当前分区：" + ParentAreaName + "·" + AreaName + "\n"
                + LiveStatus switch
                {
                    LiveStatus.Live => "当前正在直播",
                    LiveStatus.Round => "当前正在轮播",
                    _ => "当前未开播"
                } + "\n"
                + "当前人气值：" + Online + "\n"
                + Url;
        #endregion

        #region 构造
        public static LiveRoom Get(int id)
        {
            var info = JObject.Parse(Utils.HttpGET("http://api.live.bilibili.com/room/v1/Room/get_info?room_id=" + id));
            if ((int)info["code"] != 0)
                throw new Exception((string)info["message"]);
            var room = new LiveRoom()
            {
                UserId = (int)info["data"]["uid"],
                Id = (int)info["data"]["room_id"],
                ShortId = (int)info["data"]["short_id"],
                ParentAreaId = (int)info["data"]["parent_area_id"],
                ParentAreaName = (string)info["data"]["parent_area_name"],
                AreaId = (int)info["data"]["area_id"],
                AreaName = (string)info["data"]["area_name"],
                Title = (string)info["data"]["title"],
                Description = (string)info["data"]["description"],
                UserCoverUrl = (string)info["data"]["user_cover"],
                KeyFrame = (string)info["data"]["keyframe"],
                Tags = ((string)info["data"]["tags"]).Split(","),
                LiveStatus = (LiveStatus)(int)info["data"]["live_status"],
                Online = (int)info["data"]["online"],
            };
            if (room.LiveStatus == LiveStatus.Live)
                room.LiveStartTime = Convert.ToDateTime((string)info["data"]["live_time"], new DateTimeFormatInfo()
                {
                    FullDateTimePattern = "yyyy-MM-dd hh:mm:ss"
                });
            return room;
        }
        #endregion

        #region 静态方法
        public static int GetUserId(int roomid)
        {
            var info = JObject.Parse(Utils.HttpGET("https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom?room_id=" + roomid));
            if ((int)info["code"] != 0)
                throw new Exception((string)info["message"]);
            return (int)info["data"]["room_info"]["uid"];
        }
        #endregion
    }

    public enum LiveStatus
    {
        /// <summary>
        /// 未开播
        /// </summary>
        Cancel = 0,
        /// <summary>
        /// 直播中
        /// </summary>
        Live = 1,
        /// <summary>
        /// 轮播中
        /// </summary>
        Round = 2
    }
}
