using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("qqgroupsetting")]
    class QQGroupSetting
    {
        [Column("group"), Indexed]
        public long Group { get; set; }
    }
}
