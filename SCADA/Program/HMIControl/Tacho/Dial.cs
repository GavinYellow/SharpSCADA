using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace HMIControl
{
    public class Dial : ControlBase
    {
        public static readonly DependencyProperty MaximumProperty = GaugeBase.MaximumProperty.AddOwner(typeof(Dial),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceMaxReading));
        public static readonly DependencyProperty MinimumProperty = GaugeBase.MinimumProperty.AddOwner(typeof(Dial),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceMinReading));
        public static readonly DependencyProperty OriginProperty = GaugeBase.OriginProperty.AddOwner(typeof(Dial));

        public static readonly DependencyProperty MinTicksCountProperty = GaugeBase.MinTicksCountProperty.AddOwner(typeof(Dial),
            new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TickFrequencyProperty = GaugeBase.TickFrequencyProperty.AddOwner(typeof(Dial),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TickStringFormatProperty = GaugeBase.TickStringFormatProperty.AddOwner(typeof(Dial));

        public static readonly DependencyProperty TicksProperty = GaugeBase.TicksProperty.AddOwner(typeof(Dial),
            new FrameworkPropertyMetadata(new DoubleCollection(), FrameworkPropertyMetadataOptions.AffectsRender));

        //public static readonly DependencyProperty RangeColorsProperty = DependencyProperty.Register("RangeColors", typeof(ObservableCollection<GradientStop>), typeof(TickScale),
        //    new FrameworkPropertyMetadata(new ObservableCollection<GradientStop>(), FrameworkPropertyMetadataOptions.AffectsRender));
        private ObservableCollection<GradientStop> rangeColors = new ObservableCollection<GradientStop>();

        public override void EndInit()
        {
            base.EndInit();
            GaugeBase templatedParent = base.TemplatedParent as GaugeBase;
            if (templatedParent != null)
            {
                this.BindToTemplatedParent(TicksProperty, GaugeBase.TicksProperty);
                this.BindToTemplatedParent(TickFrequencyProperty, GaugeBase.TickFrequencyProperty);
                this.BindToTemplatedParent(MinTicksCountProperty, GaugeBase.MinTicksCountProperty);
                this.BindToTemplatedParent(MinimumProperty, GaugeBase.MinimumProperty);
                this.BindToTemplatedParent(MaximumProperty, GaugeBase.MaximumProperty);
                this.BindToTemplatedParent(OriginProperty, GaugeBase.OriginProperty);
                this.rangeColors = templatedParent.RangeColors;
            }
        }

        private static object CoerceMinReading(DependencyObject d, object value)
        {
            Dial tick = (Dial)d;
            double minimum = (double)value;
            double maximum = tick.Maximum;
            double origin = tick.Origin;
            if (minimum > maximum)
            {
                double temp = minimum;
                minimum = maximum;
                tick.Maximum = temp;
            }
            if (origin < minimum)
            {
                tick.Origin = minimum;
            }
            return minimum;
        }

        //在CoerceMaxReading加入强制判断赋值
        private static object CoerceMaxReading(DependencyObject d, object value)
        {
            Dial tick = (Dial)d;
            double maximum = (double)value;
            double minimum = tick.Minimum;
            double origin = tick.Origin;
            if (minimum > maximum)
            {
                double temp = minimum;
                tick.Minimum = maximum;
                maximum = temp;
            }
            if (origin > maximum)
            {
                tick.Origin = maximum;
            }
            return maximum;
        }

        public double Maximum
        {
            get
            {
                return (double)base.GetValue(MaximumProperty);
            }
            set
            {
                base.SetValue(MaximumProperty, value);
            }
        }

        public double Minimum
        {
            get
            {
                return (double)base.GetValue(MinimumProperty);
            }
            set
            {
                base.SetValue(MinimumProperty, value);
            }
        }

        public double Origin
        {
            get
            {
                return (double)base.GetValue(OriginProperty);
            }
            set
            {
                base.SetValue(OriginProperty, value);
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
    }
}
