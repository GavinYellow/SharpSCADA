using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HMIControl
{
    public class ZoomSlideControl : Control
    {
        // Fields
        private static DependencyProperty ContOpacityProperty;
        private DispatcherTimer dispatcherTimer;
        private bool isVisualActive = true;
        private static DependencyProperty MaxZoomTickProperty;
        private static DependencyProperty MinZoomTickProperty;
        private bool mouseIsOver;
        private static DependencyProperty TargetElementProperty;
        private const int TICKS_BEFORE_FADE = 6;
        private int ticksSinceMouseOut;
        private ZoomBoxPanel zoomBox;
        private static DependencyProperty ZoomProperty;
        private static DependencyProperty ZoomTickProperty;

        // Methods
        static ZoomSlideControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomSlideControl), new FrameworkPropertyMetadata(typeof(ZoomSlideControl)));
            ContOpacityProperty = DependencyProperty.Register("ContOpacity", typeof(double), typeof(ZoomSlideControl), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, null, null), null);
            ZoomTickProperty = DependencyProperty.Register("ZoomTick", typeof(double), typeof(ZoomSlideControl), new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, null, null), null);
            MinZoomTickProperty = DependencyProperty.Register("MinZoomTick", typeof(double), typeof(ZoomSlideControl), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, null, null), null);
            MaxZoomTickProperty = DependencyProperty.Register("MaxZoomTick", typeof(double), typeof(ZoomSlideControl), new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, null, null), null);
            ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(ZoomSlideControl), new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.AffectsRender, null, null), null);
            TargetElementProperty = DependencyProperty.Register("TargetElement", typeof(UIElement), typeof(ZoomSlideControl), new FrameworkPropertyMetadata(null), null);
        }

        public ZoomSlideControl()
        {
            this.SetUpCommands();
        }

        private void DecreaseZoomCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ZoomTick > this.MinZoomTick;
        }

        private void DecreaseZoomCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.zoomBox != null)
            {
                NavigationCommands.DecreaseZoom.Execute(null, this.zoomBox);
            }
            this.FocusOnTarget();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.ticksSinceMouseOut++;
            if ((!this.mouseIsOver && this.isVisualActive) && (this.ticksSinceMouseOut >= 6))
            {
                this.setControlVisualActive(false, true);
            }
        }

        private void FocusOnTarget()
        {
            if (this.TargetElement != null)
            {
                this.TargetElement.Focus();
            }
        }

        private void IncreaseZoomCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ZoomTick < this.MaxZoomTick;
        }

        private void IncreaseZoomCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.zoomBox != null)
            {
                NavigationCommands.IncreaseZoom.Execute(null, this.zoomBox);
            }
            this.FocusOnTarget();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.setControlVisualActive(false, false);
            this.SetUpTimer();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            this.mouseIsOver = true;
            this.setControlVisualActive(true, true);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.mouseIsOver = false;
            this.ticksSinceMouseOut = 0;
            base.OnMouseLeave(e);
        }

        private void setControlVisualActive(bool isOn, bool annimate)
        {
            if (this.isVisualActive != isOn)
            {
                this.isVisualActive = isOn;
                if (!annimate)
                {
                    this.ContOpacity = this.isVisualActive ? 1.0 : 0.25;
                }
                else
                {
                    double num = this.isVisualActive ? ((double)450) : ((double)0x7d0);
                    double toValue = this.isVisualActive ? 1.0 : 0.25;
                    DoubleAnimation animation = new DoubleAnimation(toValue, TimeSpan.FromMilliseconds(num));
                    base.BeginAnimation(ContOpacityProperty, animation, HandoffBehavior.SnapshotAndReplace);
                }
            }
        }

        private void SetUpCommands()
        {
            CommandBinding commandBinding = new CommandBinding(NavigationCommands.Zoom, new ExecutedRoutedEventHandler(this.ZoomCommand_Executed), new CanExecuteRoutedEventHandler(this.ZoomCommand_CanExecute));
            base.CommandBindings.Add(commandBinding);
            commandBinding = new CommandBinding(NavigationCommands.IncreaseZoom, new ExecutedRoutedEventHandler(this.IncreaseZoomCommand_Executed), new CanExecuteRoutedEventHandler(this.IncreaseZoomCommand_CanExecute));
            base.CommandBindings.Add(commandBinding);
            commandBinding = new CommandBinding(NavigationCommands.DecreaseZoom, new ExecutedRoutedEventHandler(this.DecreaseZoomCommand_Executed), new CanExecuteRoutedEventHandler(this.DecreaseZoomCommand_CanExecute));
            base.CommandBindings.Add(commandBinding);
        }

        private void SetUpTimer()
        {
            this.dispatcherTimer = new DispatcherTimer();
            this.dispatcherTimer.Tick += new EventHandler(this.dispatcherTimer_Tick);
            this.dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            this.dispatcherTimer.Start();
        }

        private void ZoomCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ZoomCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.zoomBox != null)
            {
                this.zoomBox.ZoomMode = ZoomBoxPanel.eZoomMode.FitPage;
            }
            this.FocusOnTarget();
        }

        // Properties
        public double ContOpacity
        {
            get
            {
                return (double)base.GetValue(ContOpacityProperty);
            }
            set
            {
                base.SetValue(ContOpacityProperty, value);
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

        public UIElement TargetElement
        {
            get
            {
                return (UIElement)base.GetValue(TargetElementProperty);
            }
            set
            {
                base.SetValue(TargetElementProperty, value);
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

        public ZoomBoxPanel ZoomBox
        {
            set
            {
                this.zoomBox = value;
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
    }

    public class ZoomSlideDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? nullable = value as double?;
            if (nullable.HasValue)
            {
                return string.Format("{0:0.}%", nullable);
            }
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("unexpected Convertback");
        }
    }

}
