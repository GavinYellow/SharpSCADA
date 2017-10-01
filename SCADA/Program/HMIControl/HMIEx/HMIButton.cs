using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace HMIControl
{
    public class HMIButton : Button, ITagReader, ITagWriter, ITagWindow
    {
        public static readonly DependencyProperty TagReadTextProperty = DependencyProperty.Register("TagReadText", typeof(string), typeof(HMIButton));
        public static readonly DependencyProperty TagWriteTextProperty = DependencyProperty.Register("TagWriteText", typeof(string), typeof(HMIButton));
        public static readonly DependencyProperty TagWindowTextProperty = DependencyProperty.Register("TagWindowText", typeof(string), typeof(HMIButton));
        public static readonly DependencyProperty IsModelProperty = DependencyProperty.Register("IsModel", typeof(bool), typeof(HMIButton));
        public static readonly DependencyProperty IsUniqueProperty = DependencyProperty.Register("IsUnique", typeof(bool), typeof(HMIButton));
        public static readonly DependencyProperty IsPulseProperty = DependencyProperty.Register("IsPulse", typeof(bool), typeof(HMIButton));
        public static readonly DependencyProperty PulseWriteInt16Property =
            DependencyProperty.Register("PulseWriteInt16", typeof(bool), typeof(HMIButton));

        public string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.ALARM, TagActions.DEVICENAME };
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

        [Category("Common")]
        public string TagReadText
        {
            get
            {
                return (string)base.GetValue(TagReadTextProperty);
            }
            set
            {
                base.SetValue(TagReadTextProperty, value);
            }
        }

        [Category("Common")]
        public string TagWriteText
        {
            get
            {
                return (string)base.GetValue(TagWriteTextProperty);
            }
            set
            {
                base.SetValue(TagWriteTextProperty, value);
            }
        }

        [Category("HMI")]
        public bool IsPulse
        {
            get
            {
                return (bool)base.GetValue(IsPulseProperty);
            }
            set
            {
                base.SetValue(IsPulseProperty, value);
            }
        }

        [Category("HMI")]
        public bool PulseWriteInt16
        {
            get
            {
                return (bool)base.GetValue(PulseWriteInt16Property);
            }
            set
            {
                base.SetValue(PulseWriteInt16Property, value);
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

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (PulseWriteInt16)
            {
                if (_funcWrites.Count > 0)
                    _funcWrites.ForEach(x => x(0x2));
            }
            else
            {
                if (_funcWrites.Count > 0)
                    _funcWrites.ForEach(x => x(false));
            }
            e.Handled = false;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (_funcints.Count > 0)
                _funcints.ForEach(x => x());
            if (PulseWriteInt16)
            {
                if (_funcWrites.Count > 0)
                    _funcWrites.ForEach(x => x(0x1));
            }
            else
            {
                if (_funcWrites.Count > 0)
                    _funcWrites.ForEach(x => x(true));
            }
        }

        ColorAnimationUsingKeyFrames animation;
        SolidColorBrush brush;

        public Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.ALARM:
                    var _funcAlarm = tagChanged as Func<bool>;
                    if (_funcAlarm != null)
                    {
                        if (animation == null)
                        {
                            brush = Background as SolidColorBrush;
                            if (brush != null)
                            {
                                brush = brush.Clone();
                                animation = new ColorAnimationUsingKeyFrames
                                {
                                    KeyFrames = new ColorKeyFrameCollection
                                        {
                                            new DiscreteColorKeyFrame(brush.Color, TimeSpan.FromSeconds(0.0)),
                                            new DiscreteColorKeyFrame(Colors.Red, TimeSpan.FromSeconds(0.5)),
                                            new DiscreteColorKeyFrame(brush.Color, TimeSpan.FromSeconds(1))
                                        },
                                    AutoReverse = true,
                                    RepeatBehavior = RepeatBehavior.Forever
                                };
                            }
                        }
                        return delegate
                         {
                             if (brush != null)
                             {
                                 if (_funcAlarm())
                                 {
                                     brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                                     Background = brush;
                                 }
                                 else
                                 {
                                     brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
                                     Background = brush;
                                 }
                             }
                         };
                    }
                    else
                        return null;
                case TagActions.VISIBLE:
                    var _funcVisible = tagChanged as Func<bool>;
                    if (_funcVisible != null)
                    {
                        return delegate
                        {
                            this.Visibility = _funcVisible() ? Visibility.Visible : Visibility.Hidden;
                        };
                    }
                    else
                        return null;
                case TagActions.CAPTION:
                    var _funcCaption = tagChanged as Func<string>;
                    if (_funcCaption != null)
                    {
                        return delegate { this.Content = _funcCaption(); };
                    }
                    else
                        return null;
            }
            return null;
        }

        List<Func<object, int>> _funcWrites = new List<Func<object,int>>();
        List<Func<int>> _funcints = new List<Func<int>>();
        public bool SetTagWriter(IEnumerable<Delegate> tagWriter)
        {
            bool ret = true;
            _funcWrites.Clear();
            _funcints.Clear();
            foreach (var item in tagWriter)
            {
                Func<object, int> _funcWrite = item as Func<object, int>;

                if (_funcWrite != null)
                    _funcWrites.Add(_funcWrite);
                else
                {
                    Func<int> _funcint = item as Func<int>;
                    if (_funcint != null)
                        _funcints.Add(_funcint);
                    else
                    {
                        ret = false;
                        break;
                    }
                }
            }
            return ret;
        }

        public IList<ITagLink> Children
        {
            get { return null; }
        }

        public string Node
        {
            get { return this.Name; }
        }
    }

}
