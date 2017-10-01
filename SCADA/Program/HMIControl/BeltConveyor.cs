using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
     [Startable]
    public class BeltConveyor : HMIControlBase
    {
        static BeltConveyor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BeltConveyor), new FrameworkPropertyMetadata(typeof(BeltConveyor)));
        }
        public static DependencyProperty WorkingProperty =
               DependencyProperty.Register("Working", typeof(bool), typeof(BeltConveyor),
               new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        public static DependencyProperty LeftDrectionProperty =
            DependencyProperty.Register("LeftDrection", typeof(bool), typeof(BeltConveyor),
            new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        public static DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(bool), typeof(BeltConveyor),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender,
                (obj, args) =>
                {
                    var convey = obj as BeltConveyor;
                    if (convey != null) convey.RenderTransform = new System.Windows.Media.ScaleTransform(convey.Transform ? -1 : 1, 1, convey.ActualWidth / 2, convey.ActualHeight / 2);
                }));

        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Alarm ? "AlarmOn" : Working ? (LeftDrection ? "ONL" : "ONR") : "OFF", true);
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
        public bool LeftDrection
        {
            set
            {
                SetValue(LeftDrectionProperty, value);
            }
            get
            {
                return (bool)GetValue(LeftDrectionProperty);
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
