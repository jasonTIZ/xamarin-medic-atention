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

        public string PatientId
        {
            set
            {
                if (int.TryParse(value, out var id))
                    BindingContext = new PatientDetailViewModel(id);
            }
        }
    }
}
