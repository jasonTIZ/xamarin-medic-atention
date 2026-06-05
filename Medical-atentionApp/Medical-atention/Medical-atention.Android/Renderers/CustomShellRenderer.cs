using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Google.Android.Material.BottomNavigation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(Shell), typeof(Medical_atention.Droid.Renderers.CustomShellRenderer))]

namespace Medical_atention.Droid.Renderers
{
    public class CustomShellRenderer : ShellRenderer
    {
        public CustomShellRenderer(Context context) : base(context) { }

        protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
            => new CustomShellBottomNavViewAppearanceTracker(this, shellItem);
    }

    public class CustomShellBottomNavViewAppearanceTracker : ShellBottomNavViewAppearanceTracker
    {
        static readonly Color SelectedColor = Color.ParseColor("#2196F3");
        static readonly Color UnselectedColor = Color.ParseColor("#757575");
        static readonly ColorStateList ItemColorStateList = CreateItemColorStateList();

        public CustomShellBottomNavViewAppearanceTracker(IShellContext shellContext, ShellItem shellItem)
            : base(shellContext, shellItem) { }

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);
            bottomView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;
            bottomView.ItemTextColor = ItemColorStateList;
            bottomView.ItemIconTintList = ItemColorStateList;
        }

        static ColorStateList CreateItemColorStateList()
        {
            var states = new[]
            {
                new[] { global::Android.Resource.Attribute.StateChecked },
                new[] { -global::Android.Resource.Attribute.StateChecked }
            };

            var colors = new[] { SelectedColor, UnselectedColor };
            return new ColorStateList(states, colors);
        }
    }
}
