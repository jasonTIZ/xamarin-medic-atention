using Medical_atention.Models;
using SQLite;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public static class LocalDatabase
    {
        private static SQLiteAsyncConnection _connection;
        private static bool _initialized;

        public static SQLiteAsyncConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    var path = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "medical_atention_local.db3");
                    _connection = new SQLiteAsyncConnection(path);
                }

                return _connection;
            }
        }

        public static async Task InitializeAsync()
        {
            if (_initialized) return;
            await Connection.CreateTableAsync<Patient>();
            _initialized = true;
        }
    }
}
