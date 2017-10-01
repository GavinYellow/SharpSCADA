using System.Windows;
using System.ComponentModel;
using System;

namespace HMIControl
{

    public class Cyclone : HMIControlBase
    {
        public static readonly DependencyProperty HighLevelProperty = DependencyProperty.Register("HighLevel", typeof(bool), typeof(Cyclone),
            new PropertyMetadata(false, OnHighLevelChanged));

        [Category("HMI")]
        public bool HighLevel
        {
            set
            {
                SetValue(HighLevelProperty, value);
            }
            get
            {
                return (bool)GetValue(HighLevelProperty);
            }
        }
        
        static Cyclone()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Cyclone), new FrameworkPropertyMetadata(typeof(Cyclone)));
        }

        private static void OnHighLevelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as Cyclone, (bool)args.NewValue ? "aboveHigh" : "belowHigh", true);
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.HIGHLEVEL,};
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Fan fan = base.GetTemplateChild("fan") as Fan;
            Motor motor = base.GetTemplateChild("motor") as Motor;
            children = new ITagLink[] { motor, fan };
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[3]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.4,1),ConnectOrientation.Bottom),
                    new  LinkPosition(new Point(1,0.35),ConnectOrientation.Right)
                };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.HIGHLEVEL:
                    var _funcHighLevel = tagChanged as Func<bool>;
                    if (_funcHighLevel != null)
                    {
                        return delegate { HighLevel = _funcHighLevel(); };
                    }
                    else return null;
                case TagActions.CAPTION:
                    var _funcCap = tagChanged as Func<string>;
                    if (_funcCap != null)
                    {
                        return delegate { Caption = _funcCap(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
