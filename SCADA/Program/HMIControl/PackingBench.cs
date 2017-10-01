using System.Windows;

namespace HMIControl
{

    public class PackingBench : HMIControlBase
    {
        static PackingBench()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PackingBench), new FrameworkPropertyMetadata(typeof(PackingBench)));
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[1]
                {  
                    new  LinkPosition(new Point(0.8,0),ConnectOrientation.Top),
                };
        }
    }
}
