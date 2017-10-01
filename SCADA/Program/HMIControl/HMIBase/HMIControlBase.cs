using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace HMIControl
{
    public class HMIControlBase : LinkableControl, ITagReader
    {
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(HMIControlBase));

        public static readonly DependencyProperty DeviceNameProperty = DependencyProperty.Register("DeviceName", typeof(string), typeof(HMIControlBase));

        public static readonly DependencyProperty ShowCaptionProperty =
            DependencyProperty.Register("ShowCaption", typeof(bool), typeof(HMIControlBase), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty AlarmProperty =
            DependencyProperty.Register("Alarm", typeof(bool), typeof(HMIControlBase), 
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, 
            new PropertyChangedCallback(OnValueChanged)));

        protected static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            HMIControlBase control = obj as HMIControlBase;
            if (control != null)
                control.UpdateState();
        }

        protected virtual void UpdateState() { }
        

        [Category("HMI")]
        public string Caption
        {
            get
            { return (string)GetValue(CaptionProperty); }
            set
            { SetValue(CaptionProperty, value); }
        }

        [Category("HMI")]
        public string DeviceName
        {
            get
            { return (string)GetValue(DeviceNameProperty); }
            set
            { SetValue(DeviceNameProperty, value); }
        }

        [Category("HMI")]
        public bool ShowCaption
        {
            get
            { return (bool)GetValue(ShowCaptionProperty); }
            set
            { SetValue(ShowCaptionProperty, value); }
        }

        [Category("HMI")]
        public bool Alarm
        {
            get
            { return (bool)GetValue(AlarmProperty); }
            set
            { SetValue(AlarmProperty, value); }
        }


        public HMIControlBase()
            : base()
        {
            LayoutUpdated += new EventHandler(HMIControlBase_LayoutUpdated);
            //this.CommandBindings.AddRange(BindingCommandHandler());
        }

        void HMIControlBase_LayoutUpdated(object sender, EventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this)) LayoutUpdated -= HMIControlBase_LayoutUpdated;
            var list = GetLinkPositions();
            if (list.Length == 0) return;
            var pt = TranslatePoint(new Point(), Parent as UIElement);
            double x = pt.X;
            double y = pt.Y;
            if (double.IsNaN(x) || double.IsNaN(y)) return;
            MatrixTransform matrixTran = RenderTransform as MatrixTransform;//此处也应考虑矩阵变换后的情况。
            var matrix = matrixTran == null ? Matrix.Identity : matrixTran.Matrix;
            double w = ActualWidth;
            double h = ActualHeight;
            ConnectInfo info = ConnectInfo.Empty;
            info.DesignerRect = new Rect(x, y, w, h);
            foreach (var lp in list)
            {
                Point point = matrixTran == null ? lp.Position : matrix.Transform(lp.Position);
                info.Position = new Point(w * point.X + x, h * point.Y + y);
                switch (lp.Orient)
                {
                    case ConnectOrientation.Left:
                        info.Orient = matrix.M11 < 0 ? ConnectOrientation.Right : ConnectOrientation.Left;
                        LeftPin = info;
                        break;
                    case ConnectOrientation.Right:
                        info.Orient = matrix.M11 < 0 ? ConnectOrientation.Left : ConnectOrientation.Right;
                        RightPin = info;
                        break;
                    case ConnectOrientation.Top:
                        info.Orient = matrix.M22 < 0 ? ConnectOrientation.Bottom : ConnectOrientation.Top;
                        TopPin = info;
                        break;
                    case ConnectOrientation.Bottom:
                        info.Orient = matrix.M22 < 0 ? ConnectOrientation.Top : ConnectOrientation.Bottom;
                        BottomPin = info;
                        break;
                }
            }
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            this.Focus();
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[0];
        }

        protected IList<ITagLink> children;
        public IList<ITagLink> Children
        {
            get { return children; }
        }

        public ConnectInfo GetInfo1(ConnectOrientation orient, Point p)
        {
            ConnectInfo info = ConnectInfo.Empty;
            double w = this.ActualWidth;
            double h = this.ActualHeight;
            info.Position = new Point((w - 10) * p.X + 5, (h - 10) * p.Y + 5);
            info.DesignerRect = new Rect(0, 0, w, h);
            info.Orient = orient;
            return info;
        }

        #region ITagReader接口实现
        public static readonly DependencyProperty TagReadTextProperty =
            DependencyProperty.Register("TagReadText", typeof(string), typeof(HMIControlBase));
        [Category("HMI")]
        public string TagReadText
        {
            get { return ((string)base.GetValue(TagReadTextProperty)); }
            set { base.SetValue(TagReadTextProperty, value); }
        }

        public virtual string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.ALARM };
        }

        public virtual Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.DEVICENAME:
                    var _funcname = tagChanged as Func<string>;
                    if (_funcname != null)
                    {
                        return delegate
                        {
                            this.DeviceName = _funcname();
                        };
                    }
                    else return null;
                case TagActions.VISIBLE:
                    var _funcVisible = tagChanged as Func<bool>;
                    if (_funcVisible != null)
                    {
                        return delegate
                        {
                            this.Visibility = _funcVisible() ? Visibility.Visible : Visibility.Hidden;
                        };
                    }
                    else return null;
                case TagActions.CAPTION:
                    var _funcCaption = tagChanged as Func<string>;
                    if (_funcCaption != null)
                    {
                        return delegate { this.Caption = _funcCaption(); };
                    }
                    else return null;
                case TagActions.ALARM:
                    var _funcAlarm = tagChanged as Func<bool>;
                    if (_funcAlarm != null)
                    {
                        return delegate { this.Alarm = _funcAlarm(); };
                    }
                    return null;
            }
            return null;
        }

        public string Node
        {
            get { return this.Name; }
        }

        #endregion



    }
}
