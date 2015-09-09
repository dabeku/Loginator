using Backend.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Loginator.Converter {
    
    public class LevelToForegroundConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            if (value != null) {
                string level = value.ToString();
                if (level == LoggingLevel.TRACE) {
                    return new SolidColorBrush(Colors.DarkGray);
                } else if (level == LoggingLevel.DEBUG) {
                    return new SolidColorBrush(Colors.Gray);
                } else if (level == LoggingLevel.INFO) {
                    return new SolidColorBrush(Colors.Green);
                } else if (level == LoggingLevel.WARN) {
                    return new SolidColorBrush(Colors.Orange);
                } else if (level == LoggingLevel.ERROR) {
                    return new SolidColorBrush(Colors.Red);
                } else if (level == LoggingLevel.FATAL) {
                    return new SolidColorBrush(Colors.DarkViolet);
                }
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException("[ConvertBack] not implemented");
        }
    }
}
