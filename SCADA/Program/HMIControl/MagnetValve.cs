using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class MagnetValve : HMIControlBase
    {
        public static DependencyProperty OpenProperty = DependencyProperty.Register("Open", typeof(bool), typeof(MagnetValve),
            new PropertyMetadata(new PropertyChangedCallback(OpenChangedCallback)));

        static MagnetValve()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MagnetValve), new FrameworkPropertyMetadata(typeof(MagnetValve)));
        }

        private static void OpenChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as MagnetValve, (bool)args.NewValue ? "ON" : "OFF", true);
        }

        #region HMI属性
        [Category("HMI")]
        public bool Open
        {
            set
            {
                SetValue(OpenProperty, value);
            }
            get
            {
                return (bool)GetValue(OpenProperty);
            }
        }
        #endregion

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.ON };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.ON:
                    var _funcOn = tagChanged as Func<bool>;
                    if (_funcOn != null)
                    {
                        return delegate { Open = _funcOn(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
