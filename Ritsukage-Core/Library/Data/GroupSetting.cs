using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("groupsetting")]
    class GroupSetting
    {
        [Column("group"), Indexed]
        public long Group { get; set; }
    }
}
