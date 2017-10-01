using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class ControlValve : HMIControlBase
    {
        static ControlValve()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ControlValve), new FrameworkPropertyMetadata(typeof(ControlValve)));
        }
        public static DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen",
          typeof(bool), typeof(ControlValve),
          new PropertyMetadata(new PropertyChangedCallback(ValueChangedCallback)));

        #region HMI属性
        [Category("HMI")]
        public bool IsOpen
        {
            set
            {
                SetValue(IsOpenProperty, value);
            }
            get
            {
                return (bool)GetValue(IsOpenProperty);
            }
        }

        #endregion

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as ControlValve, (bool)args.NewValue ? "Open" : "Close", true);
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0,0.82),ConnectOrientation.Left),
                    new  LinkPosition(new Point(1,0.82),ConnectOrientation.Right)
                };
        }
    }
}
