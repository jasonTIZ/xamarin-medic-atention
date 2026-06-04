using Medical_atention.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(PatientId), "patientId")]
    public partial class PatientDetailPage : ContentPage
    {
        private string _patientId;

        public string PatientId
        {
            get => _patientId;
            set => _patientId = value;
        }

        public PatientDetailPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is PatientDetailViewModel vm &&
                int.TryParse(PatientId, out var id))
            {
                await vm.LoadAsync(id);
            }
        }
    }
}
