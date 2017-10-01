using System.ComponentModel;
using System.Windows;

namespace HMIControl
{

    public class TubeArc : HMIControlBase
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
                 typeof(ConnectOrientation), typeof(TubeArc),
                 new PropertyMetadata(new PropertyChangedCallback(ValueChangedCallback)));

        static TubeArc()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TubeArc), new FrameworkPropertyMetadata(typeof(TubeArc)));
        }

        #region HMI属性
        [Category("HMI")]
        public ConnectOrientation Orientation
        {
            set
            {
                SetValue(OrientationProperty, value);
            }
            get
            {
                return (ConnectOrientation)GetValue(OrientationProperty);
            }
        }

        #endregion

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var tube = obj as TubeArc;
            switch ((ConnectOrientation)args.NewValue)
            {
                case ConnectOrientation.Top:
                    VisualStateManager.GoToState(tube, "_1stQuarter", true);
                    return;
                case ConnectOrientation.Bottom:
                    VisualStateManager.GoToState(tube, "_2ndQuarter", true);
                    return;
                case ConnectOrientation.Left:
                    VisualStateManager.GoToState(tube, "_3rdQuarter", true);
                    return;
                case ConnectOrientation.Right:
                    VisualStateManager.GoToState(tube, "_4thQuarter", true);
                    return;
            }
        }
    }
}
