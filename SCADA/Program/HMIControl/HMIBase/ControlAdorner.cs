using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace HMIControl
{
    public class ControlAdorner : Adorner
    {
        List<LinkPin> _children;
        public List<LinkPin> Children { get { return _children; } }

        public ControlAdorner(LinkableControl AdorneredItem)
            : base(AdorneredItem)
        {   
            _children = new List<LinkPin>(4);
            foreach (var pos in AdorneredItem.GetLinkPositions())
            {
                LinkPin pin = new LinkPin(pos.Orient, pos.Position);
                this._children.Add(pin);
                this.AddLogicalChild(pin);
                this.AddVisualChild(pin);
            }
        }        
        
        protected override int VisualChildrenCount
        {
            get
            {
                return _children.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                return _children.GetEnumerator();
            }
        }         

        protected override Size ArrangeOverride(Size finalSize)
        {
            var Size = this.AdornedElement.RenderSize;
            foreach (var pin in _children)
            {
                double x = (Size.Width - 10) * pin.RelativePosition.X;
                double y = (Size.Height - 10) * pin.RelativePosition.Y;
                pin.Arrange(new Rect(new Point(x, y), pin.DesiredSize));
            }
            return finalSize;
        }
    }
}
