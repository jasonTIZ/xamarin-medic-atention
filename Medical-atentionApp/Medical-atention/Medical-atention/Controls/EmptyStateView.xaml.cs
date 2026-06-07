using System.Windows.Input;
using Xamarin.Forms;

namespace Medical_atention.Controls
{
    public partial class EmptyStateView : ContentView
    {
        public static readonly BindableProperty IconProperty =
            BindableProperty.Create(nameof(Icon), typeof(string), typeof(EmptyStateView), "📋");

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyStateView), "Sin resultados");

        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(EmptyStateView), null,
                propertyChanged: (b, _, n) => ((EmptyStateView)b).HasSubtitle = !string.IsNullOrEmpty(n as string));

        public static readonly BindableProperty ActionTextProperty =
            BindableProperty.Create(nameof(ActionText), typeof(string), typeof(EmptyStateView), null,
                propertyChanged: (b, _, __) => ((EmptyStateView)b).RefreshHasAction());

        public static readonly BindableProperty ActionCommandProperty =
            BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(EmptyStateView), null,
                propertyChanged: (b, _, __) => ((EmptyStateView)b).RefreshHasAction());

        public static readonly BindableProperty HasSubtitleProperty =
            BindableProperty.Create(nameof(HasSubtitle), typeof(bool), typeof(EmptyStateView), false);

        public static readonly BindableProperty HasActionProperty =
            BindableProperty.Create(nameof(HasAction), typeof(bool), typeof(EmptyStateView), false);

        public EmptyStateView()
        {
            InitializeComponent();
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public string ActionText
        {
            get => (string)GetValue(ActionTextProperty);
            set => SetValue(ActionTextProperty, value);
        }

        public ICommand ActionCommand
        {
            get => (ICommand)GetValue(ActionCommandProperty);
            set => SetValue(ActionCommandProperty, value);
        }

        public bool HasSubtitle
        {
            get => (bool)GetValue(HasSubtitleProperty);
            private set => SetValue(HasSubtitleProperty, value);
        }

        public bool HasAction
        {
            get => (bool)GetValue(HasActionProperty);
            private set => SetValue(HasActionProperty, value);
        }

        private void RefreshHasAction()
        {
            HasAction = !string.IsNullOrEmpty(ActionText) && ActionCommand != null;
        }
    }
}
