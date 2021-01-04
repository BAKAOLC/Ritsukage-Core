using SQLite;

namespace Ritsukage.Library.Data
{
    public static class Database
    {
        public static SQLiteAsyncConnection Data { get; private set; }

        public static string DatabasePath { get; private set; } = "data.db";

        public static void Init(string path = "data.db")
        {
            Data = new SQLiteAsyncConnection(DatabasePath = path);
            InitTables();
        }

        static void InitTables()
        {
            Data.CreateTableAsync<UserData>();
        }
    }
}
