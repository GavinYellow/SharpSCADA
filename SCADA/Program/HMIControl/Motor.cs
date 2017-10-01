using System.Windows;
using System.ComponentModel;
using System;

namespace HMIControl
{
    [Startable]
    public class Motor : HMIControlBase
    {
        static Motor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Motor), new FrameworkPropertyMetadata(typeof(Motor)));
        }

        public static readonly DependencyProperty RunningProperty = 
            DependencyProperty.Register("Running", typeof(bool), typeof(Motor),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnValueChanged)));

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.ALARM, TagActions.RUN };
        }

        [Category("HMI")]
        public bool Running
        {
            get { return (bool)GetValue(RunningProperty); }
            set { SetValue(RunningProperty, value); }
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0,0.5),ConnectOrientation.Left),
                       new  LinkPosition(new Point(1,0.5),ConnectOrientation.Right),
                };
        }

        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Alarm ? "AlarmOn1" : Running ? "Inwork" : "Outwork", true);
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.RUN:
                    var _funcRun = tagChanged as Func<bool>;
                    if (_funcRun != null)
                    {
                        return delegate { Running = _funcRun(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}