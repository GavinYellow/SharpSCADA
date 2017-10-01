using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HMIControl
{
    public class HVScale : HVDial
    {
        public static readonly DependencyProperty MajorTicksOffsetProperty = DependencyProperty.Register("MajorTicksOffset", typeof(double), typeof(HVScale),
           new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty FlippingProperty = DependencyProperty.Register("Flipping", typeof(SelectiveScrollingOrientation), typeof(HVScale));

        protected override void OnRender(DrawingContext drawingContext)
        {
            double width = base.ActualWidth;
            double height = base.ActualHeight;
            double minimum = this.Minimum;
            double maximum = this.Maximum;
            double range = maximum - minimum;
            bool flag = base.Orientation == Orientation.Vertical;
            double tickFrequency = this.TickFrequency;
            double majorTicksOffset = this.MajorTicksOffset;
            var stops = this.RangeColors;
            var ticks = this.Ticks;
            int minTicksCount = this.MinTicksCount;
            int count = ticks.Count == 0 ? Convert.ToInt32((maximum - minimum) / tickFrequency) : ticks.Count;
            if (stops.Count != 0)
            {
                double num5;
                double num4 = 0.0;
                if (minimum < 0.0)
                {
                    num4 = Math.Abs(minimum);
                }
                GradientStopCollection gradientStopCollection = new GradientStopCollection();
                for (int i = 0; i < stops.Count; i++)
                {
                    GradientStop stop = stops[i].Clone();
                    stop.Offset = (stop.Offset + num4) / range;
                    gradientStopCollection.Add(stop);
                }
                num5 = flag ? -90.0 : 0.0;
                var brush = new LinearGradientBrush(gradientStopCollection, new Point(0.0, 0.5), new Point(1.0, 0.5));
                brush.RelativeTransform = new RotateTransform(num5, 0.5, 0.5);
                brush.Freeze();
                drawingContext.DrawRectangle(brush, null, new Rect(0, 0, width, height));
            }

            string text = "";
            Pen pen2 = new Pen(base.Foreground, this.StrokeThickness * 2.0);
            double num = flag ? height / count : width / count;
            double scaleX = 1.0;
            double scaleY = 1.0;
            if (this.Flipping == SelectiveScrollingOrientation.Horizontal || this.Flipping == SelectiveScrollingOrientation.Both)
            {
                scaleX = -1.0;
            }
            if (this.Flipping == SelectiveScrollingOrientation.Vertical || this.Flipping == SelectiveScrollingOrientation.Both)
            {
                scaleY = -1.0;
            }
            for (int i = 0; i <= count; i++)
            {
                double y = i * num;
                text = (i == count ? minimum : ticks.Count != 0 ? ticks[i] : maximum - i * tickFrequency).ToString(this.TickStringFormat);
                FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                       new Typeface(base.FontFamily, base.FontStyle, base.FontWeight, base.FontStretch), base.FontSize, base.Foreground);
                if (flag)
                {
                    drawingContext.DrawLine(pen2, new Point(0.0, y), new Point(width, y));
                    drawingContext.PushTransform(new ScaleTransform(scaleX, scaleY, majorTicksOffset - (formattedText.Width / 2.0), y));
                    drawingContext.DrawText(formattedText, new Point(majorTicksOffset - formattedText.Width, y - (formattedText.Height / 2.0)));
                    drawingContext.Pop();
                }
                else
                {
                    drawingContext.DrawLine(pen2, new Point(y, 0.0), new Point(y, height));
                    drawingContext.PushTransform(new ScaleTransform(scaleX, scaleY, y, majorTicksOffset - (formattedText.Height / 2.0)));
                    drawingContext.DrawText(formattedText, new Point(y - (formattedText.Width / 2.0), majorTicksOffset - formattedText.Height));
                    drawingContext.Pop();
                }
            }
            if (minTicksCount > 0)
            {
                count *= minTicksCount;
                num = flag ? height / count : width / count;
                Pen pen = new Pen(base.Foreground, this.StrokeThickness);
                for (int i = 0; i < count; i++)
                {
                    if (flag)
                    {
                        drawingContext.DrawLine(pen, new Point(width/2, i * num), new Point(width, i * num));
                    }
                    else
                    {
                        drawingContext.DrawLine(pen, new Point(i * num, height/2), new Point(i * num, height));
                    }
                }
            }
            
        }

        public SelectiveScrollingOrientation Flipping
        {
            get
            {
                return (SelectiveScrollingOrientation)base.GetValue(FlippingProperty);
            }
            set
            {
                base.SetValue(FlippingProperty, value);
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

    }
}
