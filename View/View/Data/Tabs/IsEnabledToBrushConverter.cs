using System;
using System.Windows.Data;
using System.Windows.Media;

namespace View.Data.Tabs
{
    public class IsEnabledToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                {
                    return new SolidColorBrush(Colors.LightGreen);
                }
            }
            return new SolidColorBrush(Colors.LightPink);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
