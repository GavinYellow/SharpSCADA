using System.Windows;
using System;
using System.ComponentModel;

namespace HMIControl
{
    [Startable]
    public class Sifter : HMIControlBase
    {
        static Sifter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Sifter), new FrameworkPropertyMetadata(typeof(Sifter)));
        }

        public static DependencyProperty RunningProperty = DependencyProperty.Register("Running", typeof(bool), typeof(Sifter),
           new PropertyMetadata(false, new PropertyChangedCallback(ValueChangedCallback)));

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

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as Sifter, (bool)args.NewValue ? "Active" : "Inactive", true);
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0.19,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.8,0.6),ConnectOrientation.Bottom),
                };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Motor motor = base.GetTemplateChild("motor") as Motor;
            children = new ITagLink[] { motor };
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.RUN, TagActions.DEVICENAME };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.RUN:
                    var _funcInRun = tagChanged as Func<bool>;
                    if (_funcInRun != null)
                    {
                        return delegate { Running = _funcInRun(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
