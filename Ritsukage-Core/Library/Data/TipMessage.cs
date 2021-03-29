using SQLite;
using System;

namespace Ritsukage.Library.Data
{
    [Table("TipMessage"), AutoInitTable]
    public class TipMessage : DataTable
    {
        public enum TipTargetType
        {
            QQUser,
            QQGroup,
            DiscordChannel,
            DiscordUser
        }

        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("TargetType")]
        public TipTargetType TargetType { get; set; }

        [Column("TargetID")]
        public long TargetID { get; set; }

        [Column("TipTime")]
        public DateTime TipTime { get; set; }

        [Column("Message")]
        public string Message { get; set; }

        [Column("Duplicate")]
        public bool Duplicate { get; set; }

        [Column("Interval")]
        public TimeSpan Interval { get; set; }

        [Column("EndTime")]
        public DateTime EndTime { get; set; }
    }
}
