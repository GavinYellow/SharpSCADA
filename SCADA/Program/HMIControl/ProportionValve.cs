using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    [Startable]
    public class ProportionValve : HMIControlBase
    {
        static ProportionValve()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProportionValve), new FrameworkPropertyMetadata(typeof(ProportionValve)));
        }

        public static readonly DependencyProperty OpenDgreeProperty = DependencyProperty.Register("OpenDgree", typeof(double), typeof(ProportionValve));

        #region HMI属性

        [Category("HMI")]
        public double OpenDgree
        {
            get { return (double)GetValue(OpenDgreeProperty); }
            set { SetValue(OpenDgreeProperty, value); }
        }
        #endregion

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, "OpenDgree", TagActions.DEVICENAME, };
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0,0.95),ConnectOrientation.Left),
                    new  LinkPosition(new Point(1,0.95),ConnectOrientation.Right)
                };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case "OpenDgree":
                    var _funcOpenDgree = tagChanged as Func<float>;
                    if (_funcOpenDgree != null)
                    {
                        return delegate { OpenDgree = _funcOpenDgree(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
