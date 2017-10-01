using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace HMIControl
{
    public class LinkLine : FrameworkElement, ITagReader
    {
        #region 用于定义连线形状的依赖属性
        public static readonly DependencyProperty LineBrushProperty =
            DependencyProperty.Register("LineBrush", typeof(SolidColorBrush), typeof(LinkLine),
            new FrameworkPropertyMetadata(Brushes.DarkGray, FrameworkPropertyMetadataOptions.AffectsRender));
        public SolidColorBrush LineBrush
        {
            get
            {
                return (SolidColorBrush)base.GetValue(LineBrushProperty);
            }
            set
            {
                base.SetValue(LineBrushProperty, value);
            }
        }

        public static readonly DependencyProperty RunningProperty =
    DependencyProperty.Register("Running", typeof(bool), typeof(LinkLine),
    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public bool Running
        {
            get
            {
                return (bool)base.GetValue(RunningProperty);
            }
            set
            {
                base.SetValue(RunningProperty, value);
            }
        }

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(LinkLine),
            new FrameworkPropertyMetadata(2.5, FrameworkPropertyMetadataOptions.AffectsRender));
        public double Thickness
        {
            get
            {
                return (double)base.GetValue(ThicknessProperty);
            }
            set
            {
                base.SetValue(ThicknessProperty, value);
            }
        }

        public static readonly DependencyProperty DashStyleProperty =
            DependencyProperty.Register("DashStyle", typeof(DashStyle), typeof(LinkLine),
            new FrameworkPropertyMetadata(DashStyles.Solid, FrameworkPropertyMetadataOptions.AffectsRender));
        public DashStyle DashStyle
        {
            get
            {
                return (DashStyle)base.GetValue(DashStyleProperty);
            }
            set
            {
                base.SetValue(DashStyleProperty, value);
            }
        }
        #endregion

        #region 用于定义被连接空间的依赖属性
        public static readonly DependencyProperty OriginInfoProperty =
            DependencyProperty.Register("OriginInfo", typeof(ConnectInfo), typeof(LinkLine),
            new FrameworkPropertyMetadata(ConnectInfo.Empty, FrameworkPropertyMetadataOptions.AffectsRender));
        public ConnectInfo OriginInfo
        {
            get { return (ConnectInfo)base.GetValue(OriginInfoProperty); }
            set { base.SetValue(OriginInfoProperty, value); }
        }

        public static readonly DependencyProperty TargetInfoProperty =
            DependencyProperty.Register("TargetInfo", typeof(ConnectInfo), typeof(LinkLine),
            new FrameworkPropertyMetadata(ConnectInfo.Empty, FrameworkPropertyMetadataOptions.AffectsRender));
        public ConnectInfo TargetInfo
        {
            get { return (ConnectInfo)base.GetValue(TargetInfoProperty); }
            set { base.SetValue(TargetInfoProperty, value); }
        }

        #endregion

        public LinkLine()
        {
        }

        public LinkLine(ConnectInfo info1, ConnectInfo info2)
        {
            this.OriginInfo = info1;
            this.TargetInfo = info2;
        }

        public string[] GetActions()
        {
            return new string[] { TagActions.RUN };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (!(OriginInfo == ConnectInfo.Empty || TargetInfo == ConnectInfo.Empty))
            {
                bool run = Running;
                Brush brush = run ? Brushes.GreenYellow : LineBrush;
                double thickness = this.Thickness;
                Pen pen = new Pen(brush, thickness);
                pen.StartLineCap = PenLineCap.Round;
                pen.EndLineCap = PenLineCap.Round;
                pen.LineJoin = PenLineJoin.Round;
                pen.DashStyle = DashStyle;
                Panel.SetZIndex(this, run ? 1000 : 0);
                List<Point> linepoint = PathFinder.GetConnectionLine(OriginInfo, TargetInfo, false);
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext context = geometry.Open())
                {
                    context.BeginFigure(linepoint[0], true, false);
                    linepoint.RemoveAt(0);
                    context.PolyLineTo(linepoint, true, false);
                }
                drawingContext.DrawGeometry(null, pen, geometry);
            }
        }

        #region ITagReader接口实现
        public Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.RUN:
                    var _funcRun = tagChanged as Func<bool>;
                    if (_funcRun != null)
                    {
                        return delegate
                        {
                            Running = _funcRun();
                            //LineBrush = _funcRun() ? Brushes.GreenYellow : LineBrush.GetAnimationBaseValue(LineBrushProperty) as SolidColorBrush; 
                        }; //
                    }
                    else return null;
                default:
                    return null;
            }
        }

        public static readonly DependencyProperty TagReadTextProperty =
                    HMIControlBase.TagReadTextProperty.AddOwner(typeof(LinkLine));
        [Category("HMI")]
        public string TagReadText
        {
            get { return ((string)base.GetValue(TagReadTextProperty)); }
            set { base.SetValue(TagReadTextProperty, value); }
        }

        protected IList<ITagLink> children;
        public IList<ITagLink> Children
        {
            get { return children; }
        }

        public string Node { get { return this.Name; } }
        #endregion
    }
}
