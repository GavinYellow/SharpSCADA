using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace HMIControl
{

    public class ButtonBase : HMIControlBase, ITagWriter
    {
        public static DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool?), typeof(ButtonBase),
            new FrameworkPropertyMetadata(null, OnIsCheckedPropertyChanged));

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ButtonBase));
        public static readonly DependencyProperty ClickModeProperty = DependencyProperty.Register("ClickMode", typeof(ClickMode), typeof(ButtonBase),
            new FrameworkPropertyMetadata(ClickMode.Release), new ValidateValueCallback(ButtonBase.IsValidClickMode));

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(ButtonBase), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ButtonBase));

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(ButtonBase), new FrameworkPropertyMetadata(null));

        internal static readonly DependencyPropertyKey IsPressedPropertyKey = DependencyProperty.RegisterReadOnly("IsPressed", typeof(bool), typeof(ButtonBase), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

        public static readonly DependencyProperty TagWriteTextProperty = DependencyProperty.Register("TagWriteText", typeof(string), typeof(ButtonBase));

        public static readonly DependencyProperty IsPulseProperty = DependencyProperty.Register("IsPulse", typeof(bool), typeof(ButtonBase), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(int), typeof(ButtonBase));

        private DispatcherTimer _timer;
        // Events
        public event RoutedEventHandler Click
        {
            add
            {
                base.AddHandler(ClickEvent, value);
            }
            remove
            {
                base.RemoveHandler(ClickEvent, value);
            }
        }

        // Methods
        static ButtonBase()
        {
            EventManager.RegisterClassHandler(typeof(ButtonBase), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(ButtonBase.OnAccessKeyPressed));
        }

        protected ButtonBase()
        {
            this.Cursor = Cursors.Hand;
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.STATE, TagActions.ENABLE, TagActions.DISABLE };
        }

        private static void OnIsCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ButtonBase b = d as ButtonBase;
            b.OnCheckedChanged((bool?)e.OldValue, (bool?)e.NewValue);
        }

        protected virtual void OnCheckedChanged(bool? oldstat, bool? newstat)
        {
        }

        protected void StartTimer()
        {
            if (this._timer == null)
            {
                this._timer = new DispatcherTimer();
                this._timer.Tick += new EventHandler(this.OnTimeout);
            }
            else if (this._timer.IsEnabled)
            {
                return;
            }
            this._timer.Interval = TimeSpan.FromMilliseconds(1);
            this._timer.Start();
        }

        protected void StopTimer()
        {
            if (this._timer != null)
            {
                this._timer.Stop();
            }
        }

        private void OnTimeout(object sender, EventArgs e)
        {
            int interval = this.Interval;
            if (this._timer.Interval.Milliseconds != interval)
            {
                this._timer.Interval = TimeSpan.FromMilliseconds((double)interval);
            }
            if (this.IsPressed)
            {
                this.OnClick();
            }
        }

        private bool GetMouseLeftButtonReleased()
        {
            return (InputManager.Current.PrimaryMouseDevice.LeftButton == MouseButtonState.Released);
        }

        private bool HandleIsMouseOverChanged()
        {
            if (this.ClickMode != ClickMode.Hover)
            {
                return false;
            }
            if (base.IsMouseOver)
            {
                this.SetIsPressed(true);
                this.OnClick();
            }
            else
            {
                this.SetIsPressed(false);
            }
            return true;
        }

        private static bool IsValidClickMode(object o)
        {
            ClickMode mode = (ClickMode)o;
            if ((mode != ClickMode.Press) && (mode != ClickMode.Release))
            {
                return (mode == ClickMode.Hover);
            }
            return true;
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            if (e.IsMultiple)
            {
                base.OnAccessKey(e);
            }
            else
            {
                this.OnClick();
            }
        }

        private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
        {
            if ((!e.Handled && (e.Scope == null)) && (e.Target == null))
            {
                e.Target = (UIElement)sender;
            }
        }

        protected virtual void OnClick()
        {
            RoutedEventArgs e = new RoutedEventArgs(ClickEvent, this);
            base.RaiseEvent(e);
            if (this.Command != null)
                this.Command.Execute(this.CommandParameter);
        }


        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (this.ClickMode != ClickMode.Hover)
            {
                if (e.Key == Key.Space)
                {
                    if ((((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.Alt) && !base.IsMouseCaptured) && (e.OriginalSource == this))
                    {
                        this.IsSpaceKeyDown = true;
                        this.SetIsPressed(true);
                        base.CaptureMouse();
                        if (this.ClickMode == ClickMode.Press)
                        {
                            this.OnClick();
                        }
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Return)
                {
                    if (e.OriginalSource == this)
                    {
                        this.IsSpaceKeyDown = false;
                        this.SetIsPressed(false);
                        if (base.IsMouseCaptured)
                        {
                            base.ReleaseMouseCapture();
                        }
                        this.OnClick();
                        e.Handled = true;
                    }
                }
                else if (this.IsSpaceKeyDown)
                {
                    this.SetIsPressed(false);
                    this.IsSpaceKeyDown = false;
                    if (base.IsMouseCaptured)
                    {
                        base.ReleaseMouseCapture();
                    }
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if ((this.ClickMode != ClickMode.Hover) && (((e.Key == Key.Space) && this.IsSpaceKeyDown) && ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.Alt)))
            {
                this.IsSpaceKeyDown = false;
                if (this.GetMouseLeftButtonReleased())
                {
                    bool flag = this.IsPressed && (this.ClickMode == ClickMode.Release);
                    if (base.IsMouseCaptured)
                    {
                        base.ReleaseMouseCapture();
                    }
                    if (flag)
                    {
                        this.OnClick();
                    }
                }
                else if (base.IsMouseCaptured)
                {
                    this.UpdateIsPressed();
                }
                e.Handled = true;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            if ((this.ClickMode != ClickMode.Hover) && (e.OriginalSource == this))
            {
                if (this.IsPressed)
                {
                    this.SetIsPressed(false);
                }
                if (base.IsMouseCaptured)
                {
                    base.ReleaseMouseCapture();
                }
                this.IsSpaceKeyDown = false;
            }
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            if (((e.OriginalSource == this) && (this.ClickMode != ClickMode.Hover)) && !this.IsSpaceKeyDown)
            {
                if (base.IsKeyboardFocused && !this.IsInMainFocusScope)
                {
                    Keyboard.Focus(null);
                }
                this.SetIsPressed(false);
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (this.HandleIsMouseOverChanged())
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (this.HandleIsMouseOverChanged())
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (Interval > 0)
            {
                this.IsPressed = true;
                this.StartTimer();
            }
            if (this.ClickMode != ClickMode.Hover)
            {
                //e.Handled = true; 
                base.Focus();
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    base.CaptureMouse();
                    if (base.IsMouseCaptured)
                    {
                        if (e.ButtonState == MouseButtonState.Pressed)
                        {
                            if (!this.IsPressed)
                            {
                                this.SetIsPressed(true);
                            }
                        }
                        else
                        {
                            base.ReleaseMouseCapture();
                        }
                    }
                }
                if (this.ClickMode == ClickMode.Press)
                {
                    bool flag = true;
                    try
                    {
                        this.OnClick();
                        flag = false;
                    }
                    finally
                    {
                        if (flag)
                        {
                            this.SetIsPressed(false);
                            base.ReleaseMouseCapture();
                        }
                    }
                }
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (Interval > 0)
            {
                this.IsPressed = false;
                this.StopTimer();
            }
            if (this.ClickMode != ClickMode.Hover)
            {
                e.Handled = true;
                bool flag = (!this.IsSpaceKeyDown && this.IsPressed) && (this.ClickMode == ClickMode.Release);
                if (base.IsMouseCaptured && !this.IsSpaceKeyDown)
                {
                    base.ReleaseMouseCapture();
                }
                if (flag)
                {
                    this.OnClick();
                }
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (((this.ClickMode != ClickMode.Hover) && base.IsMouseCaptured) && ((Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed) && !this.IsSpaceKeyDown))
            {
                this.UpdateIsPressed();
                e.Handled = true;
            }
        }

        private void SetIsPressed(bool pressed)
        {
            if (pressed)
            {
                base.SetValue(IsPressedPropertyKey, pressed);
            }
            else
            {
                base.ClearValue(IsPressedPropertyKey);
            }
        }

        private void UpdateIsPressed()
        {
            Point position = Mouse.PrimaryDevice.GetPosition(this);
            if (((position.X >= 0.0) && (position.X <= base.ActualWidth)) && ((position.Y >= 0.0) && (position.Y <= base.ActualHeight)))
            {
                if (!this.IsPressed)
                {
                    this.SetIsPressed(true);
                }
            }
            else if (this.IsPressed)
            {
                this.SetIsPressed(false);
            }
        }

        [Category("Common")]
        public ClickMode ClickMode
        {
            get
            {
                return (ClickMode)base.GetValue(ClickModeProperty);
            }
            set
            {
                base.SetValue(ClickModeProperty, value);
            }
        }

        public ICommand Command
        {
            get
            {
                return (ICommand)base.GetValue(CommandProperty);
            }
            set
            {
                base.SetValue(CommandProperty, value);
            }
        }

        public object CommandParameter
        {
            get
            {
                return base.GetValue(CommandParameterProperty);
            }
            set
            {
                base.SetValue(CommandParameterProperty, value);
            }
        }

        public IInputElement CommandTarget
        {
            get
            {
                return (IInputElement)base.GetValue(CommandTargetProperty);
            }
            set
            {
                base.SetValue(CommandTargetProperty, value);
            }
        }


        private bool IsInMainFocusScope
        {
            get
            {
                Visual focusScope = FocusManager.GetFocusScope(this) as Visual;
                if (focusScope != null)
                {
                    return (VisualTreeHelper.GetParent(focusScope) == null);
                }
                return true;
            }
        }

        private bool IsSpaceKeyDown
        {
            get;
            set;

        }

        public bool IsPressed
        {
            get
            {
                return (bool)base.GetValue(IsPressedProperty);
            }
            protected set
            {
                base.SetValue(IsPressedPropertyKey, value);
            }
        }

        [Category("HMI")]
        public bool? IsChecked
        {
            get
            {
                return (bool?)base.GetValue(IsCheckedProperty);
            }
            set
            {
                base.SetValue(IsCheckedProperty, value);
            }
        }

        [Category("HMI")]
        public int Interval
        {
            get
            {
                return (int)base.GetValue(IntervalProperty);
            }
            set
            {
                base.SetValue(IntervalProperty, value);
            }
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.STATE:
                    var _funcRead = tagChanged as Func<bool>;
                    if (_funcRead != null)
                    {
                        return delegate { IsChecked = _funcRead(); };
                    }
                    else return null;
                case TagActions.ENABLE:
                    var _funcEnable = tagChanged as Func<bool>;
                    if (_funcEnable != null)
                    {
                        return delegate
                        {
                            this.IsEnabled = _funcEnable();
                            InvalidateVisual();
                        };
                    }
                    else return null;
                case TagActions.DISABLE:
                    var _funcDisable = tagChanged as Func<bool>;
                    if (_funcDisable != null)
                    {
                        return delegate
                        {
                            this.IsEnabled = !_funcDisable();
                            InvalidateVisual();
                        };
                    }
                    else return null;
            }
            return null;
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

        protected List<Func<object, int>> _funcWrites = new List<Func<object, int>>();
        protected List<Func<int>> _funcints = new List<Func<int>>();
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
    }

    public delegate void ButtonStateChangedEventHandler(object sender, StateChangedEventArgs e);

    public class StateChangedEventArgs : EventArgs
    {
        public StateChangedEventArgs(bool? oldStat, bool? newStat)
        {
            this.OldState = oldStat;
            this.NewState = newStat;
        }

        public bool? OldState;
        public bool? NewState;
    }
}
