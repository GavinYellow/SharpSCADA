using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace CoreTest
{
    public sealed class ConfigCache
    {
        public const string PATH = @"C:\DataConfig\client.xml";

        static ConfigCache()
        {
            _windows = new Dictionary<string, Rect>();
            string path = PATH;
            if (File.Exists(path))
            {
                try
                {
                    using (var reader = XmlTextReader.Create(path))
                    {
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "Server":
                                        {
                                            if (reader.MoveToAttribute("AlarmLimit"))
                                                int.TryParse(reader.Value, out _alarmLimit);
                                            if (reader.MoveToAttribute("Cycle"))
                                                int.TryParse(reader.Value, out _cycle);
                                        }
                                        break;
                                    case "RecipeDisplay":
                                        {
                                            if (reader.MoveToAttribute("Row"))
                                                int.TryParse(reader.Value, out _row);
                                            if (reader.MoveToAttribute("RowByCount"))
                                                int.TryParse(reader.Value, out _rowbycount);
                                            if (reader.MoveToAttribute("RowHeight"))
                                                int.TryParse(reader.Value, out _rowheight);
                                            if (reader.MoveToAttribute("ColWidth"))
                                                int.TryParse(reader.Value, out _colwidth);
                                        }
                                        break;
                                    case "DataDisplay":
                                        {
                                            if (reader.MoveToAttribute("HdaLargeTick"))
                                                int.TryParse(reader.Value, out _hda_l_tick);
                                            if (reader.MoveToAttribute("HdaSmallTick"))
                                                int.TryParse(reader.Value, out _hda_s_tick);
                                            if (reader.MoveToAttribute("RtWaitTick"))
                                                int.TryParse(reader.Value, out _rtWaitTick);
                                            if (reader.MoveToAttribute("RtCap"))
                                                int.TryParse(reader.Value, out _rtCap);
                                        }
                                        break;
                                    case "WindowSet":
                                        {
                                            if (reader.MoveToAttribute("Background"))
                                                _background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(reader.Value));
                                        }
                                        break;
                                    case "Window":
                                        {
                                            string name = null;
                                            double left, top, width, height; left = top = width = height = 0;
                                            if (reader.MoveToAttribute("Name"))
                                                name = reader.Value;
                                            if (reader.MoveToAttribute("Left"))
                                                double.TryParse(reader.Value, out left);
                                            if (reader.MoveToAttribute("Top"))
                                                double.TryParse(reader.Value, out top);
                                            if (reader.MoveToAttribute("Width"))
                                                double.TryParse(reader.Value, out width);
                                            if (reader.MoveToAttribute("Height"))
                                                double.TryParse(reader.Value, out height);
                                            if (!string.IsNullOrEmpty(name))
                                            {
                                                if (double.IsInfinity(left) || double.IsInfinity(top) || double.IsInfinity(width) || double.IsInfinity(height))
                                                    _windows.Add(name, new Rect());
                                                else
                                                    _windows.Add(name, new Rect(left, top, width, height));
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    App.AddErrorLog(err);
                }
            }
        }

        public static void SaveConfig()
        {
            try
            {
                using (var writer = XmlTextWriter.Create(PATH))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Client");
                    writer.WriteStartElement("Server");
                    if (ConfigCache.AlarmLimit > 0)
                        writer.WriteAttributeString("AlarmLimit", ConfigCache.AlarmLimit.ToString());
                    if (ConfigCache.Cycle > 0)
                        writer.WriteAttributeString("Cycle", ConfigCache.Cycle.ToString());
                    writer.WriteEndElement();
                    writer.WriteStartElement("RecipeDisplay");
                    if (ConfigCache.Row > 0)
                        writer.WriteAttributeString("Row", ConfigCache.Row.ToString());
                    if (ConfigCache.RowByCount > 0)
                        writer.WriteAttributeString("RowByCount", ConfigCache.RowByCount.ToString());
                    if (ConfigCache.RowHeight > 0)
                        writer.WriteAttributeString("RowHeight", ConfigCache.RowHeight.ToString());
                    if (ConfigCache.ColWidth > 0)
                        writer.WriteAttributeString("ColWidth", ConfigCache.ColWidth.ToString());
                    writer.WriteEndElement();
                    writer.WriteStartElement("DataDisplay");
                    if (ConfigCache.HdaLargeTick > 0)
                        writer.WriteAttributeString("HdaLargeTick", ConfigCache.HdaLargeTick.ToString());
                    if (ConfigCache.HdaLargeTick > 0)
                        writer.WriteAttributeString("HdaSmallTick", ConfigCache.HdaSmallTick.ToString());
                    if (ConfigCache.HdaLargeTick > 0)
                        writer.WriteAttributeString("RtWaitTick", ConfigCache.RtWaitTick.ToString());
                    if (ConfigCache.HdaLargeTick > 0)
                        writer.WriteAttributeString("RtCap", ConfigCache.RtCap.ToString());
                    writer.WriteEndElement();
                    writer.WriteStartElement("WindowSet");
                    var background = ConfigCache.Background;
                    if (background != null)
                        writer.WriteAttributeString("Background", background.ToString());
                    foreach (var item in ConfigCache.Windows)
                    {
                        writer.WriteStartElement("Window");
                        Rect rec = item.Value;
                        if (double.IsInfinity(rec.Width) || double.IsInfinity(rec.Height) || double.IsInfinity(rec.Top) || double.IsInfinity(rec.Left))
                            rec = new Rect();
                        writer.WriteAttributeString("Name", item.Key);
                        writer.WriteAttributeString("Left", rec.Left.ToString());
                        writer.WriteAttributeString("Top", rec.Top.ToString());
                        writer.WriteAttributeString("Width", rec.Width.ToString());
                        writer.WriteAttributeString("Height", rec.Height.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
            }
            catch (Exception err)
            {
                App.AddErrorLog(err);
            }
        }

        private static SolidColorBrush _background = Brushes.DarkSlateGray;
        public static SolidColorBrush Background
        {
            get
            {
                return _background;
            }
            set
            {
                _background = value;
            }
        }

        private static int _hda_l_tick = 60;
        public static int HdaLargeTick
        {
            get
            {
                return _hda_l_tick;
            }
        }

        private static int _hda_s_tick = 10;
        public static int HdaSmallTick
        {
            get
            {
                return _hda_s_tick;
            }
        }

        private static int _rtWaitTick = 5000;
        public static int RtWaitTick
        {
            get
            {
                return _rtWaitTick;
            }
        }

        private static int _rtCap = 200;
        public static int RtCap
        {
            get
            {
                return _rtCap;
            }
        }

        private static int _alarmLimit = 1000;
        public static int AlarmLimit
        {
            get
            {
                return _alarmLimit;
            }
        }

        private static int _cycle = 60000;
        public static int Cycle
        {
            get
            {
                return _cycle;
            }
        }

        private static int _row = 5;
        public static int Row
        {
            get
            {
                return _row;
            }
        }

        private static int _rowbycount = 15;
        public static int RowByCount
        {
            get
            {
                return _rowbycount;
            }
        }

        private static int _rowheight = 40;
        public static int RowHeight
        {
            get
            {
                return _rowheight;
            }
        }

        private static int _colwidth = 150;
        public static int ColWidth
        {
            get
            {
                return _colwidth;
            }
        }

        private static Dictionary<string, Rect> _windows;
        public static Dictionary<string, Rect> Windows
        {
            get
            {
                return _windows;
            }
        }
    }
}

