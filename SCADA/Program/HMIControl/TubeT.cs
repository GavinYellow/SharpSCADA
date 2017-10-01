using System.ComponentModel;
using System.Windows;

namespace HMIControl
{

    public class TubeT : HMIControlBase
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
              typeof(ConnectOrientation), typeof(TubeT),
              new PropertyMetadata(new PropertyChangedCallback(ValueChangedCallback)));

        static TubeT()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TubeT), new FrameworkPropertyMetadata(typeof(TubeT)));
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
            var tube = obj as TubeT;
            switch ((ConnectOrientation)args.NewValue)
            {
                case ConnectOrientation.Top:
                    VisualStateManager.GoToState(tube, "ToTop", true);
                    return;
                case  ConnectOrientation.Bottom:
                    VisualStateManager.GoToState(tube, "ToBottom", true);
                    return;
                case ConnectOrientation.Left:
                    VisualStateManager.GoToState(tube, "ToLeft", true);
                    return;
                case ConnectOrientation.Right:
                    VisualStateManager.GoToState(tube, "ToRight", true);
                    return;
            }
        }

    }
}
