using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("userdata")]
    public class UserData
    {
        [Column("id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("qq"), Indexed]
        public long QQ { get; set; }

        [Column("discord"), Indexed]
        public long Discord { get; set; }

        [Column("bilibili"), Indexed]
        public int Bilibili { get; set; }

        [Column("coins")]
        public long Coins { get; set; }

        [Column("bilibili.cookie")]
        public string BilibiliCookie { get; set; }
    }
}
