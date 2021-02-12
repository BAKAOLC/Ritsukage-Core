using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("SubscribeList"), AutoInitTable]
    public class SubscribeList
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Platform")]
        public string Platform { get; set; }

        [Column("Type")]
        public string Type { get; set; }

        [Column("Target")]
        public string Target { get; set; }

        [Column("Listener")]
        public string Listener { get; set; }
    }
}
