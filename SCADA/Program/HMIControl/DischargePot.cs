using System.Windows;
using System.ComponentModel;

namespace HMIControl
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:HMIControl"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:HMIControl;assembly=HMIControl"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:DischargePot/>
    ///
    /// </summary>
    public class DischargePot : HMIControlBase
    {
        static DischargePot()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DischargePot), new FrameworkPropertyMetadata(typeof(DischargePot)));
        }

        public static DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen",
           typeof(bool),
           typeof(DischargePot),
           new PropertyMetadata(new PropertyChangedCallback(ValueChangedCallback)));

        #region HMI属性
        [Category("HMI")]
        public bool IsOpen
        {
            set
            {
                SetValue(IsOpenProperty, value);
            }
            get
            {
                return (bool)GetValue(IsOpenProperty);
            }
        }

        #endregion
        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            VisualStateManager.GoToState(obj as DischargePot, (bool)args.NewValue ? "Openned" : "Closed", true);
        }

    }
}
