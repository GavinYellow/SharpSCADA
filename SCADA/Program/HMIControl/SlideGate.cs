using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class SlideGate : HMIControlBase
    {
        static SlideGate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SlideGate), new FrameworkPropertyMetadata(typeof(SlideGate)));
        }

        public static readonly DependencyProperty GateOpenProperty =
            DependencyProperty.Register("GateOpen", typeof(bool), typeof(SlideGate),
            new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        public static readonly DependencyProperty GateCloseProperty =
            DependencyProperty.Register("GateClose", typeof(bool), typeof(SlideGate),
            new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        #region HMI属性
        [Category("HMI")]
        public bool GateOpen
        {
            set
            {
                SetValue(GateOpenProperty, value);
            }
            get
            {
                return (bool)GetValue(GateOpenProperty);
            }
        }

        public bool GateClose
        {
            set
            {
                SetValue(GateCloseProperty, value);
            }
            get
            {
                return (bool)GetValue(GateCloseProperty);
            }
        }
        #endregion

        protected override void UpdateState()
        {
            if (Alarm)
                VisualStateManager.GoToState(this, "AlarmOn", true);
            else
                VisualStateManager.GoToState(this, GateOpen ? "Open" : "Closed", true);
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.ALARM, TagActions.ON, TagActions.OFF };
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0.4,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.4,1),ConnectOrientation.Bottom),
                };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.ON:
                    var _funcGateOpen = tagChanged as Func<bool>;
                    if (_funcGateOpen != null)
                    {
                        return delegate { GateOpen = _funcGateOpen(); };
                    }
                    else return null;
                case TagActions.OFF:
                    var _funcGateClose = tagChanged as Func<bool>;
                    if (_funcGateClose != null)
                    {
                        return delegate { GateClose = _funcGateClose(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
