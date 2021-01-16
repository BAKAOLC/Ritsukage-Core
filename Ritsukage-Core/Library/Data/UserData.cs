using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("UserData")]
    public class UserData
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("QQ"), Indexed]
        public long QQ { get; set; }

        [Column("Discord"), Indexed]
        public long Discord { get; set; }

        [Column("Bilibili"), Indexed]
        public int Bilibili { get; set; }

        [Column("Coins")]
        public long Coins { get; set; }

        [Column("Bilibili.Cookie")]
        public string BilibiliCookie { get; set; }
    }
}
