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
            return level switch
            {
                2 => Color.FromHex("#E53E3E"),
                1 => Color.FromHex("#DD6B20"),
                _ => Color.FromHex("#38A169")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
