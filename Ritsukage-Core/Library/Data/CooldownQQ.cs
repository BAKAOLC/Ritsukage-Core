using SQLite;
using System;

namespace Ritsukage.Library.Data
{
    [Table("CooldownQQ"), AutoInitTable]
    public class CooldownQQ
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("IsGroup")]
        public bool IsGroup { get; set; }

        [Column("QQ"), Indexed]
        public long QQ { get; set; }

        [Column("Tag")]
        public string Tag { get; set; }

        [Column("LastUsed")]
        public DateTime LastUsed { get; set; }
    }
}
