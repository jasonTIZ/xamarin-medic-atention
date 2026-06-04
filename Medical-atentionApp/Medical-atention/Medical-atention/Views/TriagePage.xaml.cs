using Medical_atention.Helpers;
using Medical_atention.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TriagePage : ContentPage
    {
        private readonly TriageViewModel _viewModel;
        private bool _timerRunning;

        public TriagePage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new TriageViewModel();
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
                await _viewModel.LoadAsync(silent: true);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.StartAutoRefresh();
            await _viewModel.LoadAsync();
            StartRefreshTimer();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.StopAutoRefresh();
            _timerRunning = false;
        }

        private void StartRefreshTimer()
        {
            if (_timerRunning) return;
            _timerRunning = true;

            Device.StartTimer(TimeSpan.FromSeconds(60), () =>
            {
                if (!_viewModel.ShouldAutoRefresh())
                    return _timerRunning;

                Device.BeginInvokeOnMainThread(async () => await _viewModel.LoadAsync(silent: true));
                return _timerRunning;
            });
        }

        private async void OnRefreshing(object sender, EventArgs e)
        {
            await _viewModel.LoadAsync();
            RefreshHost.IsRefreshing = false;
        }

        private TriagePatientItem GetItemFromView(Element element)
        {
            var view = element as BindableObject;
            while (view != null)
            {
                if (view.BindingContext is TriagePatientItem item)
                    return item;
                view = view.Parent;
            }
            return null;
        }

        private async void OnSwipePriorityInvoked(object sender, EventArgs e)
        {
            var item = GetItemFromView(sender as Element);
            if (item != null)
                await ShowPriorityActionSheetAsync(item);
        }

        private async void OnPriorityBadgeTapped(object sender, EventArgs e)
        {
            var item = GetItemFromView(sender as Element);
            if (item != null)
                await ShowPriorityActionSheetAsync(item);
        }

        private async Task ShowPriorityActionSheetAsync(TriagePatientItem item)
        {
            var options = PriorityHelper.AllLevels
                .Select(l => PriorityHelper.GetDisplayName(l))
                .Concat(new[] { "Cancelar" })
                .ToArray();

            var selected = await DisplayActionSheet(
                $"Prioridad — {item.FullName}",
                "Cancelar",
                null,
                options.Take(options.Length - 1).ToArray());

            if (string.IsNullOrEmpty(selected) || selected == "Cancelar")
                return;

            var level = PriorityHelper.AllLevels
                .FirstOrDefault(l => PriorityHelper.GetDisplayName(l) == selected);

            await _viewModel.ChangePriorityAsync(item, level);
        }

        private async void OnRegisterConsultationClicked(object sender, EventArgs e)
        {
            var item = GetItemFromView(sender as Element);
            if (item == null) return;

            await DisplayAlert(
                "Registrar consulta",
                "El registro de consultas se implementará en una tarea posterior. Se abre el detalle del paciente.",
                "Continuar");

            await Shell.Current.GoToAsync($"{nameof(PatientDetailPage)}?id={item.Id}");
        }
    }
}
