using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace HMIControl
{

    public class LinkPin : FrameworkElement
    {
        public static readonly DependencyProperty BackgroudProperty =
            DependencyProperty.Register("Background", typeof(SolidColorBrush), typeof(LinkPin),
            new FrameworkPropertyMetadata(Brushes.Navy, FrameworkPropertyMetadataOptions.AffectsRender));
        public SolidColorBrush Background
        {
            get
            {
                return (SolidColorBrush)base.GetValue(BackgroudProperty);
            }
            set
            {
                base.SetValue(BackgroudProperty, value);
            }
        }

        ControlAdorner parent;        

        public Point RelativePosition { get; set; }//PIN在控件上的相对位置

        public ConnectOrientation Orientation { get; set; }
        
        public LinkPin() { }

        public LinkPin(ConnectOrientation o, Point p)
        {
            this.Cursor = Cursors.Cross;
            this.RelativePosition = p;
            this.Orientation = o;    
   
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Brush brush = Background;
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 14, 14));
            drawingContext.DrawRectangle(Brushes.Lavender, new Pen(brush, 1), new Rect(0, 0, 10, 10));
        }

        public ConnectInfo GetInfo()
        {            
            parent = Parent as ControlAdorner;
            var element = parent.AdornedElement as HMIControlBase;
            return element.GetInfo1(this.Orientation, this.RelativePosition);
        }
    }
}
