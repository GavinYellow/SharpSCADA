using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DataService;
using HMIControl;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using FORMS = System.Windows.Forms;

namespace CoreTest
{
    public static class WindowHelper
    {
        //绑定到DAServer,采用递归
        public static List<TagNodeHandle> BindingToServer(this DependencyObject panel, IDataServer _srv)
        {
            if (_srv == null) return null;
            ExpressionEval eval = _srv.Eval;
            List<TagNodeHandle> valueChangedList = new List<TagNodeHandle>();
            var items = panel.FindTagControls();
            if (items != null)
            {
                foreach (var element in items)
                {
                    BindingControl(element, valueChangedList, eval);
                }
            }
            eval.Clear();
            valueChangedList.Sort();
            return valueChangedList;
        }

        private static void BindingControl(ITagLink taglink, List<TagNodeHandle> valueChangedList, ExpressionEval eval)
        {
            var ctrl = taglink as UIElement;
            if (ctrl == null) return;
            var complex = taglink as ITagReader;
            if (complex != null)
            {
                string txt = complex.TagReadText;
                if (!string.IsNullOrEmpty(txt))
                {
                    foreach (var v in txt.GetListFromText())
                    {
                        ITagLink tagConn = complex;
                        string[] strs = v.Key.Split('.');
                        if (strs.Length > 1)
                        {
                            for (int i = 0; i < strs.Length - 1; i++)
                            {
                                var c = tagConn as ITagReader;
                                if (c == null || c.Children == null) break;
                                foreach (var item in c.Children)
                                {
                                    if (item.Node == strs[i])
                                    {
                                        tagConn = item;
                                        break;
                                    }
                                }
                            }
                        }
                        var r = tagConn as ITagReader;
                        var key = strs[strs.Length - 1];
                        try
                        {
                            var action = r.SetTagReader(key, eval.Eval(v.Value));
                            if (action != null)
                            {
                                action();
                                ValueChangedEventHandler handle = (s1, e1) =>
                                {
                                    ctrl.InvokeAsynchronously(action);
                                };
                                foreach (ITag tag in eval.TagList)
                                {
                                    valueChangedList.Add(new TagNodeHandle(tag.ID, key, tagConn, handle));
                                    tag.ValueChanged += handle;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            App.AddErrorLog(e);
                            MessageBox.Show(string.Format("设备'{0}'属性'{1}'的值'{2}'转化出错！", string.IsNullOrEmpty(r.Node) ? r.GetType().ToString() : r.Node, key, v.Value));
                        }
                        if (Attribute.IsDefined(tagConn.GetType(), typeof(StartableAttribute), false))
                        {
                            FrameworkElement element = tagConn as FrameworkElement;
                            element.Cursor = Cursors.UpArrow;
                            element.AddHandler(UIElement.MouseEnterEvent, new MouseEventHandler(element_MouseEnter));
                            element.AddHandler(UIElement.MouseLeaveEvent, new MouseEventHandler(element_MouseLeave));
                            element.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(element_MouseLeftButtonDown));
                        }
                        var hmi = tagConn as HMIControlBase;
                        if (hmi != null && hmi.ShowCaption && !string.IsNullOrEmpty(hmi.Caption))
                        {
                            AdornerLayer lay = AdornerLayer.GetAdornerLayer(hmi);
                            if (lay != null)
                            {
                                TextAdorner frame = new TextAdorner(hmi);
                                frame.Text = hmi.Caption;
                                lay.Add(frame);
                            }
                        }
                    }
                }
            }
            var writer = taglink as ITagWriter;
            if (writer != null && !string.IsNullOrEmpty(writer.TagWriteText))
            {
                var delgts = new List<Delegate>();
                foreach (var item in writer.TagWriteText.GetListFromText())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                            delgts.Add(eval.WriteEval(item.Key, item.Value));
                        else
                            delgts.Add(eval.WriteEval(item.Key));
                    }
                    catch (Exception e)
                    {
                        App.AddErrorLog(e);
                        MessageBox.Show(string.Format("设备{0}变量{1}写入PLC公式转换失败", taglink.Node, item.Key) + "\n" + e.Message);
                    }
                    writer.SetTagWriter(delgts);
                }
            }
        }

        static AnimationTimeline animaEnter = new DoubleAnimationUsingKeyFrames
        {
            KeyFrames = new DoubleKeyFrameCollection { new DiscreteDoubleKeyFrame(1.05, KeyTime.FromPercent(0)) }
        };

        static AnimationTimeline animaLeave = new DoubleAnimationUsingKeyFrames
        {
            KeyFrames = new DoubleKeyFrameCollection { new DiscreteDoubleKeyFrame(1, KeyTime.FromPercent(0)) }
        };

        static ScaleTransform GetScaleTransform(UIElement element)
        {
            var group = element.RenderTransform as TransformGroup;
            ScaleTransform scale = null;
            if (group == null)
            {
                scale = new ScaleTransform();
                element.RenderTransform = new TransformGroup { Children = new TransformCollection { element.RenderTransform, scale } };
            }
            else
            {
                if (group.IsFrozen)
                    group = group.Clone();
                foreach (var transform in group.Children)
                {
                    scale = transform as ScaleTransform;
                    if (scale != null)
                        break;
                }
                if (scale == null)
                {
                    scale = new ScaleTransform();
                    group.Children.Add(scale);
                }
                element.RenderTransform = group;
            }
            return scale;
        }

        static void element_MouseEnter(object sender, MouseEventArgs e)
        {
            UIElement element = sender as UIElement;
            //ScaleTransform scale = GetScaleTransform(element);
            //scale.BeginAnimation(ScaleTransform.ScaleXProperty, animaEnter);
            //scale.BeginAnimation(ScaleTransform.ScaleYProperty, animaEnter);
            AdornerLayer lay = AdornerLayer.GetAdornerLayer(element);
            if (lay != null)
            {
                FrameAdorner frame = new FrameAdorner(element);
                lay.Add(frame);
            }
        }

        static void element_MouseLeave(object sender, MouseEventArgs e)
        {
            UIElement element = sender as UIElement;
            //var scale = GetScaleTransform(element);
            //scale.BeginAnimation(ScaleTransform.ScaleXProperty, animaLeave);
            //scale.BeginAnimation(ScaleTransform.ScaleYProperty, animaLeave);
            AdornerLayer lay = AdornerLayer.GetAdornerLayer(element);
            if (lay != null)
            {
                var adorners = lay.GetAdorners(element);
                if (adorners != null)
                {
                    FrameAdorner frame = null;
                    for (int i = 0; i < adorners.Length; i++)
                    {
                        frame = adorners[i] as FrameAdorner;
                        if (frame != null)
                        {
                            lay.Remove(frame);
                            return;
                        }
                    }
                }
            }
        }
        static StartDevice frm1 = null;
        static void element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HMIControlBase ui = sender as HMIControlBase;
            if (ui != null && !string.IsNullOrEmpty(ui.DeviceName))
            {
                if (frm1 != null)
                    frm1.Close();
                frm1 = new StartDevice(ui.DeviceName, ui.PointToScreen(new Point(ui.ActualWidth / 2, ui.ActualHeight / 2)));
                frm1.Show();
            }
            e.Handled = true;
        }

        static ITagLink FindElement(ITagLink tag, string key)
        {
            ITagLink tagConn = tag;
            string[] strs = key.Split('.');
            for (int i = 0; i < strs.Length; i++)
            {
                var c = tagConn as ITagReader;
                if (c == null || c.Children == null) break;
                foreach (var item in c.Children)
                {
                    if (item.Node == strs[i])
                    {
                        tagConn = item;
                        break;
                    }
                }
            }
            return tagConn;
        }

        public static IEnumerable<DependencyObject> FindVisualChildren(this DependencyObject parent)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    yield return child;

                    var children = FindVisualChildren(child);
                    foreach (var item in children)
                        yield return item;
                }
            }
        }

        public static IEnumerable<T> FindChildren<T>(this DependencyObject parent) where T : class
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    var t = child as T;
                    if (t != null)
                        yield return t;

                    var children = FindChildren<T>(child);
                    foreach (var item in children)
                        yield return item;
                }
            }
        }

        public static IEnumerable<ITagLink> FindTagControls(this DependencyObject parent)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            if (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    var t = child as ITagLink;
                    if (t != null)
                        yield return t;
                    else
                    {
                        var children = FindTagControls(child);
                        foreach (var item in children)
                            yield return item;
                    }
                }
            }
        }

        public static LinkLine FindLine(this DependencyObject parent, string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return null;
            var controls = parent.FindTagControls(); if (controls == null) return null;
            var lines = new List<LinkLine>();
            Rect sourcerect = Rect.Empty;
            Rect targetrect = Rect.Empty;
            foreach (var ctrl in controls)
            {
                var element = ctrl as HMIControlBase;
                if (element != null)
                {
                    if (sourcerect == Rect.Empty && element.Caption == source)
                    {
                        sourcerect = new Rect(element.TranslatePoint(new Point(), element.Parent as UIElement), new Size(element.ActualWidth, element.ActualHeight));
                    }
                    else if (targetrect == Rect.Empty && element.Caption == target)
                    {
                        targetrect = new Rect(element.TranslatePoint(new Point(), element.Parent as UIElement), new Size(element.ActualWidth, element.ActualHeight));
                    }
                }
                else
                {
                    var line = ctrl as LinkLine;
                    if (line != null)
                    {
                        if (sourcerect != Rect.Empty && targetrect != Rect.Empty)
                        {
                            if (line.OriginInfo.DesignerRect == sourcerect && line.TargetInfo.DesignerRect == targetrect)
                                return line;
                        }
                        else
                            lines.Add(line);
                    }
                }
            }
            if (sourcerect == Rect.Empty || targetrect == Rect.Empty)
                return null;
            foreach (LinkLine line in lines)
            {
                if (line.OriginInfo.DesignerRect == sourcerect && line.TargetInfo.DesignerRect == targetrect)
                    return line;
            }
            return null;
        }

        public static T HitTest<T>(this Visual hitobj, Point p)
        {
            var target = VisualTreeHelper.HitTest(hitobj, p).VisualHit;
            while (target != null && !(target is T))
            {
                target = VisualTreeHelper.GetParent(target);
            }
            return target == null ? default(T) : (T)((object)target);
        }

        public static Window GetParentWindow(this DependencyObject child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
            {
                return null;
            }

            Window parent = parentObject as Window;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return GetParentWindow(parentObject);
            }
        }

        public static void RemoveHandles(this IDataServer srv, params List<TagNodeHandle>[] handleLists)
        {
            foreach (var handleList in handleLists)
            {
                if (handleList != null)
                {
                    foreach (var item in handleList)
                    {
                        srv[item.TagID].ValueChanged -= item.Handle;
                        var element = item.Element as FrameworkElement;
                        if (element != null)
                        {
                            element.RemoveHandler(UIElement.MouseEnterEvent, new MouseEventHandler(element_MouseEnter));
                            element.RemoveHandler(UIElement.MouseLeaveEvent, new MouseEventHandler(element_MouseLeave));
                            element.RemoveHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(element_MouseLeftButtonDown));
                        }
                    }
                    handleList.Clear();
                }
            }
        }

        public static void SetWindowState(this Window window)
        {
            Dictionary<string, Rect> dict = ConfigCache.Windows;
            string name = window.DependencyObjectType.Name;
            if (dict.ContainsKey(name))
            {
                Rect rec = dict[name];
                if (rec.Width == 0 || rec.Height == 0 || double.IsInfinity(rec.Left) || double.IsInfinity(rec.Top))
                {
                    window.WindowState = WindowState.Maximized;
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                else
                {
                    window.Left = rec.Left;
                    window.Top = rec.Top;
                    window.Width = rec.Width;
                    window.Height = rec.Height;
                }
            }
            else
            {
                ConfigCache.Windows.Add(name, new Rect());
                window.WindowState = WindowState.Maximized;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        public static void SaveWindowState(this Window window)
        {
            Dictionary<string, Rect> dict = ConfigCache.Windows;
            dict[window.DependencyObjectType.Name] = window.WindowState == WindowState.Maximized ? new Rect() :
                new Rect(window.Left, window.Top, window.Width, window.Height);
        }

        public static Window FindWindow(string name)
        {
            foreach (Window window in App.Current.Windows)
            {
                if (window.DependencyObjectType.Name == name)
                {
                    return window;
                }
            }
            return null;
        }

        public static void SetWindowPos(this Window window, int screenIndex)
        {
            var screens = FORMS.Screen.AllScreens;//只是测试状态，待完善
            if (screenIndex < screens.Length)
            {
                var area = screens[screenIndex].WorkingArea;
                if (window.WindowState == WindowState.Maximized)
                {
                    window.Top = area.Top;
                    window.Left = area.Left;
                }
                else
                {
                    window.Top += area.Top;
                    window.Left += area.Left;
                }
            }
        }

        public static void InvokeAsynchronously(this UIElement element, Action action)
        {
            if (element.Dispatcher.CheckAccess())
            {
                action();
            }
            else
                element.Dispatcher.BeginInvoke(action, System.Windows.Threading.DispatcherPriority.Normal);
        }

        public static void GetWorkInfo(DateTime current, out DateTime start, out DateTime end, out string team)
        {
            float[] startarray = new float[3] { App.Server["START_1"].Value.Single, App.Server["START_2"].Value.Single, App.Server["START_3"].Value.Single };
            float[] endarray = new float[3] { App.Server["END_1"].Value.Single, App.Server["END_2"].Value.Single, App.Server["END_3"].Value.Single };
            double hour = current.Hour + current.Minute / 60.0;
            //Array.Sort(startarray);
            DateTime today = current.Date;
            for (int i = 0; i < startarray.Length; i++)
            {
                if (startarray[i] == endarray[i]) continue;
                if (hour >= startarray[i] && hour <= endarray[i])
                {
                    start = today.AddHours(startarray[i]);
                    end = today.AddHours(endarray[i]);
                    team = i.ToString();
                    return;
                }
                else if (startarray[i] >= endarray[i] && hour >= startarray[i])
                {
                    start = today.AddHours(startarray[i]);
                    end = today.AddDays(1).AddHours(endarray[i]);
                    team = i.ToString();
                    return;
                }
                else if (startarray[i] >= endarray[i] && hour <= endarray[i])
                {
                    start = today.AddDays(-1).AddHours(startarray[i]);
                    end = today.AddHours(endarray[i]);
                    team = i.ToString();
                    return;
                }
            }
            start = current.AddHours(-8);
            end = current;
            team = null;
        }

        //startTime/endTime:班次上/下班时间 starthour/endhour:阶段起始/结束时间 start/end:本班次阶段区间 
        public static void GetTimeRange(DateTime startTime, DateTime endTime, float starthour, float endhour, out DateTime start, out DateTime end)
        {
            if (starthour == endhour)
            {
                start = end = DateTime.MinValue;
                return;
            }
            DateTime startdate = startTime.Date;
            DateTime enddate = endTime.Date;
            double hstart = startTime.Hour + startTime.Minute / 60.0 + startTime.Second / 60.0;
            double hend = endTime.Hour + endTime.Minute / 60.0 + endTime.Second / 60.0;
            if (startdate == enddate)
            {
                if (endhour > starthour)
                {
                    start = starthour > hstart ? startdate.AddHours(starthour) : startTime;
                    end = endhour < hend ? startdate.AddHours(endhour) : endTime;
                }
                else
                {
                    if (endhour > hstart)
                    {
                        start = startTime;
                        end = endhour > hend ? endTime : startdate.AddHours(endhour);
                    }
                    else if (starthour < hend)
                    {
                        end = endTime;
                        start = starthour > hstart ? startdate.AddHours(starthour) : startTime;
                    }
                    else
                    {
                        start = end = DateTime.MinValue;
                    }
                }
            }
            else
            {
                if (endhour > starthour)
                {
                    if (endhour > hstart)
                    {
                        start = starthour > hstart ? startdate.AddHours(starthour) : startTime;
                        end = startdate.AddHours(endhour);
                    }
                    else if (starthour < hend)
                    {
                        end = endhour < hend ? enddate.AddHours(endhour) : endTime;
                        start = enddate.AddHours(starthour);
                    }
                    else
                    {
                        start = end = DateTime.MinValue;
                    }
                }
                else
                {
                    start = starthour > hstart ? startdate.AddHours(starthour) : startTime;
                    end = endhour < hend ? enddate.AddHours(endhour) : endTime;
                }
            }
        }

        public static void SetSourceMapping(this QueueListSource<HistoryData> source, ITag currentTag)
        {
            if (currentTag != null)
            {
                switch (currentTag.Address.VarType)
                {
                    case DataType.BOOL:
                        source.SetYMapping(Y => Y.Value.Boolean ? 1 : 0);
                        break;
                    case DataType.BYTE:
                        source.SetYMapping(Y => Y.Value.Byte);
                        break;
                    case DataType.WORD:
                        source.SetYMapping(Y => Y.Value.Word);
                        break;
                    case DataType.SHORT:
                        source.SetYMapping(Y => Y.Value.Int16);
                        break;
                    case DataType.DWORD:
                        source.SetYMapping(Y => Y.Value.DWord);
                        break;
                    case DataType.INT:
                        source.SetYMapping(Y => Y.Value.Int32);
                        break;
                    case DataType.FLOAT:
                        source.SetYMapping(Y => Y.Value.Single);
                        break;
                }
            }
        }

        public static void SetSourceMapping(this ObservableDataSource<HistoryData> source, ITag currentTag)
        {
            if (currentTag != null)
            {
                switch (currentTag.Address.VarType)
                {
                    case DataType.BOOL:
                        source.SetYMapping(Y => Y.Value.Boolean ? 1 : 0);
                        break;
                    case DataType.BYTE:
                        source.SetYMapping(Y => Y.Value.Byte);
                        break;
                    case DataType.WORD:
                        source.SetYMapping(Y => Y.Value.Word);
                        break;
                    case DataType.SHORT:
                        source.SetYMapping(Y => Y.Value.Int16);
                        break;
                    case DataType.DWORD:
                        source.SetYMapping(Y => Y.Value.DWord);
                        break;
                    case DataType.INT:
                        source.SetYMapping(Y => Y.Value.Int32);
                        break;
                    case DataType.FLOAT:
                        source.SetYMapping(Y => Y.Value.Single);
                        break;
                }
            }
        }
    }

    public class TagNodeHandle : IComparable<TagNodeHandle>
    {
        short _tagID;
        public short TagID
        {
            get
            {
                return _tagID;
            }
        }

        string _key;
        public string Key
        {
            get
            {
                return _key;
            }
        }

        ITagLink _element;
        public ITagLink Element
        {
            get
            {
                return _element;
            }
        }

        ValueChangedEventHandler _handle;
        public ValueChangedEventHandler Handle
        {
            get
            {
                return _handle;
            }
        }

        public TagNodeHandle(short tag, string key, ITagLink element, ValueChangedEventHandler handle)
        {
            _tagID = tag;
            _key = key;
            _element = element;
            _handle = handle;
        }

        public int CompareTo(TagNodeHandle other)
        {
            int comp = _tagID.CompareTo(other._tagID);
            return comp == 0 ? _key.CompareTo(other._key) : comp;
        }
    }

    public static class Extension
    {
        public static string GetNullableString(this SqlDataReader dataReader, int index)
        {
            var svr = dataReader.GetSqlString(index);
            return svr.IsNull ? null : svr.Value;
        }

        public static string GetNullableString(this DateTime? time)
        {
            return time.HasValue ? "'" + time.ToString() + "'" : "NULL";
        }

        /// <summary>
        /// 返回指定变量最接近指定时间的值
        /// </summary>
        /// <param name="list"></param>
        /// <param name="time">时间</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int GetInt(this IEnumerable<HistoryData> list, DateTime time, short id)
        {
            var data = (from raw in list where raw.ID == id && raw.TimeStamp <= time && raw.Value.Int32 > 0 select raw.Value.Int32).LastOrDefault();
            if (data == 0) data = (from raw in list where raw.ID == id && raw.TimeStamp >= time && raw.Value.Int32 > 0 select raw.Value.Int32).FirstOrDefault();
            return data;
        }

        /// <summary>
        /// 返回变量最接近指定时间的值
        /// </summary>
        /// <param name="list"></param>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static int GetInt(this IEnumerable<HistoryData> list, DateTime time)
        {
            var data = (from raw in list where raw.TimeStamp <= time && raw.Value.Int32 > 0 select raw.Value.Int32).LastOrDefault();
            if (data == 0) data = (from raw in list where raw.TimeStamp >= time && raw.Value.Int32 > 0 select raw.Value.Int32).FirstOrDefault();
            return data;
        }

        /// <summary>
        /// 返回在开始时间和结束时间范围内变量的变化值
        /// </summary>
        /// <param name="list"></param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static int GetInt(this IEnumerable<HistoryData> list, DateTime start, DateTime end, short id)
        {
            return GetInt(list, end, id) - GetInt(list, start, id);
        }

        /// <summary>
        /// 返回指定变量最接近指定时间的值
        /// </summary>
        /// <param name="list"></param>
        /// <param name="time">时间</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static float GetFloat(this IEnumerable<HistoryData> list, DateTime time, short id)
        {
            var data = (from raw in list where raw.ID == id && raw.TimeStamp <= time && raw.Value.Single > 0 select raw.Value.Single).LastOrDefault();
            if (data == 0) data = (from raw in list where raw.ID == id && raw.TimeStamp >= time && raw.Value.Single > 0 select raw.Value.Single).FirstOrDefault();
            return data;
        }

        /// <summary>
        /// 返回变量最接近指定时间的值
        /// </summary>
        /// <param name="list"></param>
        /// <param name="time">时间</param>
        /// <returns></returns>
        public static float GetFloat(this IEnumerable<HistoryData> list, DateTime time)
        {
            var data = (from raw in list where raw.TimeStamp <= time && raw.Value.Single > 0 select raw.Value.Single).LastOrDefault();
            if (data == 0) data = (from raw in list where raw.TimeStamp >= time && raw.Value.Single > 0 select raw.Value.Single).FirstOrDefault();
            return data;
        }

        /// <summary>
        /// 返回在开始时间和结束时间范围内变量的变化值
        /// </summary>
        /// <param name="list"></param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static float GetFloat(this IEnumerable<HistoryData> list, DateTime start, DateTime end, short id)
        {
            return GetFloat(list, end, id) - GetFloat(list, start, id);
        }

        public static float GetFloat2(this List<HistoryData> list, DateTime start, DateTime end, short id)
        {
            float result = 0;
            var data = (from raw in list where raw.TimeStamp >= start && raw.TimeStamp <= end && raw.ID == id && raw.Value.Single >= 0 select raw.Value.Single).ToList();
            if (data.Count > 0)
            {
                List<int> li = new List<int>();
                li.Add(0);
                for (int i = 0; i < data.Count - 1; i++)
                {
                    if (data[i] > data[i + 1] && i > 0)
                    {
                        li.Add(i);
                    }
                }
                li.Add(data.Count - 1);
                for (int i = 1; i < li.Count; i++)
                {
                    result += data[li[i]] - data[li[i - 1] + 1];
                }
            }
            return result;
        }
    }
}
