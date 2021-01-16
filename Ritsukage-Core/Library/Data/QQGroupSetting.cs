using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("QQGroupSetting")]
    public class QQGroupSetting
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Group"), Indexed]
        public long Group { get; set; }
    }
}
