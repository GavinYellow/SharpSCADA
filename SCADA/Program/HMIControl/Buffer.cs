using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    public class Buffer : HMIControlBase
    {
        public static readonly DependencyProperty StatusProperty =
           DependencyProperty.Register("Status", typeof(short), typeof(Buffer));

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
        
        static Buffer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Buffer), new FrameworkPropertyMetadata(typeof(Buffer)));
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.5,1),ConnectOrientation.Bottom),
                };
        }

        public override string[] GetActions()
        {
            return new string[] { "状态", TagActions.VISIBLE, TagActions.CAPTION, "Filled", TagActions.ALARM, TagActions.DEVICENAME };

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
                case "Filled": //有料信号
                    var _funcFilled = tagChanged as Func<bool>;
                    if (_funcFilled != null)
                    {
                        return delegate { VisualStateManager.GoToState(this, _funcFilled() ? "Filled" : "Empty", true); };
                    }
                    else return null;
                case TagActions.ALARM:
                    var _funcAlarm = tagChanged as Func<bool>;
                    if (_funcAlarm != null)
                    {
                        return delegate { VisualStateManager.GoToState(this, _funcAlarm() ? "Alarm" : "Unalarm", true); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var time = base.GetTemplateChild("discharge") as HMIText;
            if(time != null)
                children = new ITagLink[] { time };
        }

    }
}
