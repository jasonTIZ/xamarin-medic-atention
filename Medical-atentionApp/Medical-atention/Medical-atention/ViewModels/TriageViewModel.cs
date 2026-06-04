using Medical_atention.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Medical_atention.ViewModels
{
    public class TriageViewModel : INotifyPropertyChanged
    {
        public TriageViewModel()
        {
            Queue = new ObservableCollection<TriageQueueItem>
            {
                new TriageQueueItem { PatientName = "Adam Acuña", IdentificationNumber = "151646464", Priority = "urgent", ChiefComplaint = "Dolor torácico" },
                new TriageQueueItem { PatientName = "Beatriz Blanco", IdentificationNumber = "102030405", Priority = "high", ChiefComplaint = "Fiebre persistente" },
                new TriageQueueItem { PatientName = "Carlos Mora", IdentificationNumber = "203040506", Priority = "medium", ChiefComplaint = "Control rutinario" },
                new TriageQueueItem { PatientName = "Diana Rojas", IdentificationNumber = "304050607", Priority = "low", ChiefComplaint = "Renovación de receta" }
            };
        }

        public ObservableCollection<TriageQueueItem> Queue { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
