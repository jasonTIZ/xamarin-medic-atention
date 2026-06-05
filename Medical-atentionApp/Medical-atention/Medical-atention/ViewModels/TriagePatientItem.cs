using Medical_atention.Helpers;
using Medical_atention.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class TriagePatientItem : INotifyPropertyChanged
    {
        private PriorityLevel _priority;

        public int Id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string IdentificationNumber { get; set; }
        private DateTime? _lastConsultationAt;

        public DateTime? LastConsultationAt
        {
            get => _lastConsultationAt;
            set
            {
                _lastConsultationAt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastConsultationText));
            }
        }

        public string FullName => $"{Name} {LastName}".Trim();

        public PriorityLevel Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PriorityLabel));
                OnPropertyChanged(nameof(PriorityColor));
            }
        }

        public string PriorityLabel => PriorityHelper.GetDisplayName(Priority);
        public Color PriorityColor => PriorityHelper.GetBadgeColor(Priority);
        public string LastConsultationText => PriorityHelper.FormatTimeSinceLastConsultation(LastConsultationAt);

        public static TriagePatientItem FromPatient(Patient patient) =>
            new TriagePatientItem
            {
                Id = patient.Id,
                Name = patient.FirstName,
                LastName = patient.LastName,
                IdentificationNumber = patient.DocumentNumber,
                Priority = patient.EffectivePriority,
                LastConsultationAt = patient.LastConsultationAt
            };

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class TriagePriorityGroup : ObservableCollection<TriagePatientItem>
    {
        public TriagePriorityGroup(PriorityLevel level) : base()
        {
            Level = level;
            Title = PriorityHelper.GetDisplayName(level);
            BackgroundColor = PriorityHelper.GetGroupBackgroundColor(level);
        }

        public PriorityLevel Level { get; }
        public string Title { get; }
        public Color BackgroundColor { get; }
    }
}
