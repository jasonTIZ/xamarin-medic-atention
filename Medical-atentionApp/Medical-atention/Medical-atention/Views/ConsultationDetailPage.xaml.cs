using Medical_atention.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(PatientId), "patientId")]
    [QueryProperty(nameof(Priority), "priority")]
    public partial class ConsultationDetailPage : ContentPage
    {
        private readonly ConsultationDetailViewModel _viewModel = new ConsultationDetailViewModel();

        public ConsultationDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public string PatientId
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _viewModel.PatientName = $"Paciente #{value}";
            }
        }

        public string Priority
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _viewModel.Priority = value;
            }
        }
    }
}
