using System.Windows;
using System;
using System.ComponentModel;

namespace HMIControl
{
    /// <summary>
    ///     <MyNamespace:FeedBin/>
    ///   喂料仓
    /// </summary>
    [Startable]
    public class FeedBin : HMIControlBase
    {
        static FeedBin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FeedBin), new FrameworkPropertyMetadata(typeof(FeedBin)));
        }

        //public static readonly DependencyProperty HammarOnProperty =
        //    DependencyProperty.Register("HammarOn", typeof(bool), typeof(FeedBin));

        //public static readonly DependencyProperty GateOpenProperty =
        //    DependencyProperty.Register("GateOpen", typeof(bool), typeof(FeedBin));

        //public static readonly DependencyProperty IsFeedingProperty =
        //    DependencyProperty.Register("IsFeeding", typeof(bool), typeof(FeedBin),
        //    new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnValueChanged)));

        public static readonly DependencyProperty BelowSQLProperty = 
            DependencyProperty.Register("BelowSQL", typeof(bool), typeof(FeedBin),
            new PropertyMetadata(false));

        public static readonly DependencyProperty AboveSQHProperty = 
            DependencyProperty.Register("AboveSQH", typeof(bool), typeof(FeedBin),
            new PropertyMetadata(false));

        public static readonly DependencyProperty RawNameProperty = 
            DependencyProperty.Register("RawName", typeof(string), typeof(FeedBin));

        public static readonly DependencyProperty BinNameProperty =
            DependencyProperty.Register("BinName", typeof(string), typeof(FeedBin));

        public static readonly DependencyProperty PVProperty =
           DependencyProperty.Register("PV", typeof(double), typeof(FeedBin), new PropertyMetadata(0.0, IsStoreVolumnChanged));

        public static readonly DependencyProperty CVProperty =
            DependencyProperty.Register("CV", typeof(double), typeof(FeedBin), new PropertyMetadata(100.0, IsStoreVolumnChanged));

        public static readonly DependencyProperty VolumnProperty = DependencyProperty.Register("Volumn", typeof(double), typeof(FeedBin));

        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Alarm ? "Alarming" : "Normal", true);
        }

        #region HMI属性

        [Category("HMI")]
        public bool BelowSQL
        {
            set
            {
                SetValue(BelowSQLProperty, value);
            }
            get
            {
                return (bool)GetValue(BelowSQLProperty);
            }
        }

        [Category("HMI")]
        public bool AboveSQH
        {
            set
            {
                SetValue(AboveSQHProperty, value);
            }
            get
            {
                return (bool)GetValue(AboveSQHProperty);
            }
        }

        [Category("HMI")]
        public string RawName
        {
            set
            {
                SetValue(RawNameProperty, value);
            }
            get
            {
                return (string)GetValue(RawNameProperty);
            }
        }

        [Category("HMI")]
        public string BinName
        {
            set
            {
                SetValue(BinNameProperty, value);
            }
            get
            {
                return (string)GetValue(BinNameProperty);
            }
        }

        [Category("HMI")]
        public double PV
        {
            get { return (double)base.GetValue(PVProperty); }
            set { base.SetValue(PVProperty, value); }
        }

        [Category("HMI")]
        public double CV
        {
            get { return (double)base.GetValue(CVProperty); }
            set { base.SetValue(CVProperty, value); }
        }

        [Category("HMI")]
        public double Volumn
        {
            get { return (double)base.GetValue(VolumnProperty); }
            set { base.SetValue(VolumnProperty, value); }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            CutoffGate storegate = Template.FindName("cutoffGate1", this) as CutoffGate;
            AirHammer airHammer = Template.FindName("airHammer1", this) as AirHammer;
            Motor motor = Template.FindName("motor1", this) as Motor;
            children = new ITagLink[] { storegate, airHammer, motor };
        }

        static void IsStoreVolumnChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FeedBin fs = o as FeedBin;
            if (fs.PV > 0.0)
            {
                double ratio = fs.CV / fs.PV;
                fs.Volumn = Math.Min(Math.Max(ratio, 0.0), 1.0) * fs.Height;
            }
        }        

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.5,1),ConnectOrientation.Bottom),
                };
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.ALARM, TagActions.LOWLEVEL, TagActions.HIGHLEVEL, TagActions.RAWNAME, TagActions.SP, TagActions.PV }; 
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.LOWLEVEL:
                    var _funcLowLevel = tagChanged as Func<bool>;
                    if (_funcLowLevel != null)
                    {
                        return delegate { BelowSQL = _funcLowLevel(); };
                    }
                    else return null;
                case TagActions.HIGHLEVEL:
                    var _funcHighLevel = tagChanged as Func<bool>;
                    if (_funcHighLevel != null)
                    {
                        return delegate { AboveSQH = _funcHighLevel(); };
                    }
                    else return null;
                case TagActions.RAWNAME:
                    var _funcRaw = tagChanged as Func<string>;
                    if (_funcRaw != null)
                    {
                        return delegate { RawName = _funcRaw(); };
                    }
                    else return null;
                case TagActions.CAPTION:
                    var _funcCap = tagChanged as Func<string>;
                    if (_funcCap != null)
                    {
                        return delegate { Caption = _funcCap(); };
                    }
                    else return null;
                case TagActions.SP:
                    var _funSp = tagChanged as Func<float>;
                    if (_funSp != null)
                    {
                        return delegate { CV = _funSp(); };
                    }
                    else return null;
                case TagActions.PV:
                    var _funPv = tagChanged as Func<float>;
                    if (_funPv != null)
                    {
                        return delegate { PV = _funPv(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
