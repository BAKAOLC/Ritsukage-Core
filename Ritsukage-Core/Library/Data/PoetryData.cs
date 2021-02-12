using SQLite;

namespace Ritsukage.Library.Data
{
    [Table("Ci.Song.Author")]
    public class Ci_Song_Author
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        public string Description { get; set; }

        [Column("ShortDescription")]
        public string ShortDescription { get; set; }
    }

    [Table("Ci.Song")]
    public class Ci_Song
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        [Column("Author")]
        public string Author { get; set; }

        [Column("Content")]
        public string Content { get; set; }
    }

    [Table("Poetry.Song.Author")]
    public class Poetry_Song_Author
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        public string Description { get; set; }
    }

    [Table("Poetry.Song")]
    public class Poetry_Song
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        [Column("Author")]
        public string Author { get; set; }

        [Column("Paragraphs")]
        public string Paragraphs { get; set; }

        [Column("Strains")]
        public string Strains { get; set; }
    }

    [Table("Poetry.Tang.Author")]
    public class Poetry_Tang_Author
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Description")]
        public string Description { get; set; }
    }

    [Table("Poetry.Tang")]
    public class Poetry_Tang
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        [Column("Author")]
        public string Author { get; set; }

        [Column("Paragraphs")]
        public string Paragraphs { get; set; }

        [Column("Strains")]
        public string Strains { get; set; }
    }

    [Table("Poetry.Tang.300")]
    public class Poetry_Tang_300
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Type")]
        public string Type { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        [Column("SubTitle")]
        public string SubTitle { get; set; }

        [Column("Author")]
        public string Author { get; set; }

        [Column("Paragraphs")]
        public string Paragraphs { get; set; }
    }

    [Table("Poetry.Classic")]
    public class Poetry_Classic
    {
        [Column("ID"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Title")]
        public string Title { get; set; }

        [Column("Chapter")]
        public string Chapter { get; set; }

        [Column("Section")]
        public string Section { get; set; }

        [Column("Content")]
        public string Content { get; set; }
    }
}
