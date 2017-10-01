using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace HMIControl
{
    public class RoundScale : RoundDial
    {

        protected override void OnRender(DrawingContext drawingContext)
        {
            double minimum = this.Minimum;
            double maximum = this.Maximum;
            double tickFrequency = this.TickFrequency;
            int minTicksCount = this.MinTicksCount;
            double majorTickValuesOffset = this.MajorTicksOffset;
            double startAngle = base.StartAngle;
            double endAngle = base.EndAngle;
            double ringThickness = base.RingThickness;
            double scale = range / (maximum - minimum);
            Point point = new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);

            Point point2 = new Point(point.X, 0.0);
            Point point3 = new Point(point.X, ringThickness);
            Point point4 = new Point(point.X, ringThickness / 2);
            var stops = this.RangeColors;
            if (stops.Count != 0)
            {
                bool sign = base.SweepDirection == SweepDirection.Clockwise;
                double x = 0.0;
                double angle = 450 - startAngle;
                Color startColor = stops[0].Color;
                Color color = stops[0].Color;
                Size size = new Size(point.X, point.Y);
                Size size2 = new Size(Math.Abs(size.Width - ringThickness), Math.Abs(size.Height - ringThickness));
                for (int i = 0; i <= stops.Count; i++)
                {
                    double scaleAngle;
                    if (i < stops.Count)
                    {
                        double offset = stops[i].Offset;
                        if (offset > maximum)
                        {
                            offset = maximum;
                        }
                        scaleAngle = (offset - x) * scale;
                        x = offset;
                        color = stops[i].Color;
                    }
                    else
                    {
                        scaleAngle = (maximum - x) * scale;
                        x = maximum;
                    }
                    RotateTransform transform1 = new RotateTransform(angle, point.X, point.Y);
                    Point point12 = transform1.Transform(point2);
                    Point point13 = transform1.Transform(point3);
                    transform1.Angle = scaleAngle;
                    StreamGeometry geometry = new StreamGeometry();
                    using (StreamGeometryContext context = geometry.Open())
                    {
                        context.BeginFigure(point12, true, true);
                        context.ArcTo(transform1.Transform(point12), size, 0.0, scaleAngle > 180.0,
                            sign ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true, true);
                        context.LineTo(transform1.Transform(point13), true, true);
                        context.ArcTo(point13, size2, 0.0, scaleAngle > 180.0, sign ? SweepDirection.Counterclockwise :
                        SweepDirection.Clockwise, true, true);
                    }
                    geometry.Freeze();
                    transform1 = new RotateTransform(angle + (scaleAngle / 2.0), 0.5, 0.5);
                    drawingContext.DrawGeometry(new LinearGradientBrush(sign ? startColor : color, sign ? color : startColor,
                        transform1.Transform(new Point(0.0, 0.5)),
                        transform1.Transform(new Point(1.0, 0.5))), null, geometry);
                    //drawingContext.DrawGeometry(Brushes.Blue, null, geometry);
                    if (x >= maximum)
                    {
                        break;
                    }
                    startColor = color;
                    angle += scaleAngle;
                }
            }
            var ticks = this.Ticks;
            if (ticks.Count == 0 && tickFrequency == 0.0)
                return;
            int count = ticks.Count == 0 ? Convert.ToInt32((maximum - minimum) / tickFrequency) : ticks.Count;
            double angle1 = range / count;
            RotateTransform transform = new RotateTransform(0.0, point.X, point.Y);
            transform.Angle = 450 - startAngle;
            Point point5 = new Point(point.X, majorTickValuesOffset);
            Pen pen2 = new Pen(base.Foreground, base.StrokeThickness * 2.0);
            string text = "";
            for (int i = 0; i <= count; i++)
            {
                drawingContext.DrawLine(pen2, transform.Transform(point2), transform.Transform(point3));
                text = (i == count ? maximum : ticks.Count != 0 ? ticks[i] : i * tickFrequency).ToString(this.TickStringFormat);
                FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                       new Typeface(base.FontFamily, base.FontStyle, base.FontWeight, base.FontStretch), base.FontSize, base.Foreground);
                Point origin = transform.Transform(point5);
                drawingContext.PushTransform(new ScaleTransform(1.0, 1.0, origin.X, origin.Y));
                origin.X -= formattedText.Width / 2.0;
                origin.Y -= formattedText.Height / 2.0;
                drawingContext.DrawText(formattedText, origin);
                drawingContext.Pop();
                transform.Angle += angle1;
            }
            if (minTicksCount > 0)
            {
                count *= minTicksCount;
                angle1 = range / count;
                Pen pen = new Pen(base.Foreground, base.StrokeThickness);
                transform.Angle = 450 - startAngle;
                for (int j = 0; j < count; j++)
                {
                    drawingContext.DrawLine(pen, transform.Transform(point2), transform.Transform(point4));
                    transform.Angle += angle1;
                }
            }
            if (!string.IsNullOrEmpty(base.Caption))
            {
                double size = base.FontSize;
                FormattedText formattedText = new FormattedText(base.Caption, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface(base.FontFamily, base.FontStyle, base.FontWeight, base.FontStretch), size, base.Foreground);
                drawingContext.DrawText(formattedText, new Point((base.ActualWidth - Caption.Length * size) / 2.0, 0.2 * base.ActualHeight));
            }
        }

    }
}
