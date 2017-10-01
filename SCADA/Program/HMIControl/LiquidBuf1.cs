using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    public class LiquidBuf1 : HMIControlBase
    {
        public static DependencyProperty InOilProperty = DependencyProperty.Register("InOil", typeof(bool), typeof(LiquidBuf1),
            new PropertyMetadata(new PropertyChangedCallback(InOilChangedCallback)));

        public static DependencyProperty SparyOilProperty = DependencyProperty.Register("SparyOil", typeof(bool), typeof(LiquidBuf1),
          new PropertyMetadata(new PropertyChangedCallback(SparyOilChangedCallback)));

        public static DependencyProperty HighLevelProperty = DependencyProperty.Register("HighLevel", typeof(bool), typeof(LiquidBuf1),
          new PropertyMetadata(new PropertyChangedCallback(HighLevelChangedCallback)));

        static LiquidBuf1()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LiquidBuf1), new FrameworkPropertyMetadata(typeof(LiquidBuf1)));
        }

        private static void InOilChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as LiquidBuf1, (bool)args.NewValue ? "Open" : "Close", true);
        }

        private static void SparyOilChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as LiquidBuf1, (bool)args.NewValue ? "On" : "Off", true);
        }

        private static void HighLevelChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as LiquidBuf1, (bool)args.NewValue ? "highLevelon" : "HighLevelOFF", true);
        }
        #region HMI属性
        [Category("HMI")]
        public bool InOil
        {
            set
            {
                SetValue(InOilProperty, value);
            }
            get
            {
                return (bool)GetValue(InOilProperty);
            }
        }

        [Category("HMI")]
        public bool SparyOil
        {
            set
            {
                SetValue(SparyOilProperty, value);
            }
            get
            {
                return (bool)GetValue(SparyOilProperty);
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
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var InOil_motor = base.GetTemplateChild("InOil_motor") as Motor;
            var Spray_motor = base.GetTemplateChild("Spray_motor") as Motor;
            children = new ITagLink[] { Spray_motor, InOil_motor };
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0,0.8),ConnectOrientation.Left),
                    new  LinkPosition(new Point(1,0.8),ConnectOrientation.Right)
                };
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.HIGHLEVEL, "进油", "喷油" };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case "进油":
                    var _funcOn = tagChanged as Func<bool>;
                    if (_funcOn != null)
                    {
                        return delegate { InOil = _funcOn(); };
                    }
                    else return null;
                case "喷油":
                    var _funcOff = tagChanged as Func<bool>;
                    if (_funcOff != null)
                    {
                        return delegate { SparyOil = _funcOff(); };
                    }
                    else return null;
                case TagActions.HIGHLEVEL:
                    var _funcAlarm = tagChanged as Func<bool>;
                    if (_funcAlarm != null)
                    {
                        return delegate { HighLevel = _funcAlarm(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
