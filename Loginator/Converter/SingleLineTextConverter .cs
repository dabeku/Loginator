using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Loginator.Converter {

    public class SingleLineTextConverter : IValueConverter {

        private const string STRING_NEWLINE_WIN = "\r\n";
        private const string STRING_NEWLINE_UNIX = "\n";
        private const string STRING_SPACING = " ";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value == null) {
                return null;
            }
            string s = (string)value;
            s = s.Replace(STRING_NEWLINE_WIN, STRING_SPACING);
            s = s.Replace(STRING_NEWLINE_UNIX, STRING_SPACING);
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
