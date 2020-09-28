using System;
using System.Windows.Data;
using System.Windows.Media;

namespace View.Data.Channels
{
    /// <summary>
    /// Converts a boolean value to a colored solid brush to indicate whether calibration is used.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class BoolToColorConverter:IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool valAsBool = (bool)value;

            if (valAsBool)
            {
                return new SolidColorBrush(Colors.LightGreen);
            }
            else
                return null;
        }

        /// <summary>
        /// Not implemented!
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }

        #endregion
    }

    /// <summary>
    /// Converts a boolean value to a string value to indicate whether calibration is used.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class BoolToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool valAsBool = (bool)value;

            if (valAsBool)
            {
                return "Calib.";
            }
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }

        #endregion
    }
}
