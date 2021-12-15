using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GameBarTimer.Converters
{
    public class BooleanSwitchToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if(!(value is bool))
            {
                throw new ArgumentException(nameof(value));
            }

            var boolValue = (bool)value;

            var parameterBoolValue = (parameter == null || !(parameter is bool)) ? false : (bool)parameter;

            return boolValue == parameterBoolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
