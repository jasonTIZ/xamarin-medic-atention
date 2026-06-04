using Medical_atention.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_atention.Data
{
    public class PatientRepository
    {
        public async Task<List<Patient>> GetAllAsync()
        {
            await LocalDatabase.InitializeAsync();
            var list = await LocalDatabase.Connection.Table<Patient>().ToListAsync();
            return list
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToList();
        }

        public async Task ReplaceAllAsync(IEnumerable<Patient> patients)
        {
            await LocalDatabase.InitializeAsync();
            await LocalDatabase.Connection.RunInTransactionAsync(conn =>
            {
                conn.DeleteAll<Patient>();
                conn.InsertAll(patients);
            });
        }

        public async Task<Patient> GetByIdAsync(int id)
        {
            await LocalDatabase.InitializeAsync();
            return await LocalDatabase.Connection
                .Table<Patient>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}
