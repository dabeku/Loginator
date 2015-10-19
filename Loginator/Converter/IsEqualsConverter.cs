using System;
using System.Globalization;
using System.Windows.Data;

namespace Loginator.Converter {
    public class IsEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}