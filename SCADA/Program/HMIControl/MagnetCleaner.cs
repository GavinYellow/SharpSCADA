using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    //[Startable]
    public  class MagnetCleaner : HMIControlBase
    {
        static MagnetCleaner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MagnetCleaner), new FrameworkPropertyMetadata(typeof(MagnetCleaner)));
        }
        public static readonly DependencyProperty RunningProperty =
            DependencyProperty.Register("Running", typeof(bool), typeof(MagnetCleaner),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnValueChanged)));
        #region HMI属性
        [Category("HMI")]
        public bool Running
        {
            get { return (bool)GetValue(RunningProperty); }
            set { SetValue(RunningProperty, value); }
        }
        #endregion
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Motor motor = Template.FindName("motor1", this) as Motor;
            children = new ITagLink[] { motor };
        }
        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0.65,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.15,1),ConnectOrientation.Bottom),
                };
        }
        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.ALARM,  TagActions.RUN };
        }
        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
               
            }
            return base.SetTagReader(key, tagChanged);
        }
        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Alarm ? "AlarmOn" : Running ? "ON" : "OFF", true);
        }
    }
}
