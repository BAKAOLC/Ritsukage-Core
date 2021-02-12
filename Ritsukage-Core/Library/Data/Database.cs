using Ritsukage.Tools.Console;
using SQLite;
using System;
using System.Linq;
using System.Reflection;

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
            ConsoleLog.Debug("Database", "Start loading...");
            Type[] types = Assembly.GetEntryAssembly().GetExportedTypes();
            Type[] cosType = types.Where(t => Attribute.GetCustomAttributes(t, true)
            .Where(a => a is AutoInitTableAttribute).Any()).ToArray();
            foreach (var group in cosType)
            {
                ConsoleLog.Debug("Database", $"Register database table: {group.FullName}");
                Data.CreateTableAsync(group);
            }
            ConsoleLog.Debug("Database", "Finish.");
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AutoInitTableAttribute : Attribute
    {}
}
