using Medical_atention.Models.Entities;
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
        private bool _initialized;

        private LocalDatabase()
        {
            _initTask = InitializeDatabaseAsync();
        }

        public static LocalDatabase Instance => _instance.Value;

        public static SQLiteAsyncConnection Connection => Instance._connection;

        public static async Task InitializeAsync()
        {
            await Instance._initTask;
        }

        private async Task InitializeDatabaseAsync()
        {
            if (_initialized) return;

            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "medical_atention.db3");

            _connection = new SQLiteAsyncConnection(path);

            await _connection.CreateTableAsync<LocalConsultation>();
            await _connection.CreateTableAsync<PatientEntity>();
            await _connection.CreateTableAsync<SyncQueueEntity>();
            await _connection.CreateTableAsync<AttachmentEntity>();
            await EnsurePatientColumnsAsync();
            await EnsureConsultationsSchemaAsync();

            _initialized = true;
        }

        private async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            await _initTask;
            return _connection;
        }

        // ── Consultas locales (dev) ───────────────────────────────────────────

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

        public async Task<bool> UpdateConsultationTreatmentNotesAsync(
            int localId, int? serverId, string treatment, string notes, bool pendingSync)
        {
            var db = await GetConnectionAsync();
            LocalConsultation consultation = null;

            if (localId > 0)
                consultation = await db.Table<LocalConsultation>().FirstOrDefaultAsync(c => c.LocalId == localId);
            else if (serverId.HasValue && serverId.Value > 0)
                consultation = await db.Table<LocalConsultation>().FirstOrDefaultAsync(c => c.ServerId == serverId);

            if (consultation is null) return false;

            consultation.Treatment = treatment;
            consultation.Notes = notes;
            consultation.PendingSync = pendingSync;
            await db.UpdateAsync(consultation);
            return true;
        }

        public async Task<Dictionary<int, (string Priority, DateTime ConsultationDate)>> GetLatestPrioritiesByPatientAsync()
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

        // ── Adjuntos (attachments) ────────────────────────────────────────────

        public async Task<List<AttachmentEntity>> GetAttachmentsByConsultationAsync(
            int consultationLocalId, int? consultationServerId)
        {
            var db = await GetConnectionAsync();
            var byLocal = consultationLocalId > 0
                ? await db.Table<AttachmentEntity>()
                    .Where(a => a.ConsultationLocalId == consultationLocalId)
                    .ToListAsync()
                : new List<AttachmentEntity>();

            if (consultationServerId.HasValue && consultationServerId.Value > 0)
            {
                var byServer = await db.Table<AttachmentEntity>()
                    .Where(a => a.ConsultationServerId == consultationServerId.Value)
                    .ToListAsync();
                foreach (var a in byServer)
                    if (!byLocal.Exists(x => x.Id == a.Id))
                        byLocal.Add(a);
            }

            return byLocal.OrderBy(a => a.CreatedAt).ToList();
        }

        public async Task<int> SaveAttachmentAsync(AttachmentEntity attachment)
        {
            var db = await GetConnectionAsync();
            if (attachment.Id == 0)
                return await db.InsertAsync(attachment);
            await db.UpdateAsync(attachment);
            return attachment.Id;
        }

        public async Task UpdateAttachmentSyncedAsync(int id, int serverId, string remoteUrl)
        {
            var db = await GetConnectionAsync();
            var entity = await db.Table<AttachmentEntity>().FirstOrDefaultAsync(a => a.Id == id);
            if (entity is null) return;
            entity.ServerId = serverId;
            entity.RemoteUrl = remoteUrl;
            entity.PendingSync = false;
            await db.UpdateAsync(entity);
        }

        public async Task DeleteAttachmentAsync(int id)
        {
            var db = await GetConnectionAsync();
            await db.DeleteAsync<AttachmentEntity>(id);
        }

        public async Task<List<AttachmentEntity>> GetPendingAttachmentsAsync()
        {
            var db = await GetConnectionAsync();
            return await db.Table<AttachmentEntity>().Where(a => a.PendingSync).ToListAsync();
        }

        // ── Migraciones ───────────────────────────────────────────────────────

        private async Task EnsurePatientColumnsAsync()
        {
            var columns = await GetColumnNamesAsync("patients");
            var alters = new List<string>();

            if (!columns.Contains("LocalId"))
                alters.Add("ALTER TABLE patients ADD COLUMN LocalId TEXT NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'");
            if (!columns.Contains("pending_sync"))
                alters.Add("ALTER TABLE patients ADD COLUMN pending_sync INTEGER NOT NULL DEFAULT 0");
            if (!columns.Contains("DateOfBirth"))
                alters.Add("ALTER TABLE patients ADD COLUMN DateOfBirth TEXT NOT NULL DEFAULT '0001-01-01'");
            if (!columns.Contains("Gender"))
                alters.Add("ALTER TABLE patients ADD COLUMN Gender TEXT NOT NULL DEFAULT ''");

            foreach (var sql in alters)
                await _connection.ExecuteAsync(sql);
        }

        private async Task EnsureConsultationsSchemaAsync()
        {
            var rows = await _connection.QueryAsync<SqliteMasterSqlRow>(
                "SELECT sql FROM sqlite_master WHERE type='table' AND name='LocalConsultation'");
            var tableSql = rows.FirstOrDefault()?.sql;

            if (!string.IsNullOrEmpty(tableSql) &&
                tableSql.IndexOf("AUTOINCREMENT", StringComparison.OrdinalIgnoreCase) < 0)
            {
                await _connection.ExecuteAsync("DROP TABLE IF EXISTS LocalConsultation");
                await _connection.CreateTableAsync<LocalConsultation>();
            }
        }

        private async Task<HashSet<string>> GetColumnNamesAsync(string table)
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rows = await _connection.QueryAsync<TableInfoRow>($"PRAGMA table_info({table})");
            foreach (var row in rows)
                columns.Add(row.name);
            return columns;
        }

        private class TableInfoRow
        {
            public string name { get; set; }
        }

        private class SqliteMasterSqlRow
        {
            public string sql { get; set; }
        }
    }
}
