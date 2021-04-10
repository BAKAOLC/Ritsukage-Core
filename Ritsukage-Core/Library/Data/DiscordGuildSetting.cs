using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("DiscordGuildSetting"), AutoInitTable]
    public class DiscordGuildSetting : DataTable
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Guild"), Indexed]
        public long Guild{ get; set; }

        [Column("FirstCommingRole")]
        public long FirstCommingRole { get; set; }
    }
}
