using Medical_atention.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(ConsultationId), "consultationId")]
    [QueryProperty(nameof(LocalId), "localId")]
    public partial class ConsultationDetailPage : ContentPage
    {
        private readonly ConsultationDetailViewModel _viewModel = new ConsultationDetailViewModel();
        private int? _serverId;
        private int _localId;

        public ConsultationDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        public string ConsultationId
        {
            set
            {
                if (int.TryParse(value, out var id) && id > 0)
                    _serverId = id;
                TryLoad();
            }
        }

        public string LocalId
        {
            set
            {
                if (int.TryParse(value, out var id))
                    _localId = id;
                TryLoad();
            }
        }

        private void TryLoad()
        {
            if (_serverId.HasValue || _localId > 0)
                _ = _viewModel.LoadAsync(_serverId, _localId);
        }
    }
}
