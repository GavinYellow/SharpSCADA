using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class Elevator : HMIControlBase
    {
        static Elevator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Elevator), new FrameworkPropertyMetadata(typeof(Elevator)));
        }

        public static readonly DependencyProperty AmpsProperty =
            DependencyProperty.Register("Amps", typeof(float), typeof(Elevator));

        public static readonly DependencyProperty RPMProperty =
            DependencyProperty.Register("RPM", typeof(float), typeof(Elevator));

        public static readonly DependencyProperty RunningProperty =
            DependencyProperty.Register("Running", typeof(bool), typeof(Elevator),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnValueChanged)));

        #region HMI属性
        [Category("HMI")]
        public float Amps
        {
            set
            {
                SetValue(AmpsProperty, value);
            }
            get
            {
                return (float)GetValue(AmpsProperty);
            }
        }

        [Category("HMI")]
        public float RPM
        {
            set
            {
                SetValue(RPMProperty, value);
            }
            get
            {
                return (float)GetValue(RPMProperty);
            }
        }

        [Category("HMI")]
        public bool Running
        {
            get { return (bool)GetValue(RunningProperty); }
            set { SetValue(RunningProperty, value); }
        }
        #endregion

        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Alarm ? "AlarmOn" : Running ? "ON" : "OFF", true);
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.ALARM, "LowSpeed", "OverCurrent", TagActions.RUN, TagActions.AMPS, TagActions.SPEED, "跑偏" }; 
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case "跑偏":
                    var _funcSliding = tagChanged as Func<bool>;
                    if (_funcSliding != null)
                    {
                        return delegate { VisualStateManager.GoToState(this, _funcSliding() ? "Sliding" : "Normal", true); };
                    }
                    else return null;
                case TagActions.RUN:
                    var _funcInRun = tagChanged as Func<bool>;
                    if (_funcInRun != null)
                    {
                        return delegate { Running = _funcInRun(); };
                    }
                    else return null;
                case "LowSpeed":
                    var _funcLowSpeed = tagChanged as Func<bool>;
                    if (_funcLowSpeed != null)
                    {
                        return delegate { VisualStateManager.GoToState(this, _funcLowSpeed() ? "Lossspd" : "Normalspd", true); };
                    }
                    else return null;
                case "OverCurrent":
                    var _funcOverCurrent = tagChanged as Func<bool>;
                    if (_funcOverCurrent != null)
                    {
                        return delegate { VisualStateManager.GoToState(this, _funcOverCurrent() ? "OverCurrent" : "NormalCurrent", true); };
                    }
                    else return null;
                case TagActions.AMPS:
                    var _funcAmps = tagChanged as Func<float>;
                    if (_funcAmps != null)
                    {
                        return delegate { Amps = _funcAmps(); };
                    }
                    else return null;
                case TagActions.SPEED:
                    var _funcRPM = tagChanged as Func<int>;
                    if (_funcRPM != null)
                    {
                        return delegate { RPM = _funcRPM(); };
                    }
                    var _funcRPMf = tagChanged as Func<float>;
                    if (_funcRPMf != null)
                    {
                        return delegate { RPM = _funcRPMf(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
