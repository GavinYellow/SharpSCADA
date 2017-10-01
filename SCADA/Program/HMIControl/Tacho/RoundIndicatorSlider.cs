using System.Windows;
using System.Windows.Media;

namespace HMIControl
{

    public class RoundIndicatorSlider : RoundIndicator
    {
        static RoundIndicatorSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RoundIndicatorSlider), new FrameworkPropertyMetadata(typeof(RoundIndicatorSlider)));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double width = this.ActualWidth / 2.0;
            double height = this.ActualHeight / 2.0;
            Point point = new Point(width, height);
            Color color = this.ControlBrush.Color;
            var br = this.Background as SolidColorBrush;
            Brush brush = br ?? Brushes.Green;
            Brush brush1 = color.CreateLinearTwoGradientsBrush(90.0, 80, -80);
            Brush brush2 = new SolidColorBrush(color);
            Brush brush3 = color.CreateLinearTwoGradientsBrush(90.0, -80, 80);
            Pen pen = new Pen(BorderBrush, base.StrokeThickness);
            drawingContext.PushTransform(new RotateTransform(450 - this.StartAngle, width, height));
            drawingContext.DrawEllipse(brush1, pen, point, width, width);
            drawingContext.DrawEllipse(brush2, null, point, width * 0.9, width * 0.9);
            drawingContext.DrawEllipse(brush3, null, new Point(width, height*0.5), width * 0.15, width * 0.15);
            drawingContext.DrawEllipse(brush, null, new Point(width, height * 0.5), width * 0.125, width * 0.125);
            //base.OnRender(drawingContext);
        }
    }
}
