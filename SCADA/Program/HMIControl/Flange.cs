using System.Windows;

namespace HMIControl
{

    public class Flange : HMIControlBase
    {
        static Flange()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Flange), new FrameworkPropertyMetadata(typeof(Flange)));
        }
    }
}
