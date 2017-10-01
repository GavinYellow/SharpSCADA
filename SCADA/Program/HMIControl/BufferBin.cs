using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{

    public class BufferBin : HMIControlBase
    {
        static BufferBin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BufferBin), new FrameworkPropertyMetadata(typeof(BufferBin)));
        }

        public static readonly DependencyProperty LowLevelProperty = DependencyProperty.Register("LowLevel", typeof(bool), typeof(BufferBin),
          new PropertyMetadata(false, OnLowLevelChanged));

        public static readonly DependencyProperty HighLevelProperty = DependencyProperty.Register("HighLevel", typeof(bool), typeof(BufferBin),
            new PropertyMetadata(false, OnHighLevelChanged));

        public static readonly DependencyProperty RawNameProperty = DependencyProperty.Register("RawName", typeof(string), typeof(BufferBin));

        public static readonly DependencyProperty PVProperty =
           DependencyProperty.Register("PV", typeof(double), typeof(BufferBin), new PropertyMetadata(0.0, IsStoreVolumnChanged));

        public static readonly DependencyProperty CVProperty =
            DependencyProperty.Register("CV", typeof(double), typeof(BufferBin), new PropertyMetadata(100.0, IsStoreVolumnChanged));

        public static readonly DependencyProperty VolumnProperty = DependencyProperty.Register("Volumn", typeof(double), typeof(BufferBin));

        #region HMI属性

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

        static void IsStoreVolumnChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BufferBin fs = o as BufferBin;
            if (fs.PV > 0.0)
            {
                double ratio = fs.CV / fs.PV;
                fs.Volumn = Math.Min(Math.Max(ratio, 0.0), 1.0) * fs.Height * 95 / 160;
            }
        }

        private static void OnLowLevelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as BufferBin, (bool)args.NewValue ? "belowLow" : "aboveLow", true);
        }

        private static void OnHighLevelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as BufferBin, (bool)args.NewValue ? "aboveHigh" : "belowHigh", true);
        }

        //{Binding LowLevel, Converter={StaticResource BTV}, RelativeSource={RelativeSource TemplatedParent}}
        //

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Gate storegate = Template.FindName("storegate", this) as Gate;
            children = new ITagLink[] { storegate };
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
            return new string[] { TagActions.DEVICENAME, TagActions.VISIBLE, TagActions.CAPTION, TagActions.LOWLEVEL, TagActions.HIGHLEVEL, TagActions.RAWNAME, TagActions.SP, TagActions.PV };
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
