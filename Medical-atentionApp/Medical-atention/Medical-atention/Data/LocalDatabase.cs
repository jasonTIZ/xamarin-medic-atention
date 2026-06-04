using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public async Task<List<LocalConsultation>> GetConsultationsByPatientAsync(int patientId)
        {
            var db = await GetConnectionAsync();
            return await db.Table<LocalConsultation>()
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.ConsultationDate)
                .ToListAsync();
        }

        public async Task<LocalConsultation> GetConsultationByLocalIdAsync(int localId)
        {
            var db = await GetConnectionAsync();
            return await db.Table<LocalConsultation>().FirstOrDefaultAsync(c => c.LocalId == localId);
        }

        public async Task<LocalConsultation> GetConsultationByServerIdAsync(int serverId)
        {
            var db = await GetConnectionAsync();
            return await db.Table<LocalConsultation>().FirstOrDefaultAsync(c => c.ServerId == serverId);
        }

        public async Task<System.Collections.Generic.Dictionary<int, (string Priority, DateTime ConsultationDate)>> GetLatestPrioritiesByPatientAsync()
        {
            var db = await GetConnectionAsync();
            var all = await db.Table<LocalConsultation>().ToListAsync();
            return all
                .GroupBy(c => c.PatientId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var latest = g.OrderByDescending(c => c.ConsultationDate).First();
                        return (latest.Priority, latest.ConsultationDate);
                    });
        }
    }
}
