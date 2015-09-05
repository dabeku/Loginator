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
                if (level == LogLevel.TRACE) {
                    return new SolidColorBrush(Colors.DarkGray);
                } else if (level == LogLevel.DEBUG) {
                    return new SolidColorBrush(Colors.Gray);
                } else if (level == LogLevel.INFO) {
                    return new SolidColorBrush(Colors.Green);
                } else if (level == LogLevel.WARN) {
                    return new SolidColorBrush(Colors.Orange);
                } else if (level == LogLevel.ERROR) {
                    return new SolidColorBrush(Colors.Red);
                } else if (level == LogLevel.FATAL) {
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
