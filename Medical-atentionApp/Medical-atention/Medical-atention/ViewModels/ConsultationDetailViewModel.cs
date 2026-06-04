using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Medical_atention.ViewModels
{
    public class ConsultationDetailViewModel : INotifyPropertyChanged
    {
        private string _patientName = "Paciente";
        private string _priority = "medium";
        private string _chiefComplaint = "Consulta médica";
        private string _status = "En espera";

        public string PatientName
        {
            get => _patientName;
            set { _patientName = value; OnPropertyChanged(); }
        }

        public string Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(); }
        }

        public string ChiefComplaint
        {
            get => _chiefComplaint;
            set { _chiefComplaint = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
