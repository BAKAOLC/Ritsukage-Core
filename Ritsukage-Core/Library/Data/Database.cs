using Ritsukage.Tools.Console;
using SQLite;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

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

        /*
        public static Task<T> FindAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
            => Data.FindAsync(predicate);
        public static Task<T> FindAsync<T>(object pk) where T : new()
            => Data.FindAsync<T>(pk);
        public static Task<T> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
            => Data.GetAsync(predicate);
        public static Task<T> GetAsync<T>(object pk) where T : new()
            => Data.GetAsync<T>(pk);
        */
        public static Task<int> DeleteAsync<t>(object primaryKey)
            => Data.DeleteAsync<t>(primaryKey);
        public static Task<int> DeleteAsync(object objectToDelete)
            => Data.DeleteAsync(objectToDelete);
        public static Task<int> InsertAllAsync(IEnumerable objects, string extra, bool runInTransaction = true)
            => Data.InsertAllAsync(objects, extra, runInTransaction);
        public static Task<int> InsertAllAsync(IEnumerable objects, bool runInTransaction = true)
            => Data.InsertAllAsync(objects, runInTransaction);
        public static Task<int> InsertAllAsync(IEnumerable objects, Type objType, bool runInTransaction = true)
            => Data.InsertAllAsync(objects, objType, runInTransaction);
        public static Task<int> InsertAsync(object obj, Type objType)
            => Data.InsertAsync(obj, objType);
        public static Task<int> InsertAsync(object obj)
            => Data.InsertAsync(obj);
        public static Task<int> InsertAsync(object obj, string extra)
            => Data.InsertAsync(obj, extra);
        public static Task<int> InsertAsync(object obj, string extra, Type objType)
            => Data.InsertAsync(obj, extra, objType);
        public static Task<int> InsertOrReplaceAsync(object obj, Type objType)
            => Data.InsertOrReplaceAsync(obj, objType);
        public static Task<int> InsertOrReplaceAsync(object obj)
            => Data.InsertOrReplaceAsync(obj);
        public static AsyncTableQuery<T> Table<T>() where T : new()
            => Data.Table<T>();
        public static Task<int> UpdateAllAsync(IEnumerable objects, bool runInTransaction = true)
            => Data.UpdateAllAsync(objects, runInTransaction);
        public static Task<int> UpdateAsync(object obj)
            => Data.UpdateAsync(obj);
        public static Task<int> UpdateAsync(object obj, Type objType)
            => Data.UpdateAsync(obj, objType);

        public static async Task<int> DeleteAll<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            T target;
            int deleted = 0;
            while ((target = await FindAsync(predicate)) != null)
            {
                await DeleteAsync(target);
                deleted++;
            }
            return deleted;
        }
        public static async Task<T[]> GetArrayAsync<T>()
            where T : new()
            => await Table<T>()?.ToArrayAsync() ?? Array.Empty<T>();
        public static async Task<T[]> GetArrayAsync<T>(Expression<Func<T, bool>> predicate)
            where T : new()
            => (await Where<T>(predicate)).ToArray();

        static async Task<IQueryable<T>> GetQueryable<T>()
            where T : new()
            => (await GetArrayAsync<T>()).AsQueryable();

        public static async Task<IQueryable<T>> Where<T>(Expression<Func<T, bool>> predicate)
            where T : new()
            => (await GetQueryable<T>()).Where(predicate);

        public static async Task<T> FirstOrDefault<T>()
            where T : new()
            => (await GetQueryable<T>()).FirstOrDefault();
        public static async Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> predicate)
            where T : new()
            => (await GetQueryable<T>()).FirstOrDefault(predicate);
        public static async Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> predicate, T defaultValue)
            where T : new()
            => (await GetQueryable<T>()).FirstOrDefault(predicate, defaultValue);

        public static async Task<T> FindAsync<T>(Expression<Func<T, bool>> predicate)
            where T : new()
            => await FirstOrDefault(predicate);

        public static async Task<int> CountAsync<T>()
            where T : new()
            => (await GetArrayAsync<T>()).Length;
        public static async Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate)
            where T : new()
            => (await GetQueryable<T>()).Count(predicate);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AutoInitTableAttribute : Attribute
    { }

    public class DataTable
    {
        public Task<int> DeleteAsync()
            => Database.DeleteAsync(this);
        public Task<int> InsertAsync()
            => Database.InsertAsync(this);
        public Task<int> InsertOrReplaceAsync()
            => Database.InsertOrReplaceAsync(this);
        public Task<int> UpdateAsync()
            => Database.UpdateAsync(this);
    }
}
