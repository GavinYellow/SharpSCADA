using System.Windows;
using System.ComponentModel;
using System;

namespace HMIControl
{
    [Startable]
    public class Distributor : HMIControlBase
    {
        static Distributor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Distributor), new FrameworkPropertyMetadata(typeof(Distributor)));
        }

        //public static readonly DependencyProperty IsMotorWorkingProperty =
        //    DependencyProperty.Register("IsMotorWorking", typeof(bool), typeof(Distributor));

        //[Category("HMI")]
        //public bool IsMotorWorking
        //{
        //    get { return (bool)GetValue(IsMotorWorkingProperty); }
        //    set { SetValue(IsMotorWorkingProperty, value); }
        //}

        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Alarm ? "AlarmOn" : "AlarmOff", true);
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
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.ALARM };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Motor motor = Template.FindName("motor", this) as Motor;
            if (motor != null)
                children = new ITagLink[] { motor };
        }

        //public override Action SetTagReader(string key, Delegate tagChanged)
        //{
        //    switch (key)
        //    {
                
        //        case TagActions.RUN:
        //            var _funcRun = tagChanged as Func<bool>;
        //            if (_funcRun != null)
        //            {
        //                return delegate { IsMotorWorking = _funcRun(); };
        //            }
        //            else return null;
        //    }
        //    return base.SetTagReader(key, tagChanged);
        //}
    }
}
