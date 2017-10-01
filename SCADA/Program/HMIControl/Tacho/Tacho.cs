using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace HMIControl
{
    ///
    /// </summary>
    public class Tacho : RoundGuageBase, ITagReader
    {

        public static DependencyProperty NeedleDesignProperty = DependencyProperty.Register("NeedleDesign", typeof(NeedleDesign), typeof(Tacho),
           new FrameworkPropertyMetadata(NeedleDesign.Standard, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register("Layout", typeof(TachoLayout), typeof(Tacho),
           new FrameworkPropertyMetadata(TachoLayout.Inner, FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static readonly DependencyProperty TagReadTextProperty = DependencyProperty.Register("TagReadText", typeof(string), typeof(Tacho));

        static Tacho()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tacho), new FrameworkPropertyMetadata(typeof(Tacho)));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size size = base.ArrangeOverride(finalSize);
            scale.RenderTransformOrigin = new Point(0.5, 0.5);
            indict.RenderTransformOrigin = new Point(0.5, 0.5);
            double width = base.Width * 0.03;
            indict.Width = width * 2;
            indict.Height = base.Height * 0.9;
            //indict.Margin = new Thickness(width);
            this.scale.RingThickness = width;
            if (Layout == TachoLayout.Inner)
            {
                this.scale.MajorTicksOffset = 2 * width;
            }
            else
            {
                this.scale.MajorTicksOffset = -width;
            }
            return size;
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            if (indict != null)
            {
                this.indict.RenderTransform = new RotateTransform(scale.AngleFromValue(newValue));
                //this.indict.RenderTransform = new RotateTransform(indict.AngleFromValue(newValue));
            }
        }

        public NeedleDesign NeedleDesign
        {
            get
            {
                return (NeedleDesign)base.GetValue(NeedleDesignProperty);
            }
            set
            {
                base.SetValue(NeedleDesignProperty, value);
            }
        }

        public TachoLayout Layout
        {
            get
            {
                return (TachoLayout)base.GetValue(LayoutProperty);
            }
            set
            {
                base.SetValue(LayoutProperty, value);
            }
        }

        /// <summary>
        /// Tag Text
        /// </summary>
        public string TagReadText
        {
            get
            {
                return (string)base.GetValue(TagReadTextProperty);
            }
            set
            {
                base.SetValue(TagReadTextProperty, value);
            }
        }

        public string Node
        {
            get { return this.Name; }
        }

        public string[] GetActions()
        {
            return new string[] { TagActions.PV };
        }

        public Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.PV:
                    var _funcPV = tagChanged as Func<float>;
                    if (_funcPV != null)
                    {
                        return delegate
                         {
                             this.Value = _funcPV();
                         };
                    }
                    else return null;
            }
            return null;
        }

        public IList<ITagLink> Children
        {
            get { return null; }
        }
    }

    public enum NeedleDesign
    {
        Standard,
        Classic,
        Shape,
        Thin
    }

    public enum TachoLayout
    {
        Inner,
        Outer
    }
}
