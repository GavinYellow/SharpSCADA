using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace HMIControl
{
    [Startable]
    public class HMILable : HMIControlBase
    {
        public static DependencyProperty BorderStyleProperty = DependencyProperty.Register("BorderStyle", typeof(BorderStyle), typeof(HMILable),
             new FrameworkPropertyMetadata(BorderStyle.None, FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(HMILable),
             new FrameworkPropertyMetadata(TextAlignment.Center, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HMILable),
          new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register("Unit", typeof(string), typeof(HMILable),
    new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty StringFormatProperty = DependencyProperty.Register("StringFormat", typeof(string), typeof(HMILable));

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.TEXT };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);
            double width = this.ActualWidth;
            double height = this.ActualHeight;
            double bevel = height * 0.1;
            Color color = Colors.Black;
            if (this.Background is SolidColorBrush)
                color = (this.Background as SolidColorBrush).Color;
            Pen pen = new Pen(base.BorderBrush, base.BorderThickness.Left);
            switch (this.BorderStyle)
            {
                case BorderStyle.Fixed3D:
                    Brush brush1 = color.CreateLinearTwoGradientsBrush(90.0, 80, -80);
                    Brush brush2 = color.CreateLinearTwoGradientsBrush(90.0, -80, 80);
                    drawingContext.DrawRectangle(brush1, null, new Rect(0, 0, width, height));
                    drawingContext.DrawRectangle(brush2, null, new Rect(bevel, bevel, width - 2 * bevel, height - 2 * bevel));
                    break;
                case BorderStyle.FixedSingle:
                    drawingContext.DrawRectangle(this.Background, pen, new Rect(0, 0, width, height));
                    break;
                //default:
                //    drawingContext.DrawRectangle(this.Background, pen, new Rect(0, 0, width, height));
                //    break;
            }
            string txt = this.Text;
            if (!string.IsNullOrEmpty(txt))
            {
                FormattedText formattedText = new FormattedText(txt, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface(base.FontFamily, base.FontStyle, base.FontWeight, base.FontStretch), base.FontSize, base.Foreground);
                Point pt = new Point((this.TextAlignment == TextAlignment.Center ? (width - formattedText.Width) * 0.5 :
                   this.TextAlignment == TextAlignment.Left ? bevel : width - bevel - formattedText.Width), (height - formattedText.Height) * 0.5);
                drawingContext.DrawText(formattedText, pt);
            }
        }

        [Category("HMI")]
        public BorderStyle BorderStyle
        {
            get
            {
                return (BorderStyle)base.GetValue(BorderStyleProperty);
            }
            set
            {
                base.SetValue(BorderStyleProperty, value);
            }
        }

        [Category("Text")]
        public TextAlignment TextAlignment
        {
            get
            {
                return (TextAlignment)base.GetValue(TextAlignmentProperty);
            }
            set
            {
                base.SetValue(TextAlignmentProperty, value);
            }
        }

        [Category("HMI")]
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

        [Category("HMI")]
        public string Unit
        {
            get
            {
                return (string)base.GetValue(UnitProperty);
            }
            set
            {
                base.SetValue(UnitProperty, value);
            }
        }

        [Category("HMI")]
        public string StringFormat
        {
            get
            {
                return (string)base.GetValue(StringFormatProperty);
            }
            set
            {
                base.SetValue(StringFormatProperty, value);
            }
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            var unit = " " + Unit;
            switch (key)
            {
                case TagActions.TEXT:
                    var _funcText = tagChanged as Func<string>;
                    if (_funcText != null)
                    {
                        return delegate { this.Text = _funcText() + unit; };
                    }
                    else
                    {
                        var _funcFloat = tagChanged as Func<float>;
                        if (_funcFloat != null)
                        {
                            return delegate
                            {
                                var format = this.StringFormat;
                                this.Text = string.IsNullOrEmpty(format) ? _funcFloat().ToString() : _funcFloat().ToString(format) + unit;
                            };
                        }
                        else
                        {
                            var _funcint = tagChanged as Func<int>;
                            {
                                if (_funcint != null)
                                    return delegate { this.Text = _funcint().ToString() + unit; };
                                else
                                {
                                    var _funcbool = tagChanged as Func<bool>;
                                    if (_funcbool != null)
                                    {
                                        return delegate { this.Text = _funcbool() ? "1" : "0" + unit; };
                                    }
                                }
                            }

                        }
                    }
                    return null;
            }
            return base.SetTagReader(key, tagChanged);
        }
    }

    public class HMIText : HMIControlBase, ITagWriter
    {
        public static DependencyProperty BorderStyleProperty = DependencyProperty.Register("BorderStyle", typeof(BorderStyle), typeof(HMIText),
             new FrameworkPropertyMetadata(BorderStyle.None, FrameworkPropertyMetadataOptions.AffectsRender));

        public static DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(HMIText),
             new FrameworkPropertyMetadata(TextAlignment.Center, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HMIText),
          new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TagWriteTextProperty = DependencyProperty.Register("TagWriteText", typeof(string), typeof(HMIText));

        public static readonly DependencyProperty IsPulseProperty = DependencyProperty.Register("IsPulse", typeof(bool), typeof(HMIText), new FrameworkPropertyMetadata(false));


        public static DependencyProperty StringFormatProperty = DependencyProperty.Register("StringFormat", typeof(string), typeof(HMIText));

        public override string[] GetActions()
        {
            return new string[] { TagActions.VISIBLE, TagActions.CAPTION, TagActions.DEVICENAME, TagActions.TEXT };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //base.OnRender(drawingContext);
            double width = this.ActualWidth;
            double height = this.ActualHeight;
            double bevel = height * 0.1;
            Color color = Colors.Black;
            if (this.Background is SolidColorBrush)
                color = (this.Background as SolidColorBrush).Color;
            Pen pen = new Pen(base.BorderBrush, base.BorderThickness.Left);
            switch (this.BorderStyle)
            {
                case BorderStyle.Fixed3D:
                    Brush brush1 = color.CreateLinearTwoGradientsBrush(90.0, 80, -80);
                    Brush brush2 = color.CreateLinearTwoGradientsBrush(90.0, -80, 80);
                    drawingContext.DrawRectangle(brush1, null, new Rect(0, 0, width, height));
                    drawingContext.DrawRectangle(brush2, null, new Rect(bevel, bevel, width - 2 * bevel, height - 2 * bevel));
                    break;
                case BorderStyle.FixedSingle:
                    drawingContext.DrawRectangle(this.Background, pen, new Rect(0, 0, width, height));
                    break;
                default:
                    drawingContext.DrawRectangle(this.Background, pen, new Rect(0, 0, width, height));
                    break;
            }
            string txt = this.Text;
            if (!string.IsNullOrEmpty(txt))
            {
                FormattedText formattedText = new FormattedText(txt, System.Globalization.CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface(base.FontFamily, base.FontStyle, base.FontWeight, base.FontStretch), base.FontSize, base.Foreground);
                Point pt = new Point((this.TextAlignment == TextAlignment.Center ? (width - formattedText.Width) * 0.5 :
                   this.TextAlignment == TextAlignment.Left ? bevel : width - bevel - formattedText.Width), (height - formattedText.Height) * 0.5);
                drawingContext.DrawText(formattedText, pt);
            }
        }

        [Category("HMI")]
        public BorderStyle BorderStyle
        {
            get
            {
                return (BorderStyle)base.GetValue(BorderStyleProperty);
            }
            set
            {
                base.SetValue(BorderStyleProperty, value);
            }
        }

        [Category("Text")]
        public TextAlignment TextAlignment
        {
            get
            {
                return (TextAlignment)base.GetValue(TextAlignmentProperty);
            }
            set
            {
                base.SetValue(TextAlignmentProperty, value);
            }
        }

        [Category("HMI")]
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

        [Category("HMI")]
        public string StringFormat
        {
            get
            {
                return (string)base.GetValue(StringFormatProperty);
            }
            set
            {
                base.SetValue(StringFormatProperty, value);
            }
        }

        public override Action SetTagReader(string key, Delegate tagChanged)
        {
            switch (key)
            {
                case TagActions.TEXT:
                    var _funcText = tagChanged as Func<string>;
                    if (_funcText != null)
                    {
                        return delegate { this.Text = _funcText(); };
                    }
                    else
                    {
                        var _funcFloat = tagChanged as Func<float>;
                        if (_funcFloat != null)
                        {
                            return delegate
                            {
                                var format = this.StringFormat;
                                this.Text = string.IsNullOrEmpty(format) ? _funcFloat().ToString() : _funcFloat().ToString(format);
                            };
                        }
                        else
                        {
                            var _funcint = tagChanged as Func<int>;
                            {
                                if (_funcint != null)
                                    return delegate { this.Text = _funcint().ToString(); };
                                else
                                {
                                    var _funcbool = tagChanged as Func<bool>;
                                    if (_funcbool != null)
                                    {
                                        return delegate { this.Text = _funcbool() ? "1" : "0"; };
                                    }
                                }
                            }

                        }
                    }
                    return null;
            }
            return base.SetTagReader(key, tagChanged);
        }

        [Category("Common")]
        public string TagWriteText
        {
            get
            {
                return (string)base.GetValue(TagWriteTextProperty);
            }
            set
            {
                base.SetValue(TagWriteTextProperty, value);
            }
        }

        [Category("HMI")]
        public bool IsPulse
        {
            get
            {
                return (bool)base.GetValue(IsPulseProperty);
            }
            set
            {
                base.SetValue(IsPulseProperty, value);
            }
        }
        protected List<Func<object, int>> _funcWrites = new List<Func<object, int>>();
        public bool SetTagWriter(IEnumerable<Delegate> tagWriter)
        {
            bool ret = true;
            _funcWrites.Clear();

            foreach (var item in tagWriter)
            {
                Func<object, int> _funcWrite = item as Func<object, int>;

                if (_funcWrite != null)
                    _funcWrites.Add(_funcWrite);
                else
                {
                    ret = false;
                    break;
                }
            }
            return ret;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Return && _funcWrites.Count>0 && !string.IsNullOrEmpty(Text))
            {
                foreach (var func in _funcWrites)
                {
                    func(Text);
                }
            }
            base.OnKeyDown(e);
        }
    }

    public enum BorderStyle
    {
        Fixed3D,
        FixedSingle,
        None
    }
}