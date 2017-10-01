using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HMIControl
{
    public static class HMIHelper
    {
        public static void BindToTemplatedParent(this FrameworkElement element, DependencyProperty target, DependencyProperty source)
        {
            Binding binding = new Binding
            {
                RelativeSource = RelativeSource.TemplatedParent,
                Path = new PropertyPath(source)
            };
            element.SetBinding(target, binding);
        }

        public static Color AddColorDelta(this Color c, int delta)
        {
            int r = c.R + delta;
            int g = c.G + delta;
            int b = c.B + delta;
            if (r < 0)
            {
                r = 0;
            }
            else if (r > 0xff)
            {
                r = 0xff;
            }
            if (g < 0)
            {
                g = 0;
            }
            else if (g > 0xff)
            {
                g = 0xff;
            }
            if (b < 0)
            {
                b = 0;
            }
            else if (b > 0xff)
            {
                b = 0xff;
            }
            return Color.FromArgb(c.A, (byte)r, (byte)g, (byte)b);
        }

        public static LinearGradientBrush CreateLinearTwoGradientsBrush(this Color color, double rotateAngle, int deltaA, int deltaB)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0.0, 0.5),
                EndPoint = new Point(1.0, 0.5),
                GradientStops = new GradientStopCollection{
                new GradientStop(AddColorDelta(color, deltaA), 0.0), 
                new GradientStop(AddColorDelta(color, deltaB), 1.0) }
            };
            if (rotateAngle != 0.0)
            {
                brush.RelativeTransform = new RotateTransform(rotateAngle, 0.5, 0.5);
            }
            return brush;
        }

        public static LinearGradientBrush CreateLinearGradientsBrush(this Color color, double offset)
        {
            return new LinearGradientBrush
            {
                GradientStops = { new GradientStop(color, 0) ,
                                  new GradientStop(color.AddColorDelta(-80), offset) },
                EndPoint = new Point(0.707, 0.707)
            };
        }

        public static LinearGradientBrush CreateLinearGradientsBrush(this Color color, double rotateAngle, double offset, Color color2)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0.0, 0.5),
                EndPoint = new Point(1.0, 0.5),
                GradientStops = new GradientStopCollection{
                new GradientStop(color2, 0.0), 
                new GradientStop(color2, offset), 
                new GradientStop(color, 1.0) }
            };
            if (rotateAngle != 0.0)
            {
                brush.RelativeTransform = new RotateTransform(rotateAngle, 0.5, 0.5);
            }
            return brush;
        }

        public static LinearGradientBrush CreateLinearThreeGradientsBrush(this Color color, double rotateAngle, int deltaA, int deltaB)
        {
            Color color1 = AddColorDelta(color, deltaA);
            Color color2 = AddColorDelta(color, deltaB);
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new Point(0.0, 0.5),
                EndPoint = new Point(1.0, 0.5),
                GradientStops = new GradientStopCollection { new GradientStop(color1, 0.0), new GradientStop(color2, 0.5), new GradientStop(color1, 1.0) }
            };
            if (rotateAngle != 0.0)
            {
                brush.RelativeTransform = new RotateTransform(rotateAngle, 0.5, 0.5);
            }
            return brush;
        }

        public static RadialGradientBrush CreateRadialTwoGradientBrush(this Color color, Transform relativeTransform, int deltaA)
        {
            RadialGradientBrush brush = new RadialGradientBrush(new GradientStopCollection { new GradientStop(color.AddColorDelta(deltaA), 0.0), new GradientStop(color, 1.0) });
            brush.RelativeTransform = relativeTransform;
            return brush;
        }

        public static RadialGradientBrush CreateRadialTwoGradientBrush(this Color color, Transform relativeTransform, int deltaA, int deltaB)
        {
            RadialGradientBrush brush = new RadialGradientBrush(new GradientStopCollection { new GradientStop(color.AddColorDelta(deltaA), 0.0), new GradientStop(color.AddColorDelta(deltaB), 1.0) });
            brush.RelativeTransform = relativeTransform;
            return brush;
        }

        public static RadialGradientBrush CreateRadialThreeGradientBrush(this Color color, Transform relativeTransform, int deltaA)
        {
            RadialGradientBrush brush = new RadialGradientBrush(new GradientStopCollection { new GradientStop(color, 0.0), new GradientStop(color.AddColorDelta(deltaA), 0.5), new GradientStop(color, 1.0) });
            brush.RelativeTransform = relativeTransform;
            return brush;
        }

        public static RadialGradientBrush CreateRadialThreeGradientBrush(this Color color, Transform relativeTransform, Color color2)
        {
            RadialGradientBrush brush = new RadialGradientBrush(new GradientStopCollection { new GradientStop(color2, 0.0), new GradientStop(color, 0.5), new GradientStop(color2, 1.0) });
            brush.RelativeTransform = relativeTransform;
            return brush;
        }

        public static int GetLength(this string words)
        {
            int len = 0;
            var encodn = Encoding.GetEncoding(0);
            for (int i = 0; i < words.Length; i++)
            {
                byte[] sarr = encodn.GetBytes(words.Substring(i, 1));
                len += sarr.Length;
            }
            return len;
        }

        public static Dictionary<string, string> GetListFromText(this string text)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(text)) return dict;
            string[] strs = text.Split('\\');
            foreach (string str in strs)
            {
                int index = str.IndexOf(':');
                if (index >= 0) dict.Add(str.Substring(0, index), str.Substring(index + 1));
            }
            return dict;
        }
    }
}
