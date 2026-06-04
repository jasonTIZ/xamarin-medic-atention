using Medical_atention.Data;
using Medical_atention.Helpers;
using Medical_atention.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Medical_atention.Services
{
    public class PatientService : IPatientService
    {
        private readonly PatientRepository _repository = new PatientRepository();

        public bool IsOnline() =>
            Connectivity.NetworkAccess == NetworkAccess.Internet;

        public async Task<IReadOnlyList<Patient>> LoadPatientsAsync(bool forceRefresh = false)
        {
            if (IsOnline())
            {
                try
                {
                    var remote = await FetchFromApiAsync();
                    if (remote.Count > 0)
                    {
                        await _repository.ReplaceAllAsync(remote);
                        return remote;
                    }
                }
                catch (Exception)
                {
                    if (!forceRefresh)
                        return await _repository.GetAllAsync();
                    throw;
                }
            }

            return await _repository.GetAllAsync();
        }

        public async Task<Patient> GetPatientAsync(int id)
        {
            var local = await _repository.GetByIdAsync(id);
            if (local != null) return local;

            var all = await LoadPatientsAsync();
            return all.FirstOrDefault(p => p.Id == id);
        }

        private static async Task<List<Patient>> FetchFromApiAsync()
        {
            using var client = await ApiClient.CreateAsync();
            var response = await client.GetAsync("api/patients");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var dtos = JsonConvert.DeserializeObject<List<PatientApiDto>>(json) ?? new List<PatientApiDto>();

            return dtos.Select(d => new Patient
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                DocumentNumber = d.DocumentNumber ?? string.Empty,
                PriorityLevel = d.PriorityLevel
            }).ToList();
        }

        private class PatientApiDto
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
            public string DocumentNumber { get; set; }
            public int PriorityLevel { get; set; }
        }
    }
}
