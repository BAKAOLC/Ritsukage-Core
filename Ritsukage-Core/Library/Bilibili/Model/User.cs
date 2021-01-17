using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Text;

namespace Ritsukage.Library.Bilibili.Model
{
    public class User
    {
        #region 属性
        /// <summary>
        /// UID
        /// </summary>
        public int Id;
        /// <summary>
        /// 昵称
        /// </summary>
        public string Name;
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex;
        /// <summary>
        /// 头像链接
        /// </summary>
        public string FaceUrl;
        /// <summary>
        /// 签名
        /// </summary>
        public string Sign;
        /// <summary>
        /// 等级
        /// </summary>
        public int Level;
        /// <summary>
        /// 生日
        /// </summary>
        public string Birthday;
        /// <summary>
        /// 关注人数
        /// </summary>
        public int Following;
        /// <summary>
        /// 粉丝数
        /// </summary>
        public int Follower;

        public string Url { get => "https://space.bilibili.com/" + Id; }
        #endregion

        #region 方法
        public int GetLiveRoomId() => GetLiveRoomId(Id);
        public LiveRoom GetLiveRoom() => LiveRoom.Get(GetLiveRoomId());

        public Dynamic[] GetDynamicList(ulong offset = 0) => Dynamic.GetDynamicList(Id, offset);

        public string BaseToString()
        {
            var birth = string.IsNullOrWhiteSpace(Birthday) ? "保密" : Birthday;
            return new StringBuilder()
                .AppendLine($"{Name} (UID:{Id}) Lv{Level}")
                .AppendLine($"性别：{Sex}  生日：{birth}  关注：{Following}  粉丝：{Follower}")
                .Append(Sign)
                .ToString();
        }
        public override string ToString()
            => new StringBuilder()
            .AppendLine(FaceUrl)
            .Append(BaseToString())
            .ToString();

        #endregion

        #region 构造
        public static User Get(int id)
        {
            var info = JObject.Parse(Utils.HttpGET("https://api.bilibili.com/x/web-interface/card?jsonp=jsonp&photo=1&mid=" + id));
            if (((int)info["code"]) != 0)
                throw new Exception((string)info["message"]);
            return new User()
            {
                Id = (int)info["data"]["card"]["mid"],
                Name = (string)info["data"]["card"]["name"],
                Sex = (string)info["data"]["card"]["sex"],
                FaceUrl = (string)info["data"]["card"]["face"],
                Sign = (string)info["data"]["card"]["sign"],
                Level = (int)info["data"]["card"]["level_info"]["current_level"],
                Birthday = (string)info["data"]["card"]["birthday"],
                Following = (int)info["data"]["card"]["attention"],
                Follower = (int)info["data"]["card"]["fans"],
            };
        }
        #endregion

        #region 静态方法
        public static int GetLiveRoomId(int id)
        {
            var info = JObject.Parse(Utils.HttpGET("https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld?mid=" + id));
            if ((int)info["code"] != 0)
                throw new Exception((string)info["message"]);
            return (int)info["data"]["roomid"];
        }
        #endregion
    }

    public class MyUserInfo
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Sex { get; private set; }
        public string FaceUrl { get; private set; }
        public string Sign { get; private set; }
        public int Level { get; private set; }
        public int Exp { get; private set; }
        public int ExpMin { get; private set; }
        public int ExpNext { get; private set; }
        public string Birth { get; private set; }
        public double Coin { get; private set; }
        public int Moral { get; private set; }
        public int Follower { get; private set; }
        public int Following { get; private set; }

        public string Url { get => "https://space.bilibili.com/" + Id; }

        public MyUserInfo(string cookie = "")
        {
            var data = JObject.Parse(Utils.HttpGET("https://api.bilibili.com/x/space/myinfo?jsonp=jsonp", "", 20000, cookie));
            if ((int)data["code"] != 0)
                throw new Exception((string)data["message"]);
            var data2 = JObject.Parse(Utils.HttpGET("https://api.bilibili.com/x/web-interface/nav", "", 20000, cookie));
            var data3 = JObject.Parse(Utils.HttpGET("https://api.bilibili.com/x/member/web/account", "", 20000, cookie));
            var data4 = JObject.Parse(Utils.HttpGET("https://api.bilibili.com/x/web-interface/nav/stat", "", 20000, cookie));
            Id = (int)data["data"]["mid"];
            Name = (string)data["data"]["name"];
            Sex = (string)data["data"]["sex"];
            FaceUrl = (string)data["data"]["face"];
            Sign = (string)data["data"]["sign"];
            Level = (int)data2["data"]["level_info"]["current_level"];
            Exp = (int)data2["data"]["level_info"]["current_exp"];
            ExpMin = (int)data2["data"]["level_info"]["current_min"];
            if (int.TryParse((string)data2["data"]["level_info"]["next_exp"], out var exp))
                ExpNext = exp;
            else
                ExpNext = Exp;
            Coin = (double)data2["data"]["money"];
            Moral = (int)data2["data"]["moral"];
            Birth = (string)data3["data"]["birthday"];
            Follower = (int)data4["data"]["follower"];
            Following = (int)data4["data"]["following"];
        }

        public User GetUserInfo() => User.Get(Id);

        public override string ToString()
        {
            var birth = string.IsNullOrWhiteSpace(Birth) ? "保密" : Birth;
            return FaceUrl + "\n"
                + $"{Name} (UID:{Id}) Lv{Level}({Exp}/{ExpNext})" + "\n"
                + $"性别：{Sex}  生日：{birth}  关注：{Following}  粉丝：{Follower}" + "\n"
                + Sign + "\n" + Url;
        }
    }
}
