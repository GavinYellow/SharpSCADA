using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace HMIControl
{
    public class TachoScale : RoundDial
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            double minimum = this.Minimum;
            double maximum = this.Maximum;
            double tickFrequency = this.TickFrequency;
            double majorTickFrequency = this.MinTicksCount;
            double majorTickValuesOffset = this.MajorTicksOffset;
            double startAngle = base.StartAngle;
            double endAngle = base.EndAngle;
            double ringThickness = base.RingThickness;
            double scale = (endAngle - startAngle) / (maximum - minimum);
            var stops = this.RangeColors;
            Point point = new Point(base.ActualWidth / 2.0, base.ActualHeight );
            RotateTransform transform = new RotateTransform(0.0, point.X, point.Y);
            Point point2 = new Point(point.X, 0.0);
            Point point3 = new Point(point.X, ringThickness);
            Point point4 = new Point(point.X, ringThickness / 2);
            if (stops.Count != 0)
            {
                double x = 0.0;
                double angle =90- endAngle;
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
                    transform1.Angle = scaleAngle;
                    Point point14 = transform1.Transform(point2);
                    Point point15 = transform1.Transform(point3);
                    StreamGeometry geometry = new StreamGeometry();
                    StreamGeometryContext context = geometry.Open();
                    context.BeginFigure(point2, true, true);
                    context.ArcTo(point14, size, 0.0, scaleAngle > 180.0, SweepDirection.Clockwise, true, true);
                    context.LineTo(point15, true, true);
                    context.ArcTo(point3, size2, 0.0, scaleAngle > 180.0, SweepDirection.Counterclockwise, true, true);
                    context.Close();
                    geometry.Freeze();
                    transform1 = new RotateTransform(angle + (scaleAngle / 2.0), 0.5, 0.5);
                    point2 = transform1.Transform(new Point(0.0, 0.5));
                    point14 = transform1.Transform(new Point(1.0, 0.5));
                    drawingContext.DrawGeometry(new LinearGradientBrush(startColor, color, point2, point14), null, geometry);
                    if (x >= maximum)
                    {
                        break;
                    }
                    startColor = color;
                    angle += scaleAngle;
                }
            }
            if (tickFrequency > 0.0)
            {
                double num1 = tickFrequency - (Math.Truncate((tickFrequency - minimum) / tickFrequency) * tickFrequency);
                transform.Angle =  90- endAngle - ((num1 - minimum) * scale);
                Pen pen = new Pen(base.Foreground, base.StrokeThickness);
                while (num1 <= maximum)
                {
                    drawingContext.DrawLine(pen, transform.Transform(point2), transform.Transform(point4));
                    num1 += tickFrequency;
                    transform.Angle += tickFrequency * scale;
                }
            }
            if (majorTickFrequency > 0.0)
            {
                int index = 0;
                double num1 = majorTickFrequency - (Math.Truncate((majorTickFrequency - minimum) / majorTickFrequency) * majorTickFrequency);
                transform.Angle = 90 - endAngle - ((num1 - minimum) * scale);
                Point point5 = new Point(point.X, majorTickValuesOffset);
                Pen pen2 = new Pen(base.Foreground, base.StrokeThickness * 2.0);
                double scaleX = 1.0;
                double scaleY = 1.0;
                var ticks = this.Ticks;
                while (num1 <= maximum)
                {
                    string text;
                    drawingContext.DrawLine(pen2, transform.Transform(point2), transform.Transform(point3));
                    if (ticks != null && index < ticks.Count)
                    {
                        text = ticks[index].ToString(); ;
                    }
                    else
                    {
                        text = num1.ToString(this.TickStringFormat);
                    }
                    FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                        new Typeface(base.FontFamily, base.FontStyle, base.FontWeight, base.FontStretch), base.FontSize, base.Foreground);
                    Point origin = transform.Transform(point5);
                    drawingContext.PushTransform(new ScaleTransform(scaleX, scaleY, origin.X, origin.Y));
                    origin.X -= formattedText.Width / 2.0;
                    origin.Y -= formattedText.Height / 2.0;
                    drawingContext.DrawText(formattedText, origin);
                    drawingContext.Pop();
                    num1 += majorTickFrequency;
                    transform.Angle += majorTickFrequency * scale;
                    index++;
                }
            }

        } 
    }
}
