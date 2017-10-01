using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataService;
namespace CoreTest
{
    public class AlarmConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value is DBNull)
                return Brushes.White;
            switch ((Severity)value)
            {
                case Severity.Error:
                    return Brushes.Red;
                case Severity.High:
                    return Brushes.OrangeRed;
                case Severity.Information:
                    return Brushes.Blue;
                case Severity.Low:
                    return Brushes.LightBlue;
                case Severity.Medium:
                    return Brushes.Yellow;
                case Severity.MediumHigh:
                    return Brushes.Orange;
                case Severity.MediumLow:
                    return Brushes.LightYellow;
                case Severity.Normal:
                    return Brushes.Green;
                default:
                    return Brushes.White;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class NegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return -(double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
