using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace View.Error.ErrorItems
{
    class BoolToBorderWidthConverter : IValueConverter
    {
        private static readonly int BORDER_WIDTH = 2;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? 
                    new Thickness() { Bottom = BORDER_WIDTH, Top = BORDER_WIDTH, Left = BORDER_WIDTH, Right = BORDER_WIDTH } : 
                    new Thickness() { Bottom = 0, Top = 0, Left = 0, Right = 0 };
            }

            return new Thickness() { Bottom = 0, Top = 0, Left = 0, Right = 0 };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
