using System.Windows;
using System.ComponentModel;
using System;

namespace HMIControl
{
    [Startable]
    public class Cylinder : HMIControlBase
    {
        static Cylinder()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Cylinder), new FrameworkPropertyMetadata(typeof(Cylinder)));
        }

        public static DependencyProperty IsInWorkProperty = DependencyProperty.Register("IsInWork", typeof(bool), typeof(Cylinder),
           new PropertyMetadata(new PropertyChangedCallback(ValueChangedCallback)));

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.ALARM, TagActions.ON, TagActions.OFF };
        }

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
            VisualStateManager.GoToState(obj as Cylinder, (bool)args.NewValue ? "ON" : "OFF", true);
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.ON:
                    var _funcOn = tagChanged as Func<bool>;
                    if (_funcOn != null)
                    {
                        return delegate { if (_funcOn()) IsInWork = true; };
                    }
                    else return null;
                case TagActions.OFF:
                    var _funcOff = tagChanged as Func<bool>;
                    if (_funcOff != null)
                    {
                        return delegate { if (_funcOff()) IsInWork = false; };
                    }
                    else return null;
                case TagActions.ALARM:
                    var _funcAlarm = tagChanged as Func<bool>;
                    if (_funcAlarm != null)
                    {
                        return delegate { Alarm = _funcAlarm(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }

    [Startable]
    public class CylinderTopView : Cylinder
    {
        static CylinderTopView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CylinderTopView), new FrameworkPropertyMetadata(typeof(CylinderTopView)));
        }
    }
}
