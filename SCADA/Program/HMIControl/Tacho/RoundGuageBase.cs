using System.Windows;
using System.Windows.Media;
using System;
using System.Windows.Input;

namespace HMIControl
{
    public class RoundGuageBase : GaugeBase
    {

        public static readonly DependencyProperty EndAngleProperty = DependencyProperty.Register("EndAngle", typeof(double), typeof(RoundGuageBase),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register("StartAngle", typeof(double), typeof(RoundGuageBase),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SweepDirectionProperty = DependencyProperty.Register("SweepDirection", typeof(SweepDirection), typeof(RoundGuageBase),
            new FrameworkPropertyMetadata(SweepDirection.Clockwise, FrameworkPropertyMetadataOptions.AffectsRender));

        protected RoundDial scale;
        protected RoundIndicator indict;
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.scale = base.GetTemplateChild("PART_SelectionRange") as RoundDial;
            this.indict = base.GetTemplateChild("PART_Track") as RoundIndicator;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double size = base.FontSize + 4;
            string valur = base.Value.ToString("F3");
            FormattedText formattedText = new FormattedText(valur, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(base.FontFamily, base.FontStyle, base.FontWeight, base.FontStretch), size, base.Foreground);
            drawingContext.DrawText(formattedText, new Point((base.ActualWidth - valur.Length * size * 0.5) / 2.0, 0.7 * base.ActualHeight));
        }


        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            this.InvalidateVisual();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                base.Focus();
                e.Handled = true;
                base.CaptureMouse();
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (base.IsMouseCaptured)
            {
                base.ReleaseMouseCapture();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (base.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                Point p=Mouse.GetPosition(this);
                double num = scale.AngleFromPoint(p);
                //this.indict.ValueFromAngle(lastAngle, num);
                this.Value = scale.ValueFromAngle(num);
                this.indict.RenderTransform = new RotateTransform(num);
                e.Handled = true;
            }
        }

        public RoundIndicator Indicator
        {
            get
            {
                return indict;
            }
        }

        public RoundDial Scale
        {
            get
            {
                return scale;
            }
        }

        public double EndAngle
        {
            get
            {
                return (double)base.GetValue(EndAngleProperty);
            }
            set
            {
                base.SetValue(EndAngleProperty, value);
            }
        }

        public double StartAngle
        {
            get
            {
                return (double)base.GetValue(StartAngleProperty);
            }
            set
            {
                base.SetValue(StartAngleProperty, value);
            }
        }

        public SweepDirection SweepDirection
        {
            get
            {
                return (SweepDirection)base.GetValue(SweepDirectionProperty);
            }
            set
            {
                base.SetValue(SweepDirectionProperty, value);
            }
        }
    }
}
