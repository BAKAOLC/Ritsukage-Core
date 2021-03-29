using SQLite;
using System;

namespace Ritsukage.Library.Data
{
    [Table("UserData"), AutoInitTable]
    public class UserData : DataTable
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

        [Column("FreeCoins")]
        public long FreeCoins { get; set; }

        [Column("FreeCoinsDate")]
        public DateTime FreeCoinsDate { get; set; }

        [Column("Bilibili.Cookie")]
        public string BilibiliCookie { get; set; }
    }
}
