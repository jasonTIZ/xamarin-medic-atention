using Medical_atention.Helpers;
using Medical_atention.Models;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Medical_atention.Converters
{
    public class PriorityLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PriorityLevel level)
                return PriorityHelper.GetBadgeColor(level);
            return Color.FromHex("#718096");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
