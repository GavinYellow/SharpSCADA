using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace HMIControl
{
    public class FrameAdorner : Adorner
    {
        public static DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(SolidColorBrush), typeof(TextAdorner),
            new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));

        public SolidColorBrush BorderBrush
        {
            get
            {
                return (SolidColorBrush)base.GetValue(BorderBrushProperty);
            }
            set
            {
                base.SetValue(BorderBrushProperty, value);
            }
        }

        public FrameAdorner(UIElement adornedElement)
            : base(adornedElement)
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect rect = new Rect(this.AdornedElement.RenderSize);
            //rect.Inflate(5, 5);
            drawingContext.DrawRoundedRectangle(null, new Pen(BorderBrush, 1.0f), rect, 1, 1);
        }
    }
}
