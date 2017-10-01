using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace HMIControl
{
    public class TextAdorner : Adorner
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextAdorner),
         new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));
        const double FONTSIZE = 12;
        static readonly FontFamily FONTFAMILY = new FontFamily("Microsoft YaHei");
        static readonly SolidColorBrush FOREGROUND = Brushes.DarkGoldenrod;


        public string Text
        {
            get
            {
                return (string)base.GetValue(TextProperty);
            }
            set
            {
                base.SetValue(TextProperty, value);
            }
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            MatrixTransform matrix = transform as MatrixTransform;
            if (matrix != null)
            {
                var m = Matrix.Identity;
                var m1 = matrix.Value;
                var size = this.DesiredSize;
                m.OffsetX = m1.OffsetX + size.Width * (m1.M11 - 1) / 2;
                m.OffsetY = m1.M22 < 0 ? m1.OffsetY + size.Height * m1.M22 : m1.OffsetY;
                //m.M11 = Math.Abs(m1.M11);
                //m.M22 = Math.Abs(m1.M22);
                //m.M12 = m1.M12;
                //m.M21 = m1.M21;
                return new MatrixTransform(m);
            }
            else return transform;
        }

        public TextAdorner(UIElement adornedElement)
            : base(adornedElement)
        {

        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Control element = this.AdornedElement as Control;
            FormattedText formattedText = new FormattedText(Text, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                new Typeface(FONTFAMILY, FontStyles.Normal, FontWeights.Bold, new FontStretch()), FONTSIZE, FOREGROUND);
            drawingContext.DrawText(formattedText, new Point((ActualWidth - formattedText.Width) / 2, -FONTSIZE - 5));
            //
        }
    }
}
