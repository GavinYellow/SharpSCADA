using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace HMIControl
{

    public class TubeLine : HMIControlBase
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
                typeof(Orientation), typeof(TubeLine),
                new PropertyMetadata(new PropertyChangedCallback(ValueChangedCallback)));

        static TubeLine()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TubeLine), new FrameworkPropertyMetadata(typeof(TubeLine)));
        }


        [Category("HMI")]
        public Orientation Orientation
        {
            set
            {
                SetValue(OrientationProperty, value);
            }
            get
            {
                return (Orientation)GetValue(OrientationProperty);
            }
        }

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as TubeLine, (bool)args.NewValue ? "Horizontal" : "Vertical", true);
        }
    }
}
