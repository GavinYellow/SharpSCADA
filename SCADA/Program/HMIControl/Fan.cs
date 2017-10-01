using System;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class Fan : HMIControlBase
    {
        static Fan()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Fan), new FrameworkPropertyMetadata(typeof(Fan)));
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.RUN, TagActions.ALARM, TagActions.DEVICENAME };
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[1]
                {  
                    new  LinkPosition(new Point(0.75,0),ConnectOrientation.Top),
                };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.RUN:
                    var _funcInRun = tagChanged as Func<bool>;
                    if (_funcInRun != null)
                    {
                        return delegate { VisualStateManager.GoToState(this, _funcInRun() ? "Running" : "NotRunning", true); };
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
    }
}
