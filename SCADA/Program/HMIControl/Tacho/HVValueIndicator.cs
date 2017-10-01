using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HMIControl
{
    public class HVValueIndicator : HVIndicator
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            double width = base.ActualWidth;
            double height = base.ActualHeight;
            Brush brush = new LinearGradientBrush(new GradientStopCollection{
                new GradientStop(Colors.Tomato,0.0),
                new GradientStop(Colors.Black,1.0)});
            if (this.Orientation == Orientation.Vertical)
            {
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext context = geometry.Open())
                {
                    context.BeginFigure(new Point(width, 0), true, true);
                    context.PolyLineTo(new Point[] { new Point(0, height * 0.5), new Point(width, height) }, true, false);
                }
                drawingContext.DrawGeometry(brush, null, geometry);
            }
            else
            {
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext context = geometry.Open())
                {
                    context.BeginFigure(new Point(0, height), true, true);
                    context.PolyLineTo(new Point[] { new Point(width * 0.5, 0), new Point(width, height) }, true, false);
                }
                drawingContext.DrawGeometry(brush, null, geometry);
            }
        }
    }
}
