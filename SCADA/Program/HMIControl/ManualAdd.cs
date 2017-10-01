using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace HMIControl
{
	/// <summary>
	/// ManualAdd.xaml 的交互逻辑
	/// </summary>
    public  class ManualAdd : HMIControlBase
	{
        static ManualAdd()
		{
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ManualAdd), new FrameworkPropertyMetadata(typeof(ManualAdd)));
		}

        public static DependencyProperty IsInWorkProperty = DependencyProperty.Register("IsInWork", typeof(bool), typeof(ManualAdd),
           new PropertyMetadata(OnIsInWorkChanged));

        public static DependencyProperty IsNeedAddProperty = DependencyProperty.Register("IsNeedAdd", typeof(bool), typeof(ManualAdd),
           new PropertyMetadata(OnIsNeedAddChanged));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        #region HMI属性
        [Category("HMI")]
        public bool IsInWork
        {
            set
            {
                SetValue(IsInWorkProperty, value);
            }
            get
            {
                return (bool)GetValue(IsInWorkProperty);
            }
        }

        [Category("HMI")]
        public bool IsNeedAdd
        {
            set
            {
                SetValue(IsNeedAddProperty, value);
            }
            get
            {
                return (bool)GetValue(IsNeedAddProperty);
            }
        }

        #endregion

        private static void OnIsInWorkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as ManualAdd, (bool)args.NewValue ? "Running" : "Base", true);
        }

        private static void OnIsNeedAddChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as ManualAdd, (bool)args.NewValue ? "Alarm" : "Base", true);
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[1]
                {  
                    new  LinkPosition(new Point(0.5,1),ConnectOrientation.Bottom),
                };
        }

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.RUN,  TagActions.RAWNAME,TagActions.WARN };
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.RUN:
                    var _funcrun = tagChanged as Func<bool>;
                    if (_funcrun != null)
                    {
                        return delegate { IsInWork = _funcrun(); };
                    }
                    else return null;
                case TagActions.WARN:
                    var _funcwarn = tagChanged as Func<bool>;
                    if (_funcwarn != null)
                    {
                        return delegate { IsNeedAdd = _funcwarn(); };
                    }
                    else return null;    
            }
            return base.SetTagReader(key, tagChanged);
        }
	}
}