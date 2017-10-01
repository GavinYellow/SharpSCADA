using System.Windows;

namespace HMIControl
{
    public class Dryer : HMIControlBase
    {
        static Dryer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Dryer), new FrameworkPropertyMetadata(typeof(Dryer)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var feed_motor = base.GetTemplateChild("feed_motor") as Motor;
            var fan1_motor = base.GetTemplateChild("fan1_motor") as Motor;
            var fan2_motor = base.GetTemplateChild("fan2_motor") as Motor;
            var fan3_motor = base.GetTemplateChild("fan3_motor") as Motor;
            var fan4_motor = base.GetTemplateChild("fan4_motor") as Motor;
            var fan5_motor = base.GetTemplateChild("fan5_motor") as Motor;
            var fan6_motor = base.GetTemplateChild("fan6_motor") as Motor;
            var fan7_motor = base.GetTemplateChild("fan7_motor") as Motor;
            var fan8_motor = base.GetTemplateChild("fan8_motor") as Motor;
            var conveyor1_motor = base.GetTemplateChild("conveyor1_motor") as Motor;
            var conveyor2_motor = base.GetTemplateChild("conveyor2_motor") as Motor;
            var conveyor3_motor = base.GetTemplateChild("conveyor3_motor") as Motor;
            var conveyor4_motor = base.GetTemplateChild("conveyor4_motor") as Motor;
            var deduster1_motor = base.GetTemplateChild("deduster1_motor") as Motor;
            var deduster2_motor = base.GetTemplateChild("deduster2_motor") as Motor;
            children = new ITagLink[] { feed_motor,  fan1_motor, fan2_motor, fan3_motor, fan4_motor, fan5_motor, fan6_motor, fan7_motor, 
                    fan8_motor, conveyor1_motor, conveyor2_motor, conveyor3_motor, conveyor4_motor,deduster1_motor, deduster2_motor };
        }

        public override LinkPosition[] GetLinkPositions()
        {
            return new LinkPosition[2]
                {  
                    new  LinkPosition(new Point(0.025,0),ConnectOrientation.Top),
                    new  LinkPosition(new Point(0.202,1),ConnectOrientation.Bottom),
                };
        }
    }
}
