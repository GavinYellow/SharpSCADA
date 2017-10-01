using System;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class Divert : HMIControlBase
    {
        static Divert()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Divert), new FrameworkPropertyMetadata(typeof(Divert)));
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.LEFT, TagActions.RIGHT, TagActions.ALARM }; 
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[3]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.1,0.8),ConnectOrientation.Left),
                    new  LinkPosition(new Point(0.9,0.8),ConnectOrientation.Right)
                };
        }
		
        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Alarm ? "AlarmOn" : "AlarmOff", true);
        }
		
        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.LEFT:
                    var _funcLeftOn = tagChanged as Func<bool>;
                    if (_funcLeftOn != null)
                    {
                        return delegate
                        {
                            if (_funcLeftOn())
                                VisualStateManager.GoToState(this, "Left", true);
                        };
                    }
                    else return null;
                case TagActions.RIGHT:
                    var _funcRightOn = tagChanged as Func<bool>;
                    if (_funcRightOn != null)
                    {
                        return delegate
                        {
                            if (_funcRightOn())
                                VisualStateManager.GoToState(this, "Right", true);
                        };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
