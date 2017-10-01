using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HMIControl
{
    public class BoolToVisibleOrHidden : IValueConverter
    {
        #region Constructors
        /// <summary>
        /// The default constructor
        /// </summary>
        public BoolToVisibleOrHidden() { }
        #endregion

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
        #endregion
    }

    public class StoreCVToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value * 127);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Bool2Visible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (bool)value ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StoreCVToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value == 0.0) ? 0 : 0.25;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int16_ScaleStatusConverter : IValueConverter
    {
        string[] list = new string[] { "空闲", "加料", "卸料", "完成", "等待", "皮重检测", "", "", "暂停", "保持" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short index = (short)value;
            string ret = string.Empty;
            for (int i = 0; i < list.Length; i++)
            {
                if (((1<< i) & index) != 0)
                    ret += list[i] + ".";
            }
            return ret.TrimEnd('.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int16_MixerStatusConverter : IValueConverter
    {
        string[] list = new string[] { "空闲", "启动电机", "干混", "湿混", "卸料", "卸料", "完成", "", "暂停", "保持" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short index = (short)value;
            string ret = string.Empty;
            for (int i = 0; i < list.Length; i++)
            {
                if (((1<< i) & index) != 0)
                    ret += list[i] + ".";
            }
            return ret.TrimEnd('.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int16_LiquidBufStatusConverter : IValueConverter
    {
        string[] list = new string[] { "空闲", "", "等待", "卸料", "完成", "异常", "喷吹", "", "暂停", "保持" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short index = (short)value;
            string ret = string.Empty;
            for (int i = 0; i < list.Length; i++)
            {
                if (((1<< i) & index) != 0)
                    ret += list[i] + ".";
            }
            return ret.TrimEnd('.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int16_HandAddStatusConverter : IValueConverter
    {
        string[] list = new string[] { "空闲", "添加", "等待", "等待", "卸料", "完成", "错误", "", "暂停", "保持" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short index = (short)value;
            string ret = string.Empty;
            for (int i = 0; i < list.Length; i++)
            {
                if (((1<< i) & index) != 0)
                    ret += list[i] + ".";
            }
            return ret.TrimEnd('.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int16_LiquidAddStatusConverter : IValueConverter
    {
        string[] list = new string[] { "空闲", "加料", "等待", "等待", "卸料", "完成", "错误", "皮重检测", "暂停", "保持" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short index = (short)value;
            string ret = string.Empty;
            for (int i = 0; i < list.Length; i++)
            {
                if (((1<< i) & index) != 0)
                    ret += list[i] + ".";
            }
            return ret.TrimEnd('.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int16_CheckStatusConverter : IValueConverter
    {
        string[] list = new string[] { "空闲", "延时", "捡重", "等待", "卸料", "完成", "错误", "进料", "暂停", "保持" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short index = (short)value;
            string ret = string.Empty;
            for (int i = 0; i < list.Length; i++)
            {
                if (((1<< i) & index) != 0)
                    ret += list[i] + ".";
            }
            return ret.TrimEnd('.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int16_BufferStatusConverter : IValueConverter
    {
        string[] list = new string[] { "空闲", "", "等待", "卸料", "完成", "", "", "", "暂停", "" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short index = (short)value;
            string ret = string.Empty;
            for (int i = 0; i < list.Length; i++)
            {
                if (((1<< i) & index) != 0)
                    ret += list[i] + ".";
            }
            return ret.TrimEnd('.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Int16_GrindStatusConverter : IValueConverter
    {
        string[] list = new string[] { "空闲", "启动设备", "初始调节", "启动调节", "运行调节", "过载", "自动关机", "急停", "暂停", "" };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            short index = (short)value;
            string ret = string.Empty;
            for (int i = 0; i < list.Length; i++)
            {
                if (((1<< i) & index) != 0)
                    ret += list[i] + ".";
            }
            return ret.TrimEnd('.');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
