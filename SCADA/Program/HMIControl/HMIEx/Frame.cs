using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HMIControl
{
    public class Frame : Decorator
    {
        public static DependencyProperty AspectRatioProperty = DependencyProperty.Register("AspectRatio", typeof(Size), typeof(Frame),
            new FrameworkPropertyMetadata(new Size(1.0, 1.0), FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static DependencyProperty BevelWidthProperty = DependencyProperty.Register("BevelWidth", typeof(double), typeof(Frame),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static DependencyProperty FrameOffsetProperty = DependencyProperty.Register("FrameOffset", typeof(double), typeof(Frame),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static DependencyProperty DesignProperty = DependencyProperty.Register("Design", typeof(FrameDesign), typeof(Frame),
           new FrameworkPropertyMetadata(FrameDesign.Round, FrameworkPropertyMetadataOptions.AffectsRender));
        public static DependencyProperty BevelColorProperty = DependencyProperty.Register("BevelColor", typeof(Color), typeof(Frame),
           new FrameworkPropertyMetadata(Colors.Gray, FrameworkPropertyMetadataOptions.AffectsRender));
        public static DependencyProperty FrameColorProperty = DependencyProperty.Register("FrameColor", typeof(Color), typeof(Frame),
           new FrameworkPropertyMetadata(Colors.Gray, FrameworkPropertyMetadataOptions.AffectsRender));

        private Size applyAspectRatio(Size size)
        {
            if ((this.AspectRatio.Height != 0.0) && (this.AspectRatio.Width != 0.0))
            {
                double num = this.AspectRatio.Height / this.AspectRatio.Width;
                if ((size.Width * num) < size.Height)
                {
                    size.Height = size.Width * num;
                    return size;
                }
                size.Width = size.Height / num;
            }
            return size;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            //this.Child.Measure(arrangeBounds);
            Size size = this.applyAspectRatio(arrangeBounds);
            var element = this.Child as FrameworkElement;
            if (element != null)
            {
                double bevel = (Design == FrameDesign.Round ? 6 : 4) * this.BevelWidth + this.FrameOffset;
                element.Width = size.Width - bevel;
                element.Height = size.Height - bevel;
            }
            //this.Child.Arrange(new Rect(size));
            return base.ArrangeOverride(size);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return base.MeasureOverride(this.applyAspectRatio(constraint));
        }

        protected override void OnRender(DrawingContext dc)
        {
            double bevel = this.BevelWidth;
            //double glass = this.GlassEffectOffset;
            //dc.DrawEllipse(Background, null, point, width, height);
            if (bevel != 0)
            {
                Color color = this.BevelColor;
                Color color1 = this.FrameColor;
                Brush brush = new SolidColorBrush(color.AddColorDelta(-80));
                Pen pen = new Pen(brush, 0.5);
                Brush brush1, brush2, brush3, brush4;
                double width, height;

                switch (this.Design)
                {
                    case FrameDesign.Round:
                        width = this.ActualWidth / 2.0;
                        height = this.ActualHeight / 2.0;
                        Point point = new Point(width, height);
                        brush1 = color.CreateLinearTwoGradientsBrush(90.0, 80, -80);
                        brush2 = new SolidColorBrush(color);
                        brush3 = color.CreateLinearTwoGradientsBrush(90.0, -80, 80);
                        brush4 = new RadialGradientBrush
                        {
                            GradientStops = new GradientStopCollection
                        { 
                            new GradientStop(Colors.Black,1),
                            new GradientStop(color1,0)
                        },
                            GradientOrigin = new Point(0.2, 0.2),
                            Center = new Point(0.1, 0.1),
                            RadiusX = 1,
                            RadiusY = 1
                        };
                        dc.DrawEllipse(brush1, pen, point, width, height);
                        dc.DrawEllipse(brush2, null, point, width - bevel, height - bevel);
                        dc.DrawEllipse(brush3, null, point, width - 2 * bevel, height - 2 * bevel);
                        dc.DrawEllipse(brush4, pen, point, width - 3 * bevel, height - 3 * bevel);
                        break;
                    case FrameDesign.Square:
                        width = this.ActualWidth;
                        height = this.ActualHeight;
                        brush1 = color.CreateLinearTwoGradientsBrush(90.0, 80, -80);
                        brush2 = color.CreateLinearTwoGradientsBrush(90.0, -80, 80);
                        brush3 = new SolidColorBrush(color1);
                        dc.DrawRectangle(brush1, pen, new Rect(0, 0, width, height));
                        dc.DrawRectangle(brush2, null, new Rect(bevel, bevel, width - 2 * bevel, height - 2 * bevel));
                        dc.DrawRectangle(brush3, pen, new Rect(2 * bevel, 2 * bevel, width - 4 * bevel, height - 4 * bevel));
                        break;
                    case FrameDesign.SquareGlass:
                        width = this.ActualWidth;
                        height = this.ActualHeight;
                        brush1 = color.CreateLinearTwoGradientsBrush(0, 80, -80);
                        brush2 = color.CreateLinearGradientsBrush(180.0, 0.05, Colors.White);
                        brush3 = new LinearGradientBrush(color1, color1.AddColorDelta(-80), new Point(0, 0.3), new Point(1, 0.7));
                        dc.DrawRoundedRectangle(brush1, null, new Rect(0, 0, width, height), bevel, bevel);
                        dc.DrawRoundedRectangle(brush2, null, new Rect(bevel, bevel, width - 2 * bevel, height - 2 * bevel), bevel, bevel);
                        dc.DrawRoundedRectangle(brush3, pen, new Rect(2 * bevel, 2 * bevel, width - 4 * bevel, height - 4 * bevel), bevel, bevel);
                        break;
                }

            }
        }

        public Size AspectRatio
        {
            get
            {
                return (Size)base.GetValue(AspectRatioProperty);
            }
            set
            {
                base.SetValue(AspectRatioProperty, value);
            }
        }

        public FrameDesign Design
        {
            get
            {
                return (FrameDesign)base.GetValue(DesignProperty);
            }
            set
            {
                base.SetValue(DesignProperty, value);
            }
        }

        public double BevelWidth
        {
            get
            {
                return (double)base.GetValue(BevelWidthProperty);
            }
            set
            {
                base.SetValue(BevelWidthProperty, value);
            }
        }

        public double FrameOffset
        {
            get
            {
                return (double)base.GetValue(FrameOffsetProperty);
            }
            set
            {
                base.SetValue(FrameOffsetProperty, value);
            }
        }

        public Color BevelColor
        {
            get
            {
                return (Color)base.GetValue(BevelColorProperty);
            }
            set
            {
                base.SetValue(BevelColorProperty, value);
            }
        }

        public Color FrameColor
        {
            get
            {
                return (Color)base.GetValue(FrameColorProperty);
            }
            set
            {
                base.SetValue(FrameColorProperty, value);
            }
        }
    }

    public enum FrameDesign
    {
        Round,
        Square,
        SquareGlass
    }
}

