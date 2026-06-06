using Android.Content;
using Android.Content.Res;
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
        const int SelectedColorArgb = unchecked((int)0xFF2196F3);
        const int UnselectedColorArgb = unchecked((int)0xFF757575);

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
            int[][] states =
            {
                new[] { global::Android.Resource.Attribute.StateChecked },
                new[] { -global::Android.Resource.Attribute.StateChecked }
            };

            int[] colors = { SelectedColorArgb, UnselectedColorArgb };
            return new ColorStateList(states, colors);
        }
    }
}
