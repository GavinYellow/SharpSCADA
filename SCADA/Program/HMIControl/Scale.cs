using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{

    public class Scale : HMIControlBase
    {
        static Scale()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Scale), new FrameworkPropertyMetadata(typeof(Scale)));
        }

        public static readonly DependencyProperty DeviceConnectedProperty =
           DependencyProperty.Register("DeviceConnected", typeof(bool), typeof(Scale),
           new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register("Value", typeof(float), typeof(Scale));
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(float), typeof(Scale));
        public static readonly DependencyProperty StatusProperty =
           DependencyProperty.Register("Status", typeof(short), typeof(Scale));

        #region HMI属性
        [Category("HMI")]
        public bool DeviceConnected
        {
            set
            {
                SetValue(DeviceConnectedProperty, value);
            }
            get
            {
                return (bool)GetValue(DeviceConnectedProperty);
            }
        }

        [Category("HMI")]
        public float Value
        {
            set
            {
                SetValue(ValueProperty, value);
            }
            get
            {
                return (float)GetValue(ValueProperty);
            }
        }

        [Category("HMI")]
        public float Target
        {
            set
            {
                SetValue(TargetProperty, value);
            }
            get
            {
                return (float)GetValue(TargetProperty);
            }
        }

        [Category("HMI")]
        public short Status
        {
            set
            {
                SetValue(StatusProperty, value);
            }
            get
            {
                return (Int16)GetValue(StatusProperty);
            }
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Gate gate = Template.FindName("gate", this) as Gate;
            children = new ITagLink[] { gate };
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
            return new string[] { "状态", "仪表连接", TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.PV, "目标值", "重量超差", "皮重超差" };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case "状态":
                    var _funcStatus = tagChanged as Func<int>;
                    if (_funcStatus != null)
                    {
                        return delegate { Status = (short)_funcStatus(); };
                    }
                    else return null;
                case "仪表连接":
                    var _funcConnected = tagChanged as Func<bool>;
                    if (_funcConnected != null)
                    {
                        return delegate { DeviceConnected = _funcConnected(); };
                    }
                    else return null;
                case TagActions.PV:
                    var _funcPV = tagChanged as Func<float>;
                    if (_funcPV != null)
                    {
                        return delegate { Value = _funcPV(); };
                    }
                    else return null;
                case "目标值":
                    var _funcTarget = tagChanged as Func<float>;
                    if (_funcTarget != null)
                    {
                        return delegate { Target = _funcTarget(); };
                    }
                    else return null;
                case "重量超差":
                    var _funcWeighOverDiff = tagChanged as Func<bool>;
                    if (_funcWeighOverDiff != null)
                    {
                        return delegate
                        {
                            VisualStateManager.GoToState(this, (bool)_funcWeighOverDiff() ? "WeighOverYes" : "WeighOverNo", true);
                        };
                    }
                    else return null;
                case "皮重超差":
                    var _funcInitWeighOverDiff = tagChanged as Func<bool>;
                    if (_funcInitWeighOverDiff != null)
                    {
                        return delegate
                        {
                            VisualStateManager.GoToState(this, (bool)_funcInitWeighOverDiff() ? "InitWeighOverYes" : "InitWeighOverNo", true);
                        };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
    public class Check : Scale
    {
        static Check()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Check), new FrameworkPropertyMetadata(typeof(Check)));
        }
    }
}
