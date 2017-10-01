using System.Windows;
using System.ComponentModel;
using System;

namespace HMIControl
{
    [Startable]
    public class Grind : HMIControlBase
    {
        static Grind()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Grind), new FrameworkPropertyMetadata(typeof(Grind)));
        }

        public static readonly DependencyProperty AmpsProperty = DependencyProperty.Register("Amps", typeof(float), typeof(Grind));

        public static readonly DependencyProperty StatusProperty =
           DependencyProperty.Register("Status", typeof(short), typeof(Grind));

        #region HMI属性

        [Category("HMI")]
        public float Amps
        {
            get { return (float)GetValue(AmpsProperty); }
            set { SetValue(AmpsProperty, value); }
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
                    new  LinkPosition(new Point(0.3,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.3,1),ConnectOrientation.Bottom),
                };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var feedmotor = base.GetTemplateChild("motor") as Motor;
            children = new ITagLink[] { feedmotor };
        }

        public override string[] GetActions()
        {
            return new string[] {"状态", TagActions.VISIBLE, TagActions.CAPTION, TagActions.AMPS, TagActions.DEVICENAME }; 
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
                
                case TagActions.AMPS:
                    var _funcAmps = tagChanged as Func<float>;
                    if (_funcAmps != null)
                    {
                        return delegate { Amps = _funcAmps(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }

    }
}
