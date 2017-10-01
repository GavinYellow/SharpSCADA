using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{

    public class CylinderStick : HMIControlBase
    {
        static CylinderStick()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CylinderStick), new FrameworkPropertyMetadata(typeof(CylinderStick)));
        }

        public static readonly DependencyProperty IsInWorkProperty = DependencyProperty.Register("IsInWork",
           typeof(bool),
           typeof(CylinderStick),
           new PropertyMetadata(new PropertyChangedCallback(ValueChangedCallback)));

        #region HMI属性
        [Category("HMI")]
        public bool IsInWork
        {
            set
            {
                SetValue(IsInWorkProperty, value);
            }
            get
            {
                return (bool)GetValue(IsInWorkProperty);
            }
        }

        #endregion

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as CylinderStick, (bool)args.NewValue ? "PowerOn" : "PowerOff", true);
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.RUN }; 
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.RUN:
                    var _funcInRun = tagChanged as Func<bool>;
                    if (_funcInRun != null)
                    {
                        return delegate { IsInWork = _funcInRun(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
