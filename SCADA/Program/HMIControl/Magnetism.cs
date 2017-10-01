using System.Windows;

namespace HMIControl
{
    public class Magnetism : HMIControlBase
    {
        static Magnetism()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Magnetism), new FrameworkPropertyMetadata(typeof(Magnetism)));
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.5,1),ConnectOrientation.Bottom),
                };
        }
    }
}
