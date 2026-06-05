using Medical_atention.Constants;
using Medical_atention.Models;
using Medical_atention.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Medical_atention.ViewModels
{
    public class ConsultationHistoryViewModel : INotifyPropertyChanged
    {
        private readonly IConsultationService _consultationService;
        private int _patientId;
        private string _patientDisplayLine = string.Empty;
        private DateTime _filterFrom;
        private DateTime _filterTo = DateTime.Today;
        private bool _isLoading;
        private bool _isEmpty;

        public ConsultationHistoryViewModel() : this(new ConsultationService()) { }

        public ConsultationHistoryViewModel(IConsultationService consultationService)
        {
            _consultationService = consultationService;
            _filterFrom = new DateTime(DateTime.Today.Year, 1, 1);
            FilterCommand = new Command(async () => await LoadAsync());
        }

        public ObservableCollection<ConsultationListItem> Consultations { get; } = new ObservableCollection<ConsultationListItem>();

        public string PatientDisplayLine
        {
            get => _patientDisplayLine;
            set { _patientDisplayLine = value; OnPropertyChanged(); }
        }

        public DateTime FilterFrom
        {
            get => _filterFrom;
            set { _filterFrom = value; OnPropertyChanged(); }
        }

        public DateTime FilterTo
        {
            get => _filterTo;
            set { _filterTo = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsEmpty
        {
            get => _isEmpty;
            set { _isEmpty = value; OnPropertyChanged(); }
        }

        public ICommand FilterCommand { get; }
        public void Initialize(int patientId, string patientName, string patientCedula)
        {
            _patientId = patientId;
            PatientDisplayLine = string.IsNullOrWhiteSpace(patientCedula)
                ? $"Paciente: {patientName}"
                : $"Paciente: {patientName}, Cédula: {patientCedula}";
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (_patientId <= 0) return;

            if (FilterFrom > FilterTo)
            {
                var swap = FilterFrom;
                FilterFrom = FilterTo;
                FilterTo = swap;
            }

            IsLoading = true;
            try
            {
                var items = await _consultationService.GetByPatientAsync(
                    _patientId,
                    FilterFrom,
                    FilterTo,
                    await SecureStorage.GetAsync(AppConstants.TokenKey));

                Consultations.Clear();
                foreach (var item in items)
                    Consultations.Add(item);

                IsEmpty = !Consultations.Any();
            }
            catch (Exception)
            {
                IsEmpty = !Consultations.Any();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
