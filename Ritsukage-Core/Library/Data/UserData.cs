using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("UserData")]
    public class UserData
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Indexed]
        [Column("qq")]
        public long QQ { get; set; }

        [Indexed]
        [Column("discord")]
        public long Discord { get; set; }

        [Indexed]
        [Column("bilibili")]
        public int Bilibili { get; set; }

        [Column("coins")]
        public long Coins { get; set; }

        [Column("bilibili.cookie")]
        public string BilibiliCookie { get; set; }
    }
}
