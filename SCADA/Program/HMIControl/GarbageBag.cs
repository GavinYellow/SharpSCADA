using System.Windows;

namespace HMIControl
{

    public class GarbageBag : HMIControlBase
    {
        static GarbageBag()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GarbageBag), new FrameworkPropertyMetadata(typeof(GarbageBag)));
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[1]
                {  
                    new  LinkPosition(new Point(0.5,0),ConnectOrientation.Top),
                };
        }
    }
}
