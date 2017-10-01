using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class ManualAddControl : HMIControlBase
    {
        static ManualAddControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ManualAddControl), new FrameworkPropertyMetadata(typeof(ManualAddControl)));
        }

        public static DependencyProperty IsNeedAddProperty = DependencyProperty.Register("IsNeedAdd", typeof(bool), typeof(ManualAddControl),
           new PropertyMetadata(OnIsNeedAddChanged));

        public static readonly DependencyProperty StatusProperty =
           DependencyProperty.Register("Status", typeof(Int16), typeof(ManualAddControl));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var alarm = Template.FindName("alarm", this) as AlarmControl;
            var fan = Template.FindName("fan", this) as Fan;
            var gate = Template.FindName("gate1", this) as HMILable;
            children = new ITagLink[] { alarm, fan, gate };
        }

        #region HMI属性

        [Category("HMI")]
        public bool IsNeedAdd
        {
            set
            {
                SetValue(IsNeedAddProperty, value);
            }
            get
            {
                return (bool)GetValue(IsNeedAddProperty);
            }
        }

        [Category("HMI")]
        public Int16 Status
        {
            set
            {
                SetValue(StatusProperty, value);
            }
            get
            {
                return (Int16)GetValue(StatusProperty);
            }
        }

        #endregion

        private static void OnIsInWorkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as ManualAddControl, (bool)args.NewValue ? "Running" : "Base", true);
        }

        private static void OnIsNeedAddChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as ManualAddControl, (bool)args.NewValue ? "Alarm" : "Base", true);
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[1]
                {  
                    new  LinkPosition(new Point(0.5,1),ConnectOrientation.Bottom),
                };
        }

        public override string[] GetActions()
        {
            return new string[] { "状态", TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.RUN, TagActions.RAWNAME, TagActions.WARN };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case "状态":
                    var _funcStatus = tagChanged as Func<Int32>;
                    if (_funcStatus != null)
                    {
                        return delegate { Status = (Int16)_funcStatus(); };
                    }
                    else return null;
                case TagActions.WARN:
                    var _funcwarn = tagChanged as Func<bool>;
                    if (_funcwarn != null)
                    {
                        return delegate { IsNeedAdd = _funcwarn(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}