using Medical_atention.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(PatientId), "id")]
    public partial class PatientDetailPage : ContentPage
    {
        public PatientDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            OfflineBanner?.Refresh();
        }

        public string PatientId
        {
            set
            {
                if (int.TryParse(value, out var id))
                {
                    if (BindingContext is PatientDetailViewModel existing && existing.PatientId == id)
                        return;
                    BindingContext = new PatientDetailViewModel(id);
                }
            }
        }
    }
}
