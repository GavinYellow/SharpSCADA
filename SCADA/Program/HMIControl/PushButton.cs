using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace HMIControl
{

    public class PushButton : ButtonBase
    {
        public static DependencyProperty ControlColorProperty = DependencyProperty.Register("ControlColor", typeof(Color), typeof(PushButton),
          new FrameworkPropertyMetadata(Colors.Red));

        static PushButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PushButton), new FrameworkPropertyMetadata(typeof(PushButton)));
        }

        [Category("Brushes")]
        public Color ControlColor
        {
            get
            {
                return (Color)base.GetValue(ControlColorProperty);
            }
            set
            {
                base.SetValue(ControlColorProperty, value);
            }
        }

        protected override void OnCheckedChanged(bool? oldstat, bool? newstat)
        {
            VisualStateManager.GoToState(this, newstat == true ? "Press" : "Unpress", true);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (IsPulse)
            {
                if (_funcWrites != null)
                    foreach (var _funcWrite in _funcWrites)
                        _funcWrite(true);
            }
            VisualStateManager.GoToState(this, "Press", true);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (IsPulse)
            {
                if (_funcWrites != null) 
                    foreach (var _funcWrite in _funcWrites)
                        _funcWrite(false);
            }
            if (IsChecked != true) VisualStateManager.GoToState(this, "Unpress", true);
        }

        protected override void OnClick()
        {
            base.OnClick();
            if (!IsPulse)
            {
                if (_funcints != null && _funcints.Count>0)
                    foreach (var _funcint in _funcints)
                        _funcint();
                else if (_funcWrites != null)
                {
                    foreach (var _funcWrite in _funcWrites)
                        _funcWrite(!IsChecked);
                }
            }
        }
    }
}
