using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HMIControl
{
    public class HVIndicator : ControlBase
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(HVIndicator),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MaximumProperty = GaugeBase.MaximumProperty.AddOwner(typeof(HVIndicator),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty MinimumProperty = GaugeBase.MinimumProperty.AddOwner(typeof(HVIndicator),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ValueProperty = GaugeBase.ValueProperty.AddOwner(typeof(HVIndicator),
           new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty OriginProperty = GaugeBase.OriginProperty.AddOwner(typeof(HVIndicator));

        static HVIndicator()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HVIndicator), new FrameworkPropertyMetadata(typeof(HVIndicator)));
        }

        public override void EndInit()
        {
            base.EndInit();
            GaugeBase templatedParent = base.TemplatedParent as GaugeBase;
            if (templatedParent != null)
            {
                this.BindToTemplatedParent(MinimumProperty, GaugeBase.MinimumProperty);
                this.BindToTemplatedParent(MaximumProperty, GaugeBase.MaximumProperty);
                this.BindToTemplatedParent(ValueProperty, GaugeBase.ValueProperty);
                this.BindToTemplatedParent(OriginProperty, GaugeBase.OriginProperty);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {

        }

        public Orientation Orientation
        {
            get
            {
                return (Orientation)base.GetValue(OrientationProperty);
            }
            set
            {
                base.SetValue(OrientationProperty, value);
            }
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

        public double Value
        {
            get
            {
                return (double)base.GetValue(ValueProperty);
            }
            set
            {
                base.SetValue(ValueProperty, value);
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
    }
}
