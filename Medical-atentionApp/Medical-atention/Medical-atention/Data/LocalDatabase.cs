using SQLite;
using System;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public class LocalDatabase
    {
        private static readonly Lazy<LocalDatabase> _instance = new Lazy<LocalDatabase>(() => new LocalDatabase());
        private SQLiteAsyncConnection _connection;

        public static LocalDatabase Instance => _instance.Value;

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_connection != null) return _connection;

            var path = System.IO.Path.Combine(
                Xamarin.Essentials.FileSystem.AppDataDirectory,
                "medical_atention.db3");

            _connection = new SQLiteAsyncConnection(path);
            await _connection.CreateTableAsync<Models.PatientEntity>();
            return _connection;
        }
    }
}
