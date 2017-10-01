using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    public class LiquidBuf : HMIControlBase
    {
        static LiquidBuf()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LiquidBuf), new FrameworkPropertyMetadata(typeof(LiquidBuf)));
        }

        public static DependencyProperty StatusProperty =
             DependencyProperty.Register("Status", typeof(short), typeof(LiquidBuf));

        #region HMI属性

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
            return new LinkPosition[1]
                {  
                    new  LinkPosition(new Point(0.42,1.0),ConnectOrientation.Bottom),
                };
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.STATE, TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Motor motor = Template.FindName("motor", this) as Motor;
            var Valve2 = Template.FindName("GasValve", this) as HMILable;
            var Discharge = Template.FindName("Discharge", this) as HMIText;

            children = new ITagLink[] { motor, Valve2, Discharge };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.STATE:
                    var _funcStatus = tagChanged as Func<float>;
                    if (_funcStatus != null)
                    {
                        return delegate { Status = (short)_funcStatus(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
