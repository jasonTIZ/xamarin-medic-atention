using Medical_atention.Models.Entities;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            await Connection.CreateTableAsync<PatientEntity>();
            await EnsureConsultationsSchemaAsync();
            await Connection.CreateTableAsync<SyncQueueEntity>();
            await EnsurePatientColumnsAsync();

            _initialized = true;
        }

        private static async Task EnsurePatientColumnsAsync()
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
                await Connection.ExecuteAsync(sql);
        }

        private static async Task<HashSet<string>> GetColumnNamesAsync(string table)
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rows = await Connection.QueryAsync<TableInfoRow>($"PRAGMA table_info({table})");
            foreach (var row in rows)
                columns.Add(row.name);
            return columns;
        }

        private static async Task EnsureConsultationsSchemaAsync()
        {
            var rows = await Connection.QueryAsync<SqliteMasterSqlRow>(
                "SELECT sql FROM sqlite_master WHERE type='table' AND name='consultations'");
            var tableSql = rows.FirstOrDefault()?.sql;

            if (string.IsNullOrEmpty(tableSql))
            {
                await Connection.CreateTableAsync<ConsultationEntity>();
                return;
            }

            if (tableSql.IndexOf("AUTOINCREMENT", StringComparison.OrdinalIgnoreCase) < 0)
            {
                await Connection.ExecuteAsync("DROP TABLE IF EXISTS consultations");
                await Connection.CreateTableAsync<ConsultationEntity>();
            }
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
