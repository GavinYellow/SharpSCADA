using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace HMIControl
{
    public class PinAdorner : Adorner
    {
        private PathGeometry linkLine;
        private Panel designCanvas;
        private LinkPin srcPin;
        public LinkPin SourcePin
        {
            get
            {
                return srcPin;
            }
        }

        private Pen drawingPen;

        private LinkableControl hitLink;
        public LinkableControl HitLinkableControl
        {
            get
            {
                return hitLink;
            }
            private set
            {
                if (hitLink != value)
                {
                    if (hitLink != null) hitLink.IsLinkDragOver = false;

                    hitLink = value;

                    if (hitLink != null) hitLink.IsLinkDragOver = true;
                }              
            }
        }

        private LinkPin hitPin;
        public LinkPin HitPin
        {
            get
            {
                return hitPin;
            }
            set
            {
                if (hitPin != value)
                {
                    if (hitPin != null) hitPin.Background = Brushes.Navy;

                    hitPin = value;

                    if (hitPin != null) hitPin.Background = Brushes.DarkGoldenrod;
                }
            }
        }

        public PinAdorner(Panel designer, LinkPin sourcePin) : base(designer)
        {
            this.designCanvas = designer;
            this.srcPin = sourcePin;
            drawingPen = new Pen(Brushes.LightSlateGray, 1);
            drawingPen.LineJoin = PenLineJoin.Round;
            this.Cursor = Cursors.Cross;

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!IsMouseCaptured) CaptureMouse();
                HitTesting(e.GetPosition(designCanvas));
                linkLine = GetPathGeometry(e.GetPosition(this));
                InvalidateVisual();
            }
            else
            {
                if (IsMouseCaptured) ReleaseMouseCapture();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawGeometry(null, drawingPen, this.linkLine);            
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
            // without a background the OnMouseMove event would not be fired
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
        }

        private PathGeometry GetPathGeometry(Point position)
        {
            PathGeometry geometry = new PathGeometry();

            ConnectOrientation targetOrient;
            if (HitPin != null)
                targetOrient = HitPin.Orientation;
            else
                targetOrient = ConnectOrientation.None;

            List<Point> pathPoints = PathFinder.GetConnectionLine(srcPin.GetInfo(), position, targetOrient);

            if (pathPoints.Count > 0)
            {
                PathFigure figure = new PathFigure();
                figure.StartPoint = pathPoints[0];
                pathPoints.RemoveAt(0);
                figure.Segments.Add(new PolyLineSegment(pathPoints, true));
                geometry.Figures.Add(figure);
            }

            return geometry;
        }
        
        private void HitTesting(Point p)
        {
            var element = (srcPin.Parent as ControlAdorner).AdornedElement;

            VisualTreeHelper.HitTest(designCanvas, null, new HitTestResultCallback(HitTestResultCB1), new PointHitTestParameters(p));
            DependencyObject selectedObj = result == null ? null : result.VisualHit;
            while (selectedObj != null && selectedObj != designCanvas)
            {
                if (selectedObj is LinkableControl && selectedObj != element)
                {
                    HitLinkableControl = selectedObj as LinkableControl;
                    
                    AdornerLayer layer = AdornerLayer.GetAdornerLayer(HitLinkableControl);
                    PointHitTestParameters param = new PointHitTestParameters(designCanvas.TranslatePoint(p, layer));
                    VisualTreeHelper.HitTest(layer, null, new HitTestResultCallback(HitTestResultCB1), param);
                    selectedObj = result == null ? null : result.VisualHit;
                    while (selectedObj != null && selectedObj != designCanvas)
                    {
                        if (selectedObj is LinkPin)
                        {
                            HitPin = (LinkPin)selectedObj;
                            return;
                        }
                        selectedObj = VisualTreeHelper.GetParent(selectedObj);
                    }
                    return;
                }
                selectedObj = VisualTreeHelper.GetParent(selectedObj);
            }
            HitLinkableControl = null;
            HitPin = null;

        }

        HitTestResultBehavior HitTestResultCB1(HitTestResult r)
        {
            result = r;
            return HitTestResultBehavior.Stop;
        }

        HitTestResult result;


    }
}
