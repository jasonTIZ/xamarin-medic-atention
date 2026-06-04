using Medical_atention.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RegisterPatientPage : ContentPage
    {
        public RegisterPatientPage()
        {
            InitializeComponent();
            BindingContext = new RegisterPatientViewModel();
        }
    }
}
