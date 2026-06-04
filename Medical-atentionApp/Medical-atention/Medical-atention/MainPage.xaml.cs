using Medical_atention.Models;
using Medical_atention.ViewModels;
using Medical_atention.Views;
using System.Linq;
using Xamarin.Forms;

namespace Medical_atention
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = new PatientsViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ((PatientsViewModel)BindingContext).LoadPatientsAsync();
        }

        private async void OnPatientSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is PatientResponseDto patient)
            {
                ((CollectionView)sender).SelectedItem = null;
                await Shell.Current.GoToAsync($"{nameof(PatientDetailPage)}?id={patient.Id}");
            }
        }

        private async void OnAddPatientTapped(object sender, System.EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegisterPatientPage));
        }
    }
}
