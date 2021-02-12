using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("SubscribeStatusRecord"), AutoInitTable]
    public class SubscribeStatusRecord
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Type")]
        public string Type { get; set; }

        [Column("Target")]
        public string Target { get; set; }

        [Column("Status")]
        public string Status { get; set; }
    }
}
