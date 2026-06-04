using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public class LocalDatabase
    {
        private static readonly Lazy<LocalDatabase> _instance = new Lazy<LocalDatabase>(() => new LocalDatabase());
        private SQLiteAsyncConnection _connection;
        private readonly Task _initTask;

        private LocalDatabase()
        {
            _initTask = InitializeAsync();
        }

        public static LocalDatabase Instance => _instance.Value;

        private async Task InitializeAsync()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "medical_atention.db3");
            _connection = new SQLiteAsyncConnection(path);
            await _connection.CreateTableAsync<LocalConsultation>();
        }

        private async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            await _initTask;
            return _connection;
        }

        public async Task<int> SaveConsultationAsync(LocalConsultation consultation)
        {
            var db = await GetConnectionAsync();
            if (consultation.LocalId == 0)
                return await db.InsertAsync(consultation);
            return await db.UpdateAsync(consultation);
        }

        public async Task<List<LocalConsultation>> GetPendingSyncAsync()
        {
            var db = await GetConnectionAsync();
            return await db.Table<LocalConsultation>().Where(c => c.PendingSync).ToListAsync();
        }
    }
}
