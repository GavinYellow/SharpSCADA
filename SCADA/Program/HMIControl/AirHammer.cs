using System.Windows;
using System;
using System.ComponentModel;


namespace HMIControl
{
    [Startable]
    public class AirHammer : HMIControlBase
    {
        public static DependencyProperty RunningProperty =
            DependencyProperty.Register("Running", typeof(bool), typeof(AirHammer),
              new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        #region HMI属性
        [Category("HMI")]
        public bool Running
        {
            set
            {
                SetValue(RunningProperty, value);
            }
            get
            {
                return (bool)GetValue(RunningProperty);
            }
        }

        #endregion

        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Running ? "Running" : "Normal", true);
        }

        static AirHammer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AirHammer), new FrameworkPropertyMetadata(typeof(AirHammer)));
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.RUN };
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
