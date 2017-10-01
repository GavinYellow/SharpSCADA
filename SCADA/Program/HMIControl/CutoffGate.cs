using System.Windows;
using System;
using System.ComponentModel;


namespace HMIControl
{
    /// <summary>
    ///
    ///     <MyNamespace:CutoffGate/>
    ///
    /// </summary>
    public class CutoffGate : HMIControlBase
    {
        public static DependencyProperty OpenProperty = 
            DependencyProperty.Register("Open", typeof(bool), typeof(CutoffGate),
              new PropertyMetadata(new PropertyChangedCallback(OnValueChanged)));

        protected override void UpdateState()
        {
            VisualStateManager.GoToState(this, Open ? "Open" : "Closed", true);
        }

        #region HMI属性
        [Category("HMI")]
        public bool Open
        {
            set
            {
                SetValue(OpenProperty, value);
            }
            get
            {
                return (bool)GetValue(OpenProperty);
            }
        }

        #endregion        

        static CutoffGate()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CutoffGate), new FrameworkPropertyMetadata(typeof(CutoffGate)));
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.ON };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.ON:
                    var _funcRun = tagChanged as Func<bool>;
                    if (_funcRun != null)
                    {
                        return delegate { Open = _funcRun(); };
                    }
                    else return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }
}
