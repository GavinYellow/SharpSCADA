using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace HMIControl
{

    public class GaugeBase : RangeBase
    {
        public static readonly DependencyProperty TicksProperty = DependencyProperty.Register("Ticks", typeof(DoubleCollection), typeof(GaugeBase),
            new FrameworkPropertyMetadata(new DoubleCollection(), FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register("TickFrequency", typeof(double), typeof(GaugeBase),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MinTicksCountProperty = DependencyProperty.Register("MinTicksCount", typeof(int), typeof(GaugeBase),
            new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TickStringFormatProperty = DependencyProperty.Register("TickStringFormat", typeof(string), typeof(GaugeBase));
        public static DependencyProperty IsMoveToPointEnabledProperty = DependencyProperty.Register("IsMoveToPointEnabled", typeof(bool), typeof(GaugeBase));
        public static readonly DependencyProperty OriginProperty = DependencyProperty.Register("Origin", typeof(double), typeof(GaugeBase));
        public static readonly DependencyProperty CaptionProperty = HMIControlBase.CaptionProperty.AddOwner(typeof(GaugeBase));

        protected ObservableCollection<GradientStop> rangeColors = new ObservableCollection<GradientStop>();

        static GaugeBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GaugeBase), new FrameworkPropertyMetadata(typeof(GaugeBase)));
        }

        public DoubleCollection Ticks
        {
            get
            {
                return (DoubleCollection)base.GetValue(TicksProperty);
            }
            set
            {
                base.SetValue(TicksProperty, value);
            }
        }

        public double TickFrequency
        {
            get
            {
                return (double)base.GetValue(TickFrequencyProperty);
            }
            set
            {
                base.SetValue(TickFrequencyProperty, value);
            }
        }

        public int MinTicksCount
        {
            get
            {
                return (int)base.GetValue(MinTicksCountProperty);
            }
            set
            {
                base.SetValue(MinTicksCountProperty, value);
            }
        }

        public string TickStringFormat
        {
            get
            {
                return (string)base.GetValue(TickStringFormatProperty);
            }
            set
            {
                base.SetValue(TickStringFormatProperty, value);
            }
        }


        public bool IsMoveToPointEnabled
        {
            get
            {
                return (bool)base.GetValue(IsMoveToPointEnabledProperty);
            }
            set
            {
                base.SetValue(IsMoveToPointEnabledProperty, value);
            }
        }

        public string Caption
        {
            get
            {
                return base.GetValue(CaptionProperty).ToString();
            }
            set
            {
                base.SetValue(CaptionProperty, value);
            }
        }

        public ObservableCollection<GradientStop> RangeColors
        {
            get
            {
                return rangeColors;
            }
            set
            {
                rangeColors = value;
            }
        }
    }
}
