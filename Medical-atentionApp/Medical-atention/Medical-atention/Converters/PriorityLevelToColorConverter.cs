using System;
using System.Globalization;
using Xamarin.Forms;

namespace Medical_atention.Converters
{
    public class PriorityLevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var level = value is int i ? i : 0;
            switch (level)
            {
                case 2:
                    return Color.FromHex("#E53E3E");
                case 1:
                    return Color.FromHex("#DD6B20");
                default:
                    return Color.FromHex("#38A169");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
