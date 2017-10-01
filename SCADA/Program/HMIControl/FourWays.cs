using System;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class FourWays : HMIControlBase
    {
        static FourWays()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FourWays), new FrameworkPropertyMetadata(typeof(FourWays)));
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[4]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.5,1),ConnectOrientation.Bottom),
                    new  LinkPosition(new Point(0.05,0.85),ConnectOrientation.Left),
                    new  LinkPosition(new Point(0.95,0.85),ConnectOrientation.Right)
                };
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.LEFT, TagActions.RIGHT, TagActions.MID }; 
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
                case TagActions.MID:
                    var _funcMid = tagChanged as Func<bool>;
                    if (_funcMid != null)
                    {
                        return delegate
                        {
                            if (_funcMid())
                                VisualStateManager.GoToState(this, "Middle", true);
                        };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
