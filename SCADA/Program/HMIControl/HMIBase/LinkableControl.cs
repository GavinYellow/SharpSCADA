using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace HMIControl
{
    public abstract class LinkableControl : Control
    {
        #region 用于定义被连接端口的依赖属性
        public static readonly DependencyProperty LeftPinProperty =
            DependencyProperty.Register("LeftPin", typeof(ConnectInfo), typeof(LinkableControl),
            new FrameworkPropertyMetadata(ConnectInfo.Empty));
        public ConnectInfo LeftPin
        {
            get { return (ConnectInfo)base.GetValue(LeftPinProperty); }
            set { base.SetValue(LeftPinProperty, value); }
        }

        public static readonly DependencyProperty RightPinProperty =
            DependencyProperty.Register("RightPin", typeof(ConnectInfo), typeof(LinkableControl),
            new FrameworkPropertyMetadata(ConnectInfo.Empty));
        public ConnectInfo RightPin
        {
            get { return (ConnectInfo)base.GetValue(RightPinProperty); }
            set { base.SetValue(RightPinProperty, value); }
        }

        public static readonly DependencyProperty TopPinProperty =
            DependencyProperty.Register("TopPin", typeof(ConnectInfo), typeof(LinkableControl),
            new FrameworkPropertyMetadata(ConnectInfo.Empty));
        public ConnectInfo TopPin
        {
            get { return (ConnectInfo)base.GetValue(TopPinProperty); }
            set { base.SetValue(TopPinProperty, value); }
        }

        public static readonly DependencyProperty BottomPinProperty =
            DependencyProperty.Register("BottomPin", typeof(ConnectInfo), typeof(LinkableControl),
            new FrameworkPropertyMetadata(ConnectInfo.Empty));
        public ConnectInfo BottomPin
        {
            get { return (ConnectInfo)base.GetValue(BottomPinProperty); }
            set { base.SetValue(BottomPinProperty, value); }
        }

        #endregion        

        public ControlAdorner LinkableAdorner{ get; set; }//在OnRenderSizeChanged函数中创建

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (DesignerProperties.GetIsInDesignMode(this))            
            {               
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null) 
                {
                    if (LinkableAdorner == null)
                    {
                        LinkableAdorner = new ControlAdorner(this);
                        LinkableAdorner.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        public static readonly DependencyProperty IsLinkDragOverProperty =
            DependencyProperty.Register("IsLinkDragOver", typeof(bool), typeof(LinkableControl),
            new FrameworkPropertyMetadata(false, IsLinkDragOverPropertyChanged));

        static void IsLinkDragOverPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            LinkableControl lc = o as LinkableControl;
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(lc);
            if (lc.LinkableAdorner != null && adornerLayer != null)
            {
                if (e.NewValue.Equals(true))
                {
                    lc.LinkableAdorner.Visibility = Visibility.Visible;
                    adornerLayer.Add(lc.LinkableAdorner);
                }
                else
                {
                    lc.LinkableAdorner.Visibility = Visibility.Hidden;
                    adornerLayer.Remove(lc.LinkableAdorner);
                }
            }
        }

        public bool IsLinkDragOver
        {
            get
            {
                return (bool)GetValue(IsLinkDragOverProperty);
            }
            set
            {
                SetValue(IsLinkDragOverProperty, value);
            }
        }

        public abstract LinkPosition[] GetLinkPositions();
    }

    public struct LinkPosition
    {
        public Point Position;
        public ConnectOrientation Orient;

        public LinkPosition(Point p, ConnectOrientation o)
        {
            this.Position = p;
            this.Orient = o;
        }
    }

}
