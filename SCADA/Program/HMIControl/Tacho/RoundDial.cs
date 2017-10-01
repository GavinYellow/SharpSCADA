using System;
using System.Windows;
using System.Windows.Media;

namespace HMIControl
{
    public class RoundDial : Dial
    {
        public static readonly DependencyProperty EndAngleProperty = RoundGuageBase.EndAngleProperty.AddOwner(typeof(RoundDial),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnRangeChanged, EndAngleValueCallback));
        public static readonly DependencyProperty StartAngleProperty = RoundGuageBase.StartAngleProperty.AddOwner(typeof(RoundDial),
            new FrameworkPropertyMetadata(180.0, FrameworkPropertyMetadataOptions.AffectsRender, OnRangeChanged, StartAngleValueCallback));
        public static readonly DependencyProperty SweepDirectionProperty = RoundGuageBase.SweepDirectionProperty.AddOwner(typeof(RoundDial),
            new FrameworkPropertyMetadata(SweepDirection.Clockwise, FrameworkPropertyMetadataOptions.AffectsRender, OnRangeChanged));

        public static readonly DependencyProperty RingThicknessProperty = DependencyProperty.Register("RingThickness", typeof(double), typeof(RoundDial),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty MajorTicksOffsetProperty = DependencyProperty.Register("MajorTicksOffset", typeof(double), typeof(RoundDial));

        protected double range;

        public override void EndInit()
        {
            base.EndInit();
            if (base.TemplatedParent is GaugeBase)
            {
                this.BindToTemplatedParent(EndAngleProperty, RoundGuageBase.EndAngleProperty);
                this.BindToTemplatedParent(StartAngleProperty, RoundGuageBase.StartAngleProperty);
                this.BindToTemplatedParent(SweepDirectionProperty, RoundGuageBase.SweepDirectionProperty);
                this.BindToTemplatedParent(CaptionProperty, RoundGuageBase.CaptionProperty);
            }
        }

        static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoundDial g = d as RoundDial;
            double startAngle = g.StartAngle;
            double endAngle = g.EndAngle;
            if (g.SweepDirection == SweepDirection.Clockwise)
            {
                g.range = startAngle < endAngle ? 360 - (endAngle - startAngle) : startAngle - endAngle;

            }
            else
            {
                g.range = startAngle < endAngle ? startAngle - endAngle : (startAngle - endAngle) - 360;
            };
        }

        internal double AngleFromPoint(Point p)
        {
            p.X -= base.ActualWidth / 2.0;
            p.Y -= base.ActualHeight / 2.0;
            double angle = (StartAngle + (Math.Atan2(p.Y, p.X) * 180.0 / Math.PI)) % 360.0;
            if (SweepDirection == SweepDirection.Clockwise)
                return angle > range ? 0 : angle;
            else
                return angle < 360 + range ? 0 : angle;
        }


        internal double ValueFromAngle(double angle)
        {
            double minimum = base.Minimum;
            double maximum = base.Maximum;
            double startAngle = this.StartAngle;
            double endAngle = this.EndAngle;
            var ticks = this.Ticks;

            //double offset = AngleFromPoint(p);
            if (endAngle != startAngle && angle < range)
            {
                if (ticks.Count == 0)
                {
                    return angle * (maximum - minimum) / range;
                }
                else
                {
                    double x = angle * ticks.Count / range;
                    int y = (int)x;
                    return ticks[y] + ((y < ticks.Count - 1 ? ticks[y + 1] : maximum) - ticks[y]) * (x - y);
                }
            }
            return 0.0;
        }

        internal double AngleFromValue(double value)
        {

            double minimum = this.Minimum;
            double maximum = this.Maximum;
            double startAngle = this.StartAngle;
            double endAngle = this.EndAngle;
            var ticks = this.Ticks;
            if (value <= maximum && value > minimum)
            {
                if (ticks.Count == 0)
                {
                    return value * range / (maximum - minimum);
                }
                else
                {
                    double x = range / ticks.Count;
                    for (int i = 0; i < ticks.Count; i++)
                    {
                        double y = ticks[i];
                        if (value <= y)
                        {
                            return x * (i - 1 + (value - ticks[i - 1]) / (y - ticks[i - 1]));
                        }
                    }
                    int index = ticks.Count - 1;
                    return x * (index + (value - ticks[index]) / (maximum - ticks[index]));
                }
            }
            return 0.0;
        }

        private static object EndAngleValueCallback(DependencyObject d, Object baseValue)
        {
            double endAngle = (double)baseValue;
            while (endAngle < 0.0)
            {
                endAngle += 360.0;
            }
            return endAngle;
        }

        private static object StartAngleValueCallback(DependencyObject d, Object baseValue)
        {
            double startAngle = (double)baseValue;
            while (startAngle < 0.0)
            {
                startAngle += 360.0;
            }
            return startAngle % 360.0;
        }


        public double EndAngle
        {
            get
            {
                return (double)base.GetValue(EndAngleProperty);
            }
            set
            {
                base.SetValue(EndAngleProperty, value);
            }
        }

        public double StartAngle
        {
            get
            {
                return (double)base.GetValue(StartAngleProperty);
            }
            set
            {
                base.SetValue(StartAngleProperty, value);
            }
        }


        public double RingThickness
        {
            get
            {
                return (double)base.GetValue(RingThicknessProperty);
            }
            set
            {
                base.SetValue(RingThicknessProperty, value);
            }
        }

        public double MajorTicksOffset
        {
            get
            {
                return (double)base.GetValue(MajorTicksOffsetProperty);
            }
            set
            {
                base.SetValue(MajorTicksOffsetProperty, value);
            }
        }

        public SweepDirection SweepDirection
        {
            get
            {
                return (SweepDirection)base.GetValue(SweepDirectionProperty);
            }
            set
            {
                base.SetValue(SweepDirectionProperty, value);
            }
        }
    }
}
