using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace HMIControl
{
    public class ControlBase : HMIControlBase
    {
        public static DependencyProperty ControlBrushProperty = DependencyProperty.Register("ControlBrush", typeof(SolidColorBrush), typeof(ControlBase),
           new FrameworkPropertyMetadata(Brushes.Silver, FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty ActiveColorProperty = DependencyProperty.Register("ActiveColor", typeof(Color), typeof(ControlBase),
          new FrameworkPropertyMetadata(Colors.Green, FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty RenderModelProperty = DependencyProperty.Register("RenderModel", typeof(RenderModel), typeof(ControlBase),
     new FrameworkPropertyMetadata(RenderModel.Complex, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(ControlBase),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));


        [Category("Common")]
        public double StrokeThickness
        {
            get
            {
                return (double)base.GetValue(StrokeThicknessProperty);
            }
            set
            {
                base.SetValue(StrokeThicknessProperty, value);
            }
        }

        [Category("Brushes")]
        public SolidColorBrush ControlBrush
        {
            get
            {
                return (SolidColorBrush)base.GetValue(ControlBrushProperty);
            }
            set
            {
                base.SetValue(ControlBrushProperty, value);
            }
        }

        public Color ActiveColor
        {
            get
            {
                return (Color)base.GetValue(ActiveColorProperty);
            }
            set
            {
                base.SetValue(ActiveColorProperty, value);
            }
        }

        [Category("Common")]
        public RenderModel RenderModel
        {
            get
            {
                return (RenderModel)base.GetValue(RenderModelProperty);
            }
            set
            {
                base.SetValue(RenderModelProperty, value);
            }
        }
    }

    public enum RenderModel
    {
        Simple,
        Complex
    }

    [Flags]
    public enum States
    {
        Stop = 0,
        Active = 1,
        Alarm = 2,
        Run = 3,
        Warn = 4,
        Error = 5,
        Misc = 6
    }
}

