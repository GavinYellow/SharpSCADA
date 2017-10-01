using Microsoft.Windows.Design.Features;
using Microsoft.Windows.Design.Metadata;
using HMIControl;


[assembly: ProvideMetadata(typeof(HMIControl.VisualStudio.Design.Metadata))]
namespace HMIControl.VisualStudio.Design
{
    internal class Metadata : IProvideAttributeTable
    {
        // Accessed by the designer to register any design-time metadata.
        public AttributeTable AttributeTable
        {
            get
            {
                AttributeTableBuilder builder = new AttributeTableBuilder();
                //InitializeAttributes(builder);
                // Add the adorner provider to the design-time metadata.
                builder.AddCustomAttributes(
                    typeof(LinkableControl),
                    new FeatureAttribute(typeof(ControlAdornerProvider))
                    //new FeatureAttribute(typeof(TagComplexContextMenuProvider))
                    );
                builder.AddCustomAttributes(
                  typeof(HMIControlBase),
                  //new FeatureAttribute(typeof(LinkLineAdornerProvider)),
                  new FeatureAttribute(typeof(TagComplexContextMenuProvider)));
                builder.AddCustomAttributes(
                    typeof(LinkLine),
                    new FeatureAttribute(typeof(LinkLineAdornerProvider)),
                    new FeatureAttribute(typeof(TagComplexContextMenuProvider)));
                builder.AddCustomAttributes(
                    typeof(ButtonBase),
                    new FeatureAttribute(typeof(TagWriterContextMenuProvider)));
                builder.AddCustomAttributes(
                  typeof(HMIButton),
                  new FeatureAttribute(typeof(TagWindowContextMenuProvider)),
                  new FeatureAttribute(typeof(TagComplexContextMenuProvider)),
                  new FeatureAttribute(typeof(TagWriterContextMenuProvider)));
                builder.AddCustomAttributes(
                  typeof(FromTo),
                  new FeatureAttribute(typeof(TagWindowContextMenuProvider)));
                return builder.CreateTable();
            }
        }
    }
}
