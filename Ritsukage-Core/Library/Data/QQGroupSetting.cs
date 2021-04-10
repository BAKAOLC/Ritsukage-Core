using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("QQGroupSetting"), AutoInitTable]
    public class QQGroupSetting : DataTable
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Group"), Indexed]
        public long Group { get; set; }

        [Column("SmartBilibiliLink")]
        public bool SmartBilibiliLink { get; set; }

        [Column("SmartMinecraftLink")]
        public bool SmartMinecraftLink { get; set; }
    }
}
