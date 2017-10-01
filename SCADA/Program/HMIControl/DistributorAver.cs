using System;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class DistributorAver : HMIControlBase
    {
        static DistributorAver()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DistributorAver), new FrameworkPropertyMetadata(typeof(DistributorAver)));
        }

        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Alarm ? "AlarmOn" : "AlarmOff", true);
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[3]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.14,0.69),ConnectOrientation.Left),
                    new  LinkPosition(new Point(0.86,0.69),ConnectOrientation.Right),
                };
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.ALARM };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Motor motor = Template.FindName("motor", this) as Motor;
            if (motor != null)
                children = new ITagLink[] { motor };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {

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
}
