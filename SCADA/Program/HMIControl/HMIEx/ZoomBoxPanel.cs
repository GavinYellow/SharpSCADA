using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HMIControl
{
    public class ZoomBoxPanel : Panel
    {
        // Fields
        private bool animations;
        private Size childSize = new Size(1.0, 1.0);
        private static DependencyProperty ClickZoomDeltaProperty;
        private bool lockContent;
        private static DependencyProperty MaxZoomProperty = DependencyProperty.Register("MaxZoom", typeof(double), typeof(ZoomBoxPanel), new FrameworkPropertyMetadata(1000.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ZoomBoxPanel.PropertyChanged_Zoom), new CoerceValueCallback(ZoomBoxPanel.CoerceMaxZoom)), new ValidateValueCallback(ZoomBoxPanel.ValidateIsPositiveNonZero));
        private static DependencyProperty MaxZoomTickProperty = DependencyProperty.Register("MaxZoomTick", typeof(double), typeof(ZoomBoxPanel), new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ZoomBoxPanel.PropertyChanged_Zoom), new CoerceValueCallback(ZoomBoxPanel.CoerceMaxZoomTick)), null);
        private int minMouseDelta = 0xf4240;
        private const double MINZOOM = 1E-07;
        private static DependencyProperty MinZoomProperty = DependencyProperty.Register("MinZoom", typeof(double), typeof(ZoomBoxPanel), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ZoomBoxPanel.PropertyChanged_Zoom), new CoerceValueCallback(ZoomBoxPanel.CoerceMinZoom)), new ValidateValueCallback(ZoomBoxPanel.ValidateIsPositiveNonZero));
        private static DependencyProperty MinZoomTickProperty = DependencyProperty.Register("MinZoomTick", typeof(double), typeof(ZoomBoxPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ZoomBoxPanel.PropertyChanged_Zoom), new CoerceValueCallback(ZoomBoxPanel.CoerceMinZoomTick)), null);
        public static readonly RoutedEvent ModeChangeEvent;
        private static DependencyProperty MouseModeProperty;
        private double panX;
        private double panY;
        private Cursor prevCursor;
        private double rotateAngle;
        private double rotateCenterX;
        private double rotateCenterY;
        public static RoutedUICommand rotateClockwiseCommand;
        public static RoutedUICommand rotateCounterclockwiseCommand;
        private static DependencyProperty RotateDeltaProperty;
        public static RoutedUICommand rotateHomeCommand;
        public static RoutedUICommand rotateReverseCommand;
        private RotateTransform rotateTransform = new RotateTransform();
        private double scrollDelta = 25.0;
        private Point startMouseCapturePanel = new Point(0.0, 0.0);
        private TransformGroup transformGroup = new TransformGroup();
        private TranslateTransform translateTransform = new TranslateTransform();
        private static DependencyProperty WheelModeProperty;
        private static DependencyProperty WheelZoomDeltaProperty;
        private bool zoomCircularReference;
        private static DependencyProperty ZoomModeProperty;
        private static DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(ZoomBoxPanel), new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ZoomBoxPanel.PropertyChanged_Zoom), new CoerceValueCallback(ZoomBoxPanel.CoerceZoom)), new ValidateValueCallback(ZoomBoxPanel.ValidateIsPositiveNonZero));
        private static DependencyProperty ZoomTickProperty = DependencyProperty.Register("ZoomTick", typeof(double), typeof(ZoomBoxPanel), new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ZoomBoxPanel.PropertyChanged_Zoom), new CoerceValueCallback(ZoomBoxPanel.CoerceZoomTick)), null);
        private ScaleTransform zoomTransform = new ScaleTransform();

        // Events
        public event RoutedEventHandler ModeChange
        {
            add
            {
                base.AddHandler(ModeChangeEvent, value);
            }
            remove
            {
                base.RemoveHandler(ModeChangeEvent, value);
            }
        }

        // Methods
        static ZoomBoxPanel()
        {
            FrameworkPropertyMetadata typeMetadata = new FrameworkPropertyMetadata
            {
                DefaultValue = 20.0
            };
            ClickZoomDeltaProperty = DependencyProperty.Register("ClickZoomDelta", typeof(double), typeof(ZoomBoxPanel), typeMetadata, new ValidateValueCallback(ZoomBoxPanel.ValidateIsPositiveNonZero));
            WheelZoomDeltaProperty = DependencyProperty.Register("WheelZoomDelta", typeof(double), typeof(ZoomBoxPanel), new FrameworkPropertyMetadata(10.0), new ValidateValueCallback(ZoomBoxPanel.ValidateIsPositiveNonZero));
            RotateDeltaProperty = DependencyProperty.Register("RotateDelta", typeof(double), typeof(ZoomBoxPanel), new FrameworkPropertyMetadata(15.0, null, new CoerceValueCallback(ZoomBoxPanel.CoerceRotateDelta)), new ValidateValueCallback(ZoomBoxPanel.ValidateIsPositiveNonZero));
            typeMetadata = new FrameworkPropertyMetadata
            {
                DefaultValue = eWheelMode.Zoom,
                PropertyChangedCallback = new PropertyChangedCallback(ZoomBoxPanel.PropertyChanged_AMode)
            };
            WheelModeProperty = DependencyProperty.Register("WheelMode", typeof(eWheelMode), typeof(ZoomBoxPanel), typeMetadata, null);
            typeMetadata = new FrameworkPropertyMetadata
            {
                DefaultValue = eZoomMode.FitPage,
                AffectsRender = true,
                Journal = true,
                PropertyChangedCallback = new PropertyChangedCallback(ZoomBoxPanel.PropertyChanged_AMode)
            };
            ZoomModeProperty = DependencyProperty.Register("ZoomMode", typeof(eZoomMode), typeof(ZoomBoxPanel), typeMetadata, null);
            typeMetadata = new FrameworkPropertyMetadata
            {
                DefaultValue = eMouseMode.Zoom,
                PropertyChangedCallback = new PropertyChangedCallback(ZoomBoxPanel.PropertyChanged_AMode)
            };
            MouseModeProperty = DependencyProperty.Register("MouseMode", typeof(eMouseMode), typeof(ZoomBoxPanel), typeMetadata, null);
            CommandManager.RegisterClassCommandBinding(typeof(ZoomBoxPanel), new CommandBinding(NavigationCommands.IncreaseZoom, new ExecutedRoutedEventHandler(ZoomBoxPanel.ExecutedEventHandler_IncreaseZoom), new CanExecuteRoutedEventHandler(ZoomBoxPanel.CanExecuteEventHandler_IfHasContent)));
            CommandManager.RegisterClassCommandBinding(typeof(ZoomBoxPanel), new CommandBinding(NavigationCommands.DecreaseZoom, new ExecutedRoutedEventHandler(ZoomBoxPanel.ExecutedEventHandler_DecreaseZoom), new CanExecuteRoutedEventHandler(ZoomBoxPanel.CanExecuteEventHandler_IfHasContent)));
            InputGestureCollection inputGestures = new InputGestureCollection();
            inputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            rotateClockwiseCommand = new RoutedUICommand("Rotate Clockwise", "RotateClockwise", typeof(ZoomBoxPanel), inputGestures);
            CommandManager.RegisterClassCommandBinding(typeof(ZoomBoxPanel), new CommandBinding(rotateClockwiseCommand, new ExecutedRoutedEventHandler(ZoomBoxPanel.ExecutedEventHandler_RotateClockwise), new CanExecuteRoutedEventHandler(ZoomBoxPanel.CanExecuteEventHandler_IfHasContent)));
            InputGestureCollection gestures2 = new InputGestureCollection();
            gestures2.Add(new KeyGesture(Key.R, ModifierKeys.Alt));
            rotateCounterclockwiseCommand = new RoutedUICommand("Rotate Counterclockwise", "RotateCounterclockwise", typeof(ZoomBoxPanel), gestures2);
            CommandManager.RegisterClassCommandBinding(typeof(ZoomBoxPanel), new CommandBinding(rotateCounterclockwiseCommand, new ExecutedRoutedEventHandler(ZoomBoxPanel.ExecutedEventHandler_RotateCounterclockwise), new CanExecuteRoutedEventHandler(ZoomBoxPanel.CanExecuteEventHandler_IfHasContent)));
            InputGestureCollection gestures3 = new InputGestureCollection();
            gestures3.Add(new KeyGesture(Key.Home, ModifierKeys.None));
            rotateHomeCommand = new RoutedUICommand("Rotate Home", "RotateHome", typeof(ZoomBoxPanel), gestures3);
            CommandManager.RegisterClassCommandBinding(typeof(ZoomBoxPanel), new CommandBinding(rotateHomeCommand, new ExecutedRoutedEventHandler(ZoomBoxPanel.ExecutedEventHandler_RotateHome), new CanExecuteRoutedEventHandler(ZoomBoxPanel.CanExecuteEventHandler_IfHasContent)));
            InputGestureCollection gestures4 = new InputGestureCollection();
            gestures4.Add(new KeyGesture(Key.End, ModifierKeys.None));
            rotateReverseCommand = new RoutedUICommand("Rotate Reverse", "RotateReverse", typeof(ZoomBoxPanel), gestures4);
            CommandManager.RegisterClassCommandBinding(typeof(ZoomBoxPanel), new CommandBinding(rotateReverseCommand, new ExecutedRoutedEventHandler(ZoomBoxPanel.ExecutedEventHandler_RotateReverse), new CanExecuteRoutedEventHandler(ZoomBoxPanel.CanExecuteEventHandler_IfHasContent)));
            ModeChangeEvent = EventManager.RegisterRoutedEvent("ModeChange", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ZoomBoxPanel));
        }

        public ZoomBoxPanel()
        {
            this.transformGroup.Children.Add(this.rotateTransform);
            this.transformGroup.Children.Add(this.zoomTransform);
            this.transformGroup.Children.Add(this.translateTransform);
            this.panX = 0.0;
            this.panY = 0.0;
            this.rotateAngle = 0.0;
            this.rotateCenterX = 0.0;
            this.rotateCenterY = 0.0;
            base.ClipToBounds = true;
        }

        private bool ApplyPanAbs(double deltaX, double deltaY)
        {
            double panX = deltaX;
            double panY = deltaY;
            switch (this.ZoomMode)
            {
                case eZoomMode.FitWidth:
                    panX = this.panX;
                    break;

                case eZoomMode.FitHeight:
                    panY = this.panY;
                    break;

                case eZoomMode.FitPage:
                    panX = this.panX;
                    panY = this.panY;
                    break;
            }
            if ((panX == this.panX) && (panY == this.panY))
            {
                return false;
            }
            this.panX = panX;
            this.panY = panY;
            return true;
        }

        private bool ApplyPanDelta(double deltaX, double deltaY)
        {
            double panX = this.panX + deltaX;
            double panY = this.panY + deltaY;
            switch (this.ZoomMode)
            {
                case eZoomMode.FitWidth:
                    panX = this.panX;
                    break;

                case eZoomMode.FitHeight:
                    panY = this.panY;
                    break;

                case eZoomMode.FitPage:
                    panX = this.panX;
                    panY = this.panY;
                    break;
            }
            if ((panX == this.panX) && (panY == this.panY))
            {
                return false;
            }
            this.panX = panX;
            this.panY = panY;
            return true;
        }

        public void MoveTo(UIElement element)
        {
            Point position = element.TranslatePoint(new Point(0, 0), this);
            Point centerPoint = new Point(this.ActualWidth / (2*  this.zoomTransform.ScaleX), this.ActualHeight / (2* this.zoomTransform.ScaleY));

            double x = centerPoint.X -position.X;
            double y = centerPoint.Y -position.Y;

            x *= this.zoomTransform.ScaleX;
            y *= this.zoomTransform.ScaleY;

            this.translateTransform.BeginAnimation(TranslateTransform.XProperty, CreatePanAnimation(x), HandoffBehavior.Compose);
            this.translateTransform.BeginAnimation(TranslateTransform.YProperty, CreatePanAnimation(y), HandoffBehavior.Compose);
        }

        private DoubleAnimation CreatePanAnimation(double toValue)
        {
            var da = new DoubleAnimation(toValue, new Duration(TimeSpan.FromMilliseconds(100)));
            da.AccelerationRatio = 0.1;
            da.DecelerationRatio = 0.9;
            da.FillBehavior = FillBehavior.HoldEnd;
            da.Freeze();
            return da;
        }

        protected void ApplyRotateCommand(double delta, int factor, Point panelPoint)
        {
            if (factor > 0)
            {
                this.rotateAngle = (this.rotateAngle + (delta * factor)) % 360.0;
                if (this.rotateAngle < 0.0)
                {
                    this.rotateAngle += 360.0;
                }
                this.ZoomMode = eZoomMode.CustomSize;
                this.rotateCenterX = this.childSize.Width / 2.0;
                this.rotateCenterY = this.childSize.Height / 2.0;
                this.ApplyZoom(true);
            }
        }

        protected void ApplyScrollCommand(double delta, int factor, Point panelPoint)
        {
            if (factor > 0)
            {
                double deltaY = delta * factor;
                if (this.ApplyPanDelta(0.0, deltaY))
                {
                    this.ApplyZoom(true);
                }
            }
        }

        protected void ApplyZoom(bool animate)
        {
            if (!animate || !this.animations)
            {
                this.translateTransform.BeginAnimation(TranslateTransform.XProperty, null);
                this.translateTransform.BeginAnimation(TranslateTransform.YProperty, null);
                this.zoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                this.zoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                this.rotateTransform.Angle = this.rotateAngle;
                this.rotateTransform.CenterX = this.rotateCenterX;
                this.rotateTransform.CenterY = this.rotateCenterY;
                this.translateTransform.X = this.panX;
                this.translateTransform.Y = this.panY;
                this.zoomTransform.ScaleX = this.ZoomFactor;
                this.zoomTransform.ScaleY = this.ZoomFactor;
            }
            else
            {
                DoubleAnimation animation = this.MakeZoomAnimation(this.panX);
                DoubleAnimation animation2 = this.MakeZoomAnimation(this.panY);
                DoubleAnimation animation3 = this.MakeZoomAnimation(this.ZoomFactor);
                DoubleAnimation animation4 = this.MakeZoomAnimation(this.ZoomFactor);
                this.translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                this.translateTransform.BeginAnimation(TranslateTransform.YProperty, animation2);
                this.zoomTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation3);
                this.zoomTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation4);
            }
        }

        protected void ApplyZoomCommand(Point panelPoint)
        {
            Point point = this.transformGroup.Inverse.Transform(panelPoint);
            this.ZoomMode = eZoomMode.CustomSize;
            this.panX = -1.0 * ((point.X * this.ZoomFactor) - panelPoint.X);
            this.panY = -1.0 * ((point.Y * this.ZoomFactor) - panelPoint.Y);
            this.ApplyZoom(true);
        }

        protected void ApplyZoomCommand(double delta, int factor, Point panelPoint)
        {
            if (factor > 0)
            {
                double zoomFactor = this.ZoomFactor;
                for (int i = 1; i <= factor; i++)
                {
                    zoomFactor *= delta;
                }
                this.ZoomFactor = zoomFactor;
                this.ApplyZoomCommand(panelPoint);
            }
        }

        protected override Size ArrangeOverride(Size panelRect)
        {
            foreach (UIElement element in base.InternalChildren)
            {
                element.Arrange(new Rect(0.0, 0.0, element.DesiredSize.Width, element.DesiredSize.Height));
            }
            this.RecalcPage(panelRect);
            return panelRect;
        }

        protected double CalcFloatingOffset(double zoom, double parent, double child, bool blockOnMargin, double margin)
        {
            double num = 0.0;
            double num2 = child * zoom;
            num = (parent - num2) / 2.0;
            if (blockOnMargin && (num < margin))
            {
                num = margin;
            }
            return num;
        }

        private void calcZoomFromTick()
        {
            double num = Math.Log10(this.MinZoom);
            double num2 = Math.Log10(this.MaxZoom);
            if (num2 <= num)
            {
                num2 = num + 0.01;
            }
            double num3 = (this.ZoomTick - this.MinZoomTick) / (this.MaxZoomTick - this.MinZoomTick);
            double y = (num3 * (num2 - num)) + num;
            this.Zoom = Math.Pow(10.0, y);
            Point panelPoint = new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);
            this.ApplyZoomCommand(panelPoint);
        }

        private void calcZoomTick()
        {
            double num = Math.Log10(this.MinZoom);
            double num2 = Math.Log10(this.MaxZoom);
            double num3 = Math.Log10(this.Zoom);
            if (num2 <= num)
            {
                num2 = num + 0.01;
            }
            double num4 = (num3 - num) / (num2 - num);
            this.ZoomTick = (num4 * (this.MaxZoomTick - this.MinZoomTick)) + this.MinZoomTick;
        }

        private static void CanExecuteEventHandler_IfHasContent(object sender, CanExecuteRoutedEventArgs e)
        {
            ZoomBoxPanel panel = sender as ZoomBoxPanel;
            e.CanExecute = (panel != null) && (panel.Children.Count > 0);
            e.Handled = true;
        }

        private static object CoerceMaxZoom(DependencyObject d, object value)
        {
            double num = (double)value;
            ZoomBoxPanel panel = d as ZoomBoxPanel;
            if (panel != null)
            {
                if (num < panel.MinZoom)
                {
                    panel.MinZoom = num;
                }
                if (panel.Zoom > num)
                {
                    panel.Zoom = num;
                }
            }
            return num;
        }

        private static object CoerceMaxZoomTick(DependencyObject d, object value)
        {
            double num = (double)value;
            ZoomBoxPanel panel = d as ZoomBoxPanel;
            if ((panel != null) && (num <= panel.MinZoomTick))
            {
                panel.MinZoomTick = num - 1.0;
            }
            return num;
        }

        private static object CoerceMinZoom(DependencyObject d, object value)
        {
            double num = (double)value;
            ZoomBoxPanel panel = d as ZoomBoxPanel;
            if (panel != null)
            {
                if (num <= 1E-07)
                {
                    num = 1E-07;
                }
                if (num > panel.MaxZoom)
                {
                    panel.MaxZoom = num;
                }
                if (panel.Zoom < num)
                {
                    panel.Zoom = num;
                }
            }
            return num;
        }

        private static object CoerceMinZoomTick(DependencyObject d, object value)
        {
            double num = (double)value;
            ZoomBoxPanel panel = d as ZoomBoxPanel;
            if ((panel != null) && (num >= panel.MaxZoomTick))
            {
                panel.MaxZoomTick = num + 1.0;
            }
            return num;
        }

        private static object CoerceRotateDelta(DependencyObject d, object value)
        {
            double num = (double)value;
            if (num <= 0.0)
            {
                return 1.0;
            }
            if (num > 359.0)
            {
                num = 359.0;
            }
            return num;
        }

        private static object CoerceZoom(DependencyObject d, object value)
        {
            double minZoom = (double)value;
            ZoomBoxPanel panel = d as ZoomBoxPanel;
            if (panel != null)
            {
                if (minZoom > panel.MaxZoom)
                {
                    return panel.MaxZoom;
                }
                if (minZoom < panel.MinZoom)
                {
                    minZoom = panel.MinZoom;
                }
            }
            return minZoom;
        }

        private static object CoerceZoomTick(DependencyObject d, object value)
        {
            double minZoomTick = (double)value;
            ZoomBoxPanel panel = d as ZoomBoxPanel;
            if (panel != null)
            {
                if (minZoomTick > panel.MaxZoomTick)
                {
                    return panel.MaxZoomTick;
                }
                if (minZoomTick < panel.MinZoomTick)
                {
                    minZoomTick = panel.MinZoomTick;
                }
            }
            return minZoomTick;
        }

        public static void ExecutedEventHandler_DecreaseZoom(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomBoxPanel panel = sender as ZoomBoxPanel;
            if (panel != null)
            {
                panel.process_ZoomCommand(false);
            }
        }

        public static void ExecutedEventHandler_IncreaseZoom(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomBoxPanel panel = sender as ZoomBoxPanel;
            if (panel != null)
            {
                panel.process_ZoomCommand(true);
            }
        }

        public static void ExecutedEventHandler_RotateClockwise(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomBoxPanel panel = sender as ZoomBoxPanel;
            double? angle = null;
            string parameter = e.Parameter as string;
            if ((parameter != null) && (parameter.Length > 0))
            {
                angle = new double?(double.Parse(parameter));
            }
            if (panel != null)
            {
                panel.process_RotateCommand(true, angle);
            }
        }

        public static void ExecutedEventHandler_RotateCounterclockwise(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomBoxPanel panel = sender as ZoomBoxPanel;
            double? angle = null;
            string parameter = e.Parameter as string;
            if ((parameter != null) && (parameter.Length > 0))
            {
                angle = new double?(double.Parse(parameter));
            }
            if (panel != null)
            {
                panel.process_RotateCommand(false, angle);
            }
        }

        public static void ExecutedEventHandler_RotateHome(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomBoxPanel panel = sender as ZoomBoxPanel;
            if (panel != null)
            {
                panel.process_RotateHomeReverse(true);
            }
        }

        public static void ExecutedEventHandler_RotateReverse(object sender, ExecutedRoutedEventArgs e)
        {
            ZoomBoxPanel panel = sender as ZoomBoxPanel;
            if (panel != null)
            {
                panel.process_RotateHomeReverse(false);
            }
        }

        public Point LogicalCentrePoint()
        {
            Point point = new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);
            return this.transformGroup.Inverse.Transform(point);
        }

        protected DoubleAnimation MakeZoomAnimation(double value)
        {
            return new DoubleAnimation(value, new Duration(TimeSpan.FromMilliseconds(300.0))) { FillBehavior = FillBehavior.HoldEnd };
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size(150.0, 150.0);
            if (!double.IsNaN(constraint.Width) && !double.IsInfinity(constraint.Width))
            {
                size.Width = constraint.Width;
            }
            if (!double.IsNaN(constraint.Height) && !double.IsInfinity(constraint.Height))
            {
                size.Height = constraint.Height;
            }
            this.childSize = new Size(1.0, 1.0);
            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (UIElement element in base.InternalChildren)
            {
                element.Measure(availableSize);
                if (element.DesiredSize.Width > this.childSize.Width)
                {
                    this.childSize.Width = element.DesiredSize.Width;
                }
                if (element.DesiredSize.Height > this.childSize.Height)
                {
                    this.childSize.Height = element.DesiredSize.Height;
                }
            }
            return size;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            base.Focusable = true;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            base.MouseWheel += new MouseWheelEventHandler(this.process_MouseWheel);
            base.MouseDown += new MouseButtonEventHandler(this.process_MouseDown);
            base.MouseUp += new MouseButtonEventHandler(this.process_MouseUp);
            base.MouseMove += new MouseEventHandler(this.process_MouseMove);
            base.PreviewMouseDown += new MouseButtonEventHandler(this.process_PreviewMouseDown);
            base.PreviewMouseUp += new MouseButtonEventHandler(this.process_PreviewMouseUp);
            base.PreviewMouseMove += new MouseEventHandler(this.process_PreviewMouseMove);
            this.ApplyZoom(false);
            foreach (UIElement element in base.InternalChildren)
            {
                element.RenderTransform = this.transformGroup;
            }
        }

        private void process_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.GetPosition(this);
            if ((this.MouseMode == eMouseMode.Pan) && (((e.ChangedButton == MouseButton.Left) && (((byte)(Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down)) == 0)) && (((byte)(Keyboard.GetKeyStates(Key.RightAlt) & KeyStates.Down)) == 0)))
            {
                this.prevCursor = base.Cursor;
                this.StartMouseCapture(e.GetPosition(this));
                base.Cursor = Cursors.ScrollAll;
            }
        }

        private void process_MouseMove(object sender, MouseEventArgs e)
        {
            //e.GetPosition(this);
            switch (this.MouseMode)
            {
                case eMouseMode.Zoom:
                    break;

                case eMouseMode.Pan:
                    if (base.IsMouseCaptured)
                    {
                        Point position = e.GetPosition(this);
                        double deltaX = position.X - this.startMouseCapturePanel.X;
                        double deltaY = position.Y - this.startMouseCapturePanel.Y;
                        if ((deltaX != 0.0) || (deltaY != 0.0))
                        {
                            this.startMouseCapturePanel.X = position.X;
                            this.startMouseCapturePanel.Y = position.Y;
                            if (this.ApplyPanDelta(deltaX, deltaY))
                            {
                                this.ApplyZoom(false);
                            }
                        }
                    }
                    break;

                default:
                    return;
            }
        }

        private void process_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point position;
            int num;
            double num2;
            e.GetPosition(this);
            switch (this.MouseMode)
            {
                case eMouseMode.Zoom:
                    position = e.GetPosition(this);
                    num = 0;
                    num2 = 1.0 + (this.ClickZoomDelta / 100.0);
                    switch (e.ChangedButton)
                    {
                        case MouseButton.Left:
                            num = 1;
                            break;

                        case MouseButton.Right:
                            num = 1;
                            num2 = 1.0 / num2;
                            break;
                    }
                    break;

                default:
                    goto lab1;
            }
            this.ApplyZoomCommand(num2, num, position);
        lab1:
            if (base.IsMouseCaptured)
            {
                base.ReleaseMouseCapture();
                base.Cursor = this.prevCursor;
                this.prevCursor = null;
            }
        }

        private void process_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point position = e.GetPosition(this);
            double delta = 0.0;
            int num2 = Math.Abs(e.Delta);
            if ((this.minMouseDelta > num2) && (num2 > 0))
            {
                this.minMouseDelta = num2;
            }
            int factor = num2 / this.minMouseDelta;
            if (factor < 1)
            {
                factor = 1;
            }
            switch (this.WheelMode)
            {
                case eWheelMode.Zoom:
                    delta = 1.0 + (this.WheelZoomDelta / 100.0);
                    if ((e.Delta <= 0) && (delta != 0.0))
                    {
                        delta = 1.0 / delta;
                    }
                    this.ApplyZoomCommand(delta, factor, position);
                    return;

                case eWheelMode.Scroll:
                    delta = this.scrollDelta;
                    if (e.Delta <= 0)
                    {
                        delta *= -1.0;
                    }
                    this.ApplyScrollCommand(delta, factor, position);
                    return;

                case eWheelMode.Rotate:
                    delta = this.RotateDelta;
                    if (e.Delta <= 0)
                    {
                        delta *= -1.0;
                    }
                    this.ApplyRotateCommand(delta, factor, position);
                    return;
            }
        }

        private void process_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.lockContent)
            {
                e.Handled = true;
            }
        }

        private void process_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.lockContent)
            {
                e.Handled = true;
            }
        }

        private void process_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.lockContent)
            {
                e.Handled = true;
            }
        }

        private void process_PropertyChanged_Zoom(DependencyPropertyChangedEventArgs e)
        {
            if (!this.zoomCircularReference)
            {
                this.zoomCircularReference = true;
                if (e.Property == ZoomProperty)
                {
                    this.calcZoomTick();
                }
                else if (e.Property == ZoomTickProperty)
                {
                    this.calcZoomFromTick();
                }
                this.zoomCircularReference = false;
            }
        }

        private void process_RotateCommand(bool clockWise, double? angle)
        {
            Point panelPoint = new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);
            double rotateDelta = this.RotateDelta;
            if (angle.HasValue)
            {
                rotateDelta = angle.Value;
            }
            if (!clockWise)
            {
                rotateDelta *= -1.0;
            }
            this.ApplyRotateCommand(rotateDelta, 1, panelPoint);
        }

        private void process_RotateHomeReverse(bool isHome)
        {
            new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);
            this.rotateAngle = isHome ? ((double)0) : ((double)180);
            this.rotateCenterX = this.childSize.Width / 2.0;
            this.rotateCenterY = this.childSize.Height / 2.0;
            this.ApplyZoom(true);
        }

        private void process_ZoomCommand(bool increase)
        {
            Point panelPoint = new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);
            double delta = 1.0 + (this.ClickZoomDelta / 100.0);
            if (!increase)
            {
                delta = 1.0 / delta;
            }
            this.ApplyZoomCommand(delta, 1, panelPoint);
        }

        private static void PropertyChanged_AMode(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomBoxPanel panel = d as ZoomBoxPanel;
            if (panel != null)
            {
                panel.RaiseModeChangeEvent();
            }
        }

        private static void PropertyChanged_Zoom(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ZoomBoxPanel panel = d as ZoomBoxPanel;
            if (panel != null)
            {
                panel.process_PropertyChanged_Zoom(e);
            }
        }

        private void RaiseModeChangeEvent()
        {
            RoutedEventArgs e = new RoutedEventArgs(ModeChangeEvent);
            base.RaiseEvent(e);
        }

        protected void RecalcPage(Size panelRect)
        {
            double margin = 4.0;
            double num2 = 4.0;
            double num3 = 3.0;
            double num4 = 3.0;
            double width = 0.0;
            double height = 0.0;
            double num7 = 0.0;
            double num8 = 0.0;
            double num9 = 5.0;
            switch (this.ZoomMode)
            {
                case eZoomMode.CustomSize:
                    break;

                case eZoomMode.FitWidth:
                    width = (panelRect.Width - margin) - num2;
                    if (width < num9)
                    {
                        width = num9;
                    }
                    num7 = width / this.childSize.Width;
                    this.ZoomFactor = num7;
                    this.panX = margin;
                    this.panY = this.CalcFloatingOffset(this.ZoomFactor, panelRect.Height, this.childSize.Height, true, num3);
                    this.ApplyZoom(false);
                    return;

                case eZoomMode.FitHeight:
                    height = (panelRect.Height - num3) - num4;
                    if (height < num9)
                    {
                        height = num9;
                    }
                    num8 = height / this.childSize.Height;
                    this.ZoomFactor = num8;
                    this.panX = this.CalcFloatingOffset(this.ZoomFactor, panelRect.Width, this.childSize.Width, true, margin);
                    this.panY = num3;
                    this.ApplyZoom(false);
                    return;

                case eZoomMode.FitPage:
                    width = (panelRect.Width - margin) - num2;
                    if (width < num9)
                    {
                        width = num9;
                    }
                    num7 = width / this.childSize.Width;
                    height = (panelRect.Height - num3) - num4;
                    if (height < num9)
                    {
                        height = num9;
                    }
                    num8 = height / this.childSize.Height;
                    if (num7 <= num8)
                    {
                        this.ZoomFactor = num7;
                        this.panX = margin;
                        this.panY = this.CalcFloatingOffset(this.ZoomFactor, panelRect.Height, this.childSize.Height, true, num3);
                    }
                    else
                    {
                        this.ZoomFactor = num8;
                        this.panX = this.CalcFloatingOffset(this.ZoomFactor, panelRect.Width, this.childSize.Width, true, margin);
                        this.panY = num3;
                    }
                    this.ApplyZoom(false);
                    return;

                case eZoomMode.FitVisible:
                    width = panelRect.Width;
                    if (width < num9)
                    {
                        width = num9;
                    }
                    num7 = width / this.childSize.Width;
                    height = panelRect.Height;
                    if (height < num9)
                    {
                        height = num9;
                    }
                    num8 = height / this.childSize.Height;
                    if (num7 >= num8)
                    {
                        this.ZoomFactor = num7;
                        this.panX = 0.0;
                        this.panY = this.CalcFloatingOffset(this.ZoomFactor, panelRect.Height, this.childSize.Height, true, 0.0);
                    }
                    else
                    {
                        this.ZoomFactor = num8;
                        this.panX = this.CalcFloatingOffset(this.ZoomFactor, panelRect.Width, this.childSize.Width, true, 0.0);
                        this.panY = 0.0;
                    }
                    this.ApplyZoom(false);
                    break;

                default:
                    return;
            }
        }

        private void setMouseCursorFromMode()
        {
            switch (this.MouseMode)
            {
                case eMouseMode.Zoom:
                    base.Cursor = Cursors.ScrollAll;
                    return;

                case eMouseMode.Pan:
                    base.Cursor = Cursors.Hand;
                    return;
            }
            base.Cursor = Cursors.Arrow;
        }

        public void SetPan(double x, double y)
        {
            if (this.ApplyPanAbs(x, y))
            {
                this.ApplyZoom(false);
            }
        }

        public void SetZoomForVirtualWidth(int value)
        {
            this.ZoomMode = eZoomMode.CustomSize;
            if (this.childSize.Width > 0.0)
            {
                this.ZoomFactor = ((double)value) / this.childSize.Width;
            }
            this.panX = 0.0;
            this.panY = 0.0;
            this.rotateAngle = 0.0;
            this.rotateCenterX = 0.0;
            this.rotateCenterY = 0.0;
            this.ApplyZoom(false);
        }

        private void StartMouseCapture(Point startPoint)
        {
            this.startMouseCapturePanel = startPoint;
            base.CaptureMouse();
        }

        private static bool ValidateIsNonZero(object value)
        {
            double num = (double)value;
            if (num == 0.0)
            {
                return false;
            }
            return true;
        }

        private static bool ValidateIsPositiveNonZero(object value)
        {
            double num = (double)value;
            if (num <= 0.0)
            {
                return false;
            }
            return true;
        }

        // Properties
        public bool Animations
        {
            get
            {
                return this.animations;
            }
            set
            {
                this.animations = value;
            }
        }

        public double ClickZoomDelta
        {
            get
            {
                return (double)base.GetValue(ClickZoomDeltaProperty);
            }
            set
            {
                base.SetValue(ClickZoomDeltaProperty, value);
            }
        }

        public bool LockContent
        {
            get
            {
                return this.lockContent;
            }
            set
            {
                this.lockContent = value;
            }
        }

        public double MaxZoom
        {
            get
            {
                return (double)base.GetValue(MaxZoomProperty);
            }
            set
            {
                base.SetValue(MaxZoomProperty, value);
            }
        }

        public double MaxZoomTick
        {
            get
            {
                return (double)base.GetValue(MaxZoomTickProperty);
            }
            set
            {
                base.SetValue(MaxZoomTickProperty, value);
            }
        }

        public double MinZoom
        {
            get
            {
                return (double)base.GetValue(MinZoomProperty);
            }
            set
            {
                base.SetValue(MinZoomProperty, value);
            }
        }

        public double MinZoomTick
        {
            get
            {
                return (double)base.GetValue(MinZoomTickProperty);
            }
            set
            {
                base.SetValue(MinZoomTickProperty, value);
            }
        }

        public eMouseMode MouseMode
        {
            get
            {
                return (eMouseMode)base.GetValue(MouseModeProperty);
            }
            set
            {
                base.SetValue(MouseModeProperty, value);
            }
        }

        public static RoutedUICommand RotateClockwise
        {
            get
            {
                return rotateClockwiseCommand;
            }
        }

        public static RoutedUICommand RotateCounterclockwise
        {
            get
            {
                return rotateCounterclockwiseCommand;
            }
        }

        public double RotateDelta
        {
            get
            {
                return (double)base.GetValue(RotateDeltaProperty);
            }
            set
            {
                base.SetValue(RotateDeltaProperty, value);
            }
        }

        public static RoutedUICommand RotateHome
        {
            get
            {
                return rotateHomeCommand;
            }
        }

        public static RoutedUICommand RotateReverse
        {
            get
            {
                return rotateReverseCommand;
            }
        }

        public eWheelMode WheelMode
        {
            get
            {
                return (eWheelMode)base.GetValue(WheelModeProperty);
            }
            set
            {
                base.SetValue(WheelModeProperty, value);
            }
        }

        public double WheelZoomDelta
        {
            get
            {
                return (double)base.GetValue(WheelZoomDeltaProperty);
            }
            set
            {
                base.SetValue(WheelZoomDeltaProperty, value);
            }
        }

        public double Zoom
        {
            get
            {
                return (double)base.GetValue(ZoomProperty);
            }
            set
            {
                base.SetValue(ZoomProperty, value);
            }
        }

        protected double ZoomFactor
        {
            get
            {
                return (this.Zoom / 100.0);
            }
            set
            {
                this.Zoom = value * 100.0;
            }
        }

        public eZoomMode ZoomMode
        {
            get
            {
                return (eZoomMode)base.GetValue(ZoomModeProperty);
            }
            set
            {
                base.SetValue(ZoomModeProperty, value);
            }
        }

        public double ZoomTick
        {
            get
            {
                return (double)base.GetValue(ZoomTickProperty);
            }
            set
            {
                base.SetValue(ZoomTickProperty, value);
            }
        }

        // Nested Types
        public enum eMouseMode
        {
            None,
            Pointer,
            Zoom,
            Pan
        }

        public enum eWheelMode
        {
            None,
            Zoom,
            Scroll,
            Rotate
        }

        public enum eZoomMode
        {
            CustomSize,
            FitWidth,
            FitHeight,
            FitPage,
            FitVisible
        }
    }


}
