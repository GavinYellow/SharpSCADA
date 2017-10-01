using System.Windows;
using System.Windows.Media;

namespace HMIControl
{
    public class RoundIndicator : ControlBase
    {
        public static DependencyProperty DesignProperty = Tacho.NeedleDesignProperty.AddOwner(typeof(RoundIndicator),
           new FrameworkPropertyMetadata(NeedleDesign.Standard, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty EndAngleProperty = RoundGuageBase.EndAngleProperty.AddOwner(typeof(RoundIndicator),
           new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty StartAngleProperty = RoundGuageBase.StartAngleProperty.AddOwner(typeof(RoundIndicator),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        static RoundIndicator()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RoundIndicator), new FrameworkPropertyMetadata(typeof(RoundIndicator)));
        }

        public override void EndInit()
        {
            base.EndInit();
            GaugeBase templatedParent = base.TemplatedParent as GaugeBase;
            if (templatedParent != null)
            {
                this.BindToTemplatedParent(StartAngleProperty, RoundGuageBase.StartAngleProperty);
                this.BindToTemplatedParent(EndAngleProperty, RoundGuageBase.EndAngleProperty);
                this.BindToTemplatedParent(DesignProperty, Tacho.NeedleDesignProperty);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double width = this.ActualWidth / 2.0;
            double height = this.ActualHeight / 2.0;
            Point point = new Point(width, height);
            Color color = this.ControlBrush.Color;
            Brush brush = new SolidColorBrush(color.AddColorDelta(-80));
            Brush brush1 = color.CreateLinearTwoGradientsBrush(90.0, 80, -80);
            Brush brush2 = new SolidColorBrush(color);
            Brush brush3 = color.CreateLinearTwoGradientsBrush(90.0, -80, 80);
            Pen pen = new Pen(brush, StrokeThickness);
            
            drawingContext.PushTransform(new RotateTransform(450 - this.StartAngle, width, height));
            StreamGeometry geometry;
            switch (Design)
            {
                case NeedleDesign.Standard:
                    geometry = new StreamGeometry();
                    using (StreamGeometryContext context = geometry.Open())
                    {
                        context.BeginFigure(new Point(0.85 * width, 0), true, true);
                        context.PolyLineTo(new Point[] { new Point(1.15 * width, 0), new Point(1.45 * width, height), new Point(0.55 * width, height) }, false, false);
                    }
                    geometry.Freeze();
                    drawingContext.DrawGeometry(Brushes.Red, pen, geometry);
                    drawingContext.DrawEllipse(brush1, pen, point, width * 1.5, width * 1.5);
                    drawingContext.DrawEllipse(brush2, null, point, width * 1.2, width * 1.2);
                    break;
                case NeedleDesign.Classic:
                    drawingContext.DrawLine(new Pen(Brushes.Blue,5),new Point(width, 0), new Point(width, 1.3 * height));
                    drawingContext.DrawEllipse(brush1, pen, point, width * 1.5, width * 1.5);
                    drawingContext.DrawEllipse(brush2, null, point, width * 1.2, width * 1.2);
                    break;
                case NeedleDesign.Shape:
                    geometry = new StreamGeometry();
                    using (StreamGeometryContext context = geometry.Open())
                    {
                        context.BeginFigure(new Point(width, 0), true, true);
                        context.LineTo(new Point(1.5 * width, height), true, false);
                        context.ArcTo(new Point(0.5 * width, height), new Size(width * 1.2, width * 1.2), 0.0, true, SweepDirection.Clockwise, true, true);
                    }
                    geometry.Freeze();
                    drawingContext.DrawGeometry(ControlBrush, pen, geometry);
                    break;
                case NeedleDesign.Thin:
                    drawingContext.DrawLine(pen, new Point(width, 0), new Point(width, height));
                    break;
            }
        
            //base.OnRender(drawingContext);
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

        public NeedleDesign Design
        {
            get
            {
                return (NeedleDesign)base.GetValue(DesignProperty);
            }
            set
            {
                base.SetValue(DesignProperty, value);
            }
        }
    }
}
