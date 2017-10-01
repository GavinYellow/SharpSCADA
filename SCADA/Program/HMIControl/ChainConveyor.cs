using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    /// <summary>
    /// 链式刮板机
    /// </summary>
    [Startable]
    public class ChainConveyor : HMIControlBase
    {
        static ChainConveyor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChainConveyor), new FrameworkPropertyMetadata(typeof(ChainConveyor)));            
        }
        //State 属性保留，不作显示用
        public static DependencyProperty WorkingProperty = 
            DependencyProperty.Register("Working", typeof(bool), typeof(ChainConveyor),
            new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));        

        public static DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(bool), typeof(ChainConveyor),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender,
                (obj, args) => 
                {
                    var convey = obj as ChainConveyor;
                    if (convey != null) convey.RenderTransform = new System.Windows.Media.ScaleTransform(convey.Transform ? -1 : 1, 1, convey.ActualWidth / 2, convey.ActualHeight / 2);
                }));

        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Alarm ? "AlarmOn" : Working ? "ON" : "OFF", true);
        }

        #region HMI属性
        [Category("HMI")]
        public bool Working
        {
            set
            {
                SetValue(WorkingProperty, value);
            }
            get
            {
                return (bool)GetValue(WorkingProperty);
            }
        }

        [Category("HMI")]
        public bool Transform
        {
            set
            {
                SetValue(TransformProperty, value);
            }
            get
            {
                return (bool)GetValue(TransformProperty);
            }
        }
        #endregion

        public override LinkPosition[] GetLinkPositions()
        {
            var w1 = 26 / this.ActualWidth;
            var w2 = (this.ActualWidth - 21) / this.ActualWidth;

            return 
                new LinkPosition[2]
                {
                    new  LinkPosition(new Point(w1 , 0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(w2 , 1),ConnectOrientation.Bottom),
                };
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.RUN, TagActions.ALARM };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.RUN:
                    var _funcRun = tagChanged as Func<bool>;
                    if (_funcRun != null)
                    {
                        return delegate { Working = _funcRun(); };
                        
                    }
                    else return null;                
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}