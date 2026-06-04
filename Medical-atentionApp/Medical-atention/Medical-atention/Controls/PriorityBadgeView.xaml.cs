using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Medical_atention.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PriorityBadgeView : ContentView
    {
        public static readonly BindableProperty PriorityProperty = BindableProperty.Create(
            nameof(Priority),
            typeof(string),
            typeof(PriorityBadgeView),
            string.Empty,
            propertyChanged: OnVisualPropertyChanged);

        public static readonly BindableProperty SizeProperty = BindableProperty.Create(
            nameof(Size),
            typeof(PriorityBadgeSize),
            typeof(PriorityBadgeView),
            PriorityBadgeSize.Medium,
            propertyChanged: OnVisualPropertyChanged);

        public PriorityBadgeView()
        {
            InitializeComponent();
            UpdateAppearance();
        }

        public string Priority
        {
            get => (string)GetValue(PriorityProperty);
            set => SetValue(PriorityProperty, value);
        }

        public PriorityBadgeSize Size
        {
            get => (PriorityBadgeSize)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is PriorityBadgeView badge)
                badge.UpdateAppearance();
        }

        private void UpdateAppearance()
        {
            var info = PriorityBadgeTheme.Resolve(Priority);
            var metrics = PriorityBadgeTheme.GetMetrics(Size);

            BadgeFrame.BackgroundColor = info.BackgroundColor;
            BadgeFrame.BorderColor = info.BorderColor;
            BadgeFrame.Padding = metrics.Padding;
            BadgeFrame.CornerRadius = (float)metrics.CornerRadius;

            IconLabel.Text = info.Icon;
            IconLabel.TextColor = info.ForegroundColor;
            IconLabel.FontSize = metrics.IconFontSize;

            TextLabel.Text = info.Label;
            TextLabel.TextColor = info.ForegroundColor;
            TextLabel.FontSize = metrics.TextFontSize;

            BadgeLayout.Spacing = metrics.Spacing;
        }
    }

    internal static class PriorityBadgeTheme
    {
        // Design system aligned with app palette (#2C7BE5 primary, semantic status colors)
        private const string UrgentBg = "#FED7D7";
        private const string UrgentFg = "#C53030";
        private const string UrgentBorder = "#FC8181";

        private const string HighBg = "#FEEBC8";
        private const string HighFg = "#C05621";
        private const string HighBorder = "#F6AD55";

        private const string MediumBg = "#FEFCBF";
        private const string MediumFg = "#B7791F";
        private const string MediumBorder = "#ECC94B";

        private const string LowBg = "#C6F6D5";
        private const string LowFg = "#276749";
        private const string LowBorder = "#68D391";

        private const string UnknownBg = "#EDF2F7";
        private const string UnknownFg = "#718096";
        private const string UnknownBorder = "#CBD5E0";

        public static PriorityBadgeInfo Resolve(string priority)
        {
            switch (priority?.Trim().ToLowerInvariant())
            {
                case "urgent":
                    return new PriorityBadgeInfo(UrgentBg, UrgentFg, UrgentBorder, "Urgente", "!");
                case "high":
                    return new PriorityBadgeInfo(HighBg, HighFg, HighBorder, "Alta", "↑");
                case "medium":
                    return new PriorityBadgeInfo(MediumBg, MediumFg, MediumBorder, "Media", "−");
                case "low":
                    return new PriorityBadgeInfo(LowBg, LowFg, LowBorder, "Baja", "✓");
                default:
                    return new PriorityBadgeInfo(UnknownBg, UnknownFg, UnknownBorder, "—", "•");
            }
        }

        public static PriorityBadgeMetrics GetMetrics(PriorityBadgeSize size)
        {
            switch (size)
            {
                case PriorityBadgeSize.Small:
                    return new PriorityBadgeMetrics(6, 3, 10, 10, 11, 4);
                case PriorityBadgeSize.Large:
                    return new PriorityBadgeMetrics(12, 7, 18, 16, 15, 6);
                default:
                    return new PriorityBadgeMetrics(8, 5, 14, 13, 12, 5);
            }
        }
    }

    internal readonly struct PriorityBadgeInfo
    {
        public PriorityBadgeInfo(string backgroundHex, string foregroundHex, string borderHex, string label, string icon)
        {
            BackgroundColor = Color.FromHex(backgroundHex);
            ForegroundColor = Color.FromHex(foregroundHex);
            BorderColor = Color.FromHex(borderHex);
            Label = label;
            Icon = icon;
        }

        public Color BackgroundColor { get; }
        public Color ForegroundColor { get; }
        public Color BorderColor { get; }
        public string Label { get; }
        public string Icon { get; }
    }

    internal readonly struct PriorityBadgeMetrics
    {
        public PriorityBadgeMetrics(double padH, double padV, double iconSize, double textSize, double spacing, double cornerRadius)
        {
            Padding = new Thickness(padH, padV);
            IconFontSize = iconSize;
            TextFontSize = textSize;
            Spacing = spacing;
            CornerRadius = cornerRadius;
        }

        public Thickness Padding { get; }
        public double IconFontSize { get; }
        public double TextFontSize { get; }
        public double Spacing { get; }
        public double CornerRadius { get; }
    }
}
