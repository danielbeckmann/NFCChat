using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NfcChat.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a visability state. True = Visible, False = Collapsed
        /// </summary>
        /// <param name="value">The boolean value which gets converted</param>
        /// <param name="targetType">Type: Visibility</param>
        /// <param name="parameter">Provide any parameter to invert the visibility state</param>
        /// <param name="culture">The culture</param>
        /// <returns>Visibility</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                var booleanValue = (bool)value;
                if (parameter != null)
                {
                    booleanValue = !booleanValue;
                }
                if (booleanValue)
                {
                    return System.Windows.Visibility.Visible;
                }
            }
            return System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Not implemented!
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
