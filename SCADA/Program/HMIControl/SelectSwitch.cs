using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace HMIControl
{

    public class SelectSwitch : ButtonBase
    {
        public static readonly DependencyProperty LeftLableProperty = DependencyProperty.Register("LeftLable", typeof(string), typeof(SelectSwitch));
        public static readonly DependencyProperty RightLableProperty = DependencyProperty.Register("RightLable", typeof(string), typeof(SelectSwitch));
        public static readonly DependencyProperty IsThreeStateProperty = DependencyProperty.Register("IsThreeState", typeof(bool), typeof(SelectSwitch),
        new FrameworkPropertyMetadata(true));

        static SelectSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectSwitch), new FrameworkPropertyMetadata(typeof(SelectSwitch)));
        }

        [Category("HMI")]
        public bool IsThreeState
        {
            get
            {
                return (bool)base.GetValue(IsThreeStateProperty);
            }
            set
            {
                base.SetValue(IsThreeStateProperty, value);
            }
        }

        [Category("HMI")]
        public string LeftLable
        {
            get
            {
                return (string)base.GetValue(LeftLableProperty);
            }
            set
            {
                base.SetValue(LeftLableProperty, value);
            }
        }

        [Category("HMI")]
        public string RightLable
        {
            get
            {
                return (string)base.GetValue(RightLableProperty);
            }
            set
            {
                base.SetValue(RightLableProperty, value);
            }
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.ENABLE, TagActions.DISABLE, TagActions.STATE, TagActions.LEFT, TagActions.RIGHT, TagActions.MID };
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (IsPulse)
            {
                if (_funcWrites.Count > 0)
                    _funcWrites.ForEach(x => x(false));
            }
            e.Handled = false;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (IsPulse)
            {
                if (_funcWrites.Count > 0)
                    _funcWrites.ForEach(x => x(true));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!IsPulse)
            {
                if (base.IsMouseCaptured)
                {
                    e.Handled = true;
                    double num = base.ActualWidth / 3.0;
                    Point position = Mouse.GetPosition(this);
                    if (position.X < num)
                    {
                        IsChecked = false;
                    }
                    else if (position.X > (2 * num))
                    {
                        IsChecked = true;
                    }
                    else if (IsThreeState)
                    {
                        IsChecked = null;
                    }
                    foreach (var item in _funcints)
                    {
                        _funcints.ForEach(x => x());
                    }
                    //if (_funcWrite != null)
                    //{
                    //    if (!IsThreeState && IsChecked.HasValue)
                    //    {
                    //        _funcWrite(IsChecked);
                    //    }
                    //    else if (IsThreeState)
                    //    {
                    //        _funcWrite(!IsChecked.HasValue ? 0 : IsChecked.Value == true ? 1 : 2);
                    //    }
                    //}
                }
            }
        }

        protected override void OnCheckedChanged(bool? oldstat, bool? newstat)
        {
            if (newstat.HasValue)
            {
                VisualStateManager.GoToState(this, newstat.Value == true ? "R" : "L", true);
            }
            else VisualStateManager.GoToState(this, "M", true);
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.LEFT:
                    var _funcST1 = tagChanged as Func<bool>;
                    if (_funcST1 != null)
                    {
                        return delegate { if (_funcST1())IsChecked = true; };
                    }
                    else return null;
                case TagActions.RIGHT:
                    var _funcST2 = tagChanged as Func<bool>;
                    if (_funcST2 != null)
                    {
                        return delegate { if (_funcST2())IsChecked = false; };
                    }
                    else return null;
                case TagActions.MID:
                    var _funcST3 = tagChanged as Func<bool>;
                    if (_funcST3 != null)
                    {
                        return delegate { if (_funcST3())IsChecked = null; };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
