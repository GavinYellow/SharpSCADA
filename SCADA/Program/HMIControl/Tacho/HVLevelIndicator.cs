using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HMIControl
{
    public class HVLevelIndicator : HVIndicator
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            double scale; 
            double minimum = this.Minimum;
            double maximum = this.Maximum;
            double origin = base.Origin;
            double value = base.Value;
            Brush brush;
            if (this.RenderModel == RenderModel.Simple)
            {
                brush = this.ControlBrush;
            }
            else
            {
                brush = ControlBrush.Color.CreateLinearThreeGradientsBrush(0.0, -30, 160);
            }
            if (value < minimum)
            {
                value = minimum;
            }
            else if (value > maximum)
            {
                value = maximum;
            }
            double num1 = 0.0;
            double num2 = 0.0;
            double actualWidth = base.ActualWidth;
            double actualHeight = base.ActualHeight;
            if (this.Orientation == Orientation.Vertical)
            {
                scale = base.ActualHeight / (maximum - minimum);
                actualHeight = Math.Abs(value - origin) * scale;
                num2 = base.ActualHeight - ((origin - minimum) * scale);
                if (value > origin)
                {
                    num2 -= actualHeight;
                }
            }
            else
            {
                scale = base.ActualWidth / (maximum - minimum);
                actualWidth = Math.Abs(value - origin) * scale;
                num1 = (origin - minimum) * scale;
                if (value < origin)
                {
                    num1 -= actualWidth;
                }
            }
            drawingContext.DrawRectangle(base.Background, null, new Rect(new Size(base.ActualWidth, base.ActualHeight)));
            drawingContext.DrawRectangle(brush, null, new Rect(num1, num2, actualWidth, actualHeight));
        }

    }
}
