using System.Windows;
using System.ComponentModel;
using System;

namespace HMIControl
{
    public class LiquidAdd : HMIControlBase
    {
        static LiquidAdd()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LiquidAdd), new FrameworkPropertyMetadata(typeof(LiquidAdd)));
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(float), typeof(LiquidAdd));

        public static readonly DependencyProperty DumpingSwitchProperty =
            DependencyProperty.Register("DumpingSwitch", typeof(bool), typeof(LiquidAdd), new PropertyMetadata(false, DumpingSwitchChanged));

        public static DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(short), typeof(LiquidAdd));

        static void DumpingSwitchChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            VisualStateManager.GoToState(obj as LiquidAdd, (bool)e.NewValue ? "On" : "Off", true);
        }

        #region HMI属性
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
        public bool DumpingSwitch
        {
            set
            {
                SetValue(DumpingSwitchProperty, value);
            }
            get
            {
                return (bool)GetValue(DumpingSwitchProperty);
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
                return (short)GetValue(StatusProperty);
            }
        }

        #endregion

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(1,0.025),ConnectOrientation.Right),
                    new  LinkPosition(new Point(0.5,0.9),ConnectOrientation.Bottom),
                };
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.STATE, TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, "放料阀", "实际值" };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.STATE:
                    var _funcStatus = tagChanged as Func<int>;
                    if (_funcStatus != null)
                    {
                        return delegate { Status = (short)_funcStatus(); };
                    }
                    else return null;
                case "放料阀":
                    var _funcSwitch = tagChanged as Func<bool>;
                    if (_funcSwitch != null)
                    {
                        return delegate
                        {
                            DumpingSwitch = _funcSwitch();
                        };
                    }
                    else return null;
                case "实际值":
                    var _funcWeight = tagChanged as Func<float>;
                    if (_funcWeight != null)
                    {
                        return delegate
                        {
                            Value = _funcWeight();
                        };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var Valve1 = Template.FindName("UnloadValve", this) as HMILable;
            children = new ITagLink[] { Valve1 };
        }

    }
}
