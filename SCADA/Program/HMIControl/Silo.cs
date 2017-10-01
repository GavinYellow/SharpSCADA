using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace HMIControl
{

    public class Silo : HMIControlBase
    {
        public static readonly DependencyProperty LowLevelProperty = DependencyProperty.Register("LowLevel", typeof(bool), typeof(Silo),
           new PropertyMetadata(false, OnLowLevelChanged));

        public static readonly DependencyProperty HighLevelProperty = DependencyProperty.Register("HighLevel", typeof(bool), typeof(Silo),
            new PropertyMetadata(false, OnHighLevelChanged));

        static Silo()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Silo), new FrameworkPropertyMetadata(typeof(Silo)));
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.5,1),ConnectOrientation.Bottom),
                };
        }

        [Category("HMI")]
        public bool LowLevel
        {
            set
            {
                SetValue(LowLevelProperty, value);
            }
            get
            {
                return (bool)GetValue(LowLevelProperty);
            }
        }

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
        

        private static void OnLowLevelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as Silo, (bool)args.NewValue ? "belowLow" : "aboveLow", true);
        }

        private static void OnHighLevelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as Silo, (bool)args.NewValue ? "aboveHigh" : "belowHigh", true);
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.LOWLEVEL, TagActions.HIGHLEVEL, };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.LOWLEVEL:
                    var _funcLowLevel = tagChanged as Func<bool>;
                    if (_funcLowLevel != null)
                    {
                        return delegate { LowLevel = _funcLowLevel(); };
                    }
                    else return null;
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
