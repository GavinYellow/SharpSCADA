using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace HMIControl
{
    [Startable]
    public class FromTo : HMIControlBase, ITagWindow
    {
        static FromTo()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FromTo), new FrameworkPropertyMetadata(typeof(FromTo)));
        }

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);
            if (!string.IsNullOrEmpty(TagWindowText))
            {
                this.AddHandler(UIElement.MouseEnterEvent, new MouseEventHandler(FromTo_MouseEnter));
                this.AddHandler(UIElement.MouseLeaveEvent, new MouseEventHandler(FromTo_MouseLeave));
            }
        }

        void FromTo_MouseEnter(object sender, MouseEventArgs e)
        {
            AdornerLayer lay = AdornerLayer.GetAdornerLayer(this);
            if (lay != null)
            {
                FrameAdorner frame = new FrameAdorner(this);
                lay.Add(frame);
            }
        }

        void FromTo_MouseLeave(object sender, MouseEventArgs e)
        {
            AdornerLayer lay = AdornerLayer.GetAdornerLayer(this);
            if (lay != null)
            {
                var adorners = lay.GetAdorners(this);
                if (adorners != null)
                {
                    for (int i = 0; i < adorners.Length; i++)
                    {
                        var frame = adorners[i] as FrameAdorner;
                        if (frame != null)
                        {
                            lay.Remove(frame);
                            return;
                        }
                    }
                }
            }
        }

        public static readonly DependencyProperty TagWindowTextProperty = DependencyProperty.Register("TagWindowText", typeof(string), typeof(FromTo));
        public static readonly DependencyProperty IsModelProperty = DependencyProperty.Register("IsModel", typeof(bool), typeof(FromTo));
        public static readonly DependencyProperty IsUniqueProperty = DependencyProperty.Register("IsUnique", typeof(bool), typeof(FromTo));

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[4]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.5,1),ConnectOrientation.Bottom),
                    new  LinkPosition(new Point(0,0.5),ConnectOrientation.Left),
                    new  LinkPosition(new Point(1,0.5),ConnectOrientation.Right)
                };
        }

        [Category("Common")]
        public string TagWindowText
        {
            get
            {
                return (string)base.GetValue(TagWindowTextProperty);
            }
            set
            {
                base.SetValue(TagWindowTextProperty, value);
            }
        }

        [Category("Common")]
        public bool IsModel
        {
            get
            {
                return (bool)base.GetValue(IsModelProperty);
            }
            set
            {
                base.SetValue(IsModelProperty, value);
            }
        }

        [Category("HMI")]
        public bool IsUnique
        {
            get
            {
                return (bool)base.GetValue(IsUniqueProperty);
            }
            set
            {
                base.SetValue(IsUniqueProperty, value);
            }
        }
    }
}
