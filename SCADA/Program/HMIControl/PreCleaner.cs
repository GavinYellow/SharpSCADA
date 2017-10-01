using System.Windows;

namespace HMIControl
{
    [Startable]
    public class PreCleaner : HMIControlBase
    {
        static PreCleaner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PreCleaner), new FrameworkPropertyMetadata(typeof(PreCleaner)));
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0.2,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.55,1),ConnectOrientation.Bottom),
                };
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Motor motor = Template.FindName("motor", this) as Motor;
            children = new ITagLink[] { motor };
        }
    }
}
