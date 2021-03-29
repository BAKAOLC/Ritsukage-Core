using SQLite;
using System;

namespace Ritsukage.Library.Data
{
    [Table("CooldownDiscord"), AutoInitTable]
    public class CooldownDiscord : DataTable
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("IsChannel")]
        public bool IsChannel { get; set; }

        [Column("Discord"), Indexed]
        public long Discord { get; set; }

        [Column("Tag")]
        public string Tag { get; set; }

        [Column("LastUsed")]
        public DateTime LastUsed { get; set; }
    }
}
