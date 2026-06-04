using Medical_atention.Models;
using Medical_atention.ViewModels;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PatientListPage : ContentPage
    {
        public PatientListPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is PatientListViewModel vm)
                await vm.InitializeAsync();
        }

        private async void OnPatientSelected(object sender, SelectionChangedEventArgs e)
        {
            var patient = e.CurrentSelection.FirstOrDefault() as Patient;
            if (patient == null)
                return;

            if (sender is CollectionView list)
                list.SelectedItem = null;

            if (BindingContext is PatientListViewModel vm)
                await Shell.Current.GoToAsync($"patientdetail?patientId={patient.Id}");
        }
    }
}
