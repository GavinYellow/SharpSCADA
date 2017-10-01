using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class Gate : HMIControlBase
    {
        static Gate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Gate), new FrameworkPropertyMetadata(typeof(Gate)));
        }
        
        public static readonly DependencyProperty GateOpenProperty = 
            DependencyProperty.Register("GateOpen", typeof(bool), typeof(Gate),
            new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        public static readonly DependencyProperty GateCloseProperty =
            DependencyProperty.Register("GateClose", typeof(bool), typeof(Gate),
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
                VisualStateManager.GoToState(this, GateOpen ? "Opened" : "Closed", true);
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.ALARM, TagActions.ON, TagActions.OFF };
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
