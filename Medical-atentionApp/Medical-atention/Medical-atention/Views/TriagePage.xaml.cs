using Medical_atention.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TriagePage : ContentPage
    {
        public TriagePage()
        {
            InitializeComponent();
            BindingContext = new TriageViewModel();
        }
    }
}
