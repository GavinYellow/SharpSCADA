using System;
using System.Windows;
using System.Windows.Media;

namespace HMIControl
{
    public class RoundSlider : RoundGuageBase
    {
        static RoundSlider()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RoundSlider), new FrameworkPropertyMetadata(typeof(RoundSlider)));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size size = base.ArrangeOverride(finalSize);
            scale.RenderTransformOrigin = new Point(0.5, 0.5);
            indict.RenderTransformOrigin = new Point(0.5, 0.5);
            double width = base.Width * 0.05;
            scale.Width = width * 17;
            scale.Height = width * 17;
            indict.Width = width * 15;
            indict.Height = width * 15;
            //indict.Width = base.Width * 0.7;
            //indict.Height = base.Width * 0.7;
            this.scale.RingThickness = width;
            this.scale.MajorTicksOffset = -width;
            return size;
        }


        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            //this.indict.RenderTransform = new RotateTransform(indict.AngleFromValue(newValue));
        }

       

        internal double SnapToTick(double value)
        {

            double minimum = base.Minimum;
            double maximum = base.Maximum;
            DoubleCollection ticks = this.Ticks;
            
            if ((ticks != null) && (ticks.Count > 0))
            {
                for (int i = 0; i < ticks.Count; i++)
                {
                    double num4 = ticks[i];
                    if (num4== value)
                    {
                        return value;
                    }
                    if ((num4<value) && (num4>minimum))
                    {
                        minimum = num4;
                    }
                    else if ((num4>value) &&(num4<maximum))
                    {
                        maximum = num4;
                    }
                }
            }
            else if ((this.TickFrequency> 0.0))
            {
                minimum = base.Minimum + (Math.Round((double)((value - base.Minimum) / base.TickFrequency)) * this.TickFrequency);
                maximum = Math.Min(base.Maximum, minimum + this.TickFrequency);
            }
            value = (value>(minimum + maximum) * 0.5) ? maximum : minimum;
            return value;
        }
    }
}
