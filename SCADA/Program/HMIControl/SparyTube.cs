using System;
using System.ComponentModel;
using System.Windows;

namespace HMIControl
{
    public class SparyTube : HMIControlBase
    {
        static SparyTube()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SparyTube), new FrameworkPropertyMetadata(typeof(SparyTube)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var MagnetValue1 = base.GetTemplateChild("MagnetValue1") as MagnetValve;
            var MagnetValue2 = base.GetTemplateChild("MagnetValue2") as MagnetValve;
            children = new ITagLink[] { MagnetValue1, MagnetValue2 };
        }

    }
}
