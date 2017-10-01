using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using DataService;
using Microsoft.Research.DynamicDataDisplay;

namespace CoreTest
{
    /// <summary>
    /// Interaction logic for RuntimeChart.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for RuntimeChart.xaml
    /// </summary>
    public partial class RuntimeChart : Window
    {
        int TICK = 5000;
        ITag currentTag;
        Timer timer;
        bool writelock;
        DateTime timestamp = DateTime.Now;
        QueueListSource<HistoryData> source1 = new QueueListSource<HistoryData>(ConfigCache.RtCap);
        public RuntimeChart()
        {
            InitializeComponent();
            this.SetWindowState();
            var brush = chartPlotter1.Background as SolidColorBrush;
            if (brush != null)
                colorpicker.SelectedColor = brush.Color;
            TICK = ConfigCache.RtWaitTick;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comb1.ItemsSource = App.Server.ArchiveList;
            source1.SetXMapping(X => hTimeSpanAxis.ConvertToDouble(X.TimeStamp));

            graph = chartPlotter1.AddLineGraph(source1, 2, "");
            IGroup grp = App.Server.GetGroupByName(null);
            if (grp != null)
            {
                grp.DataChange += new DataChangeEventHandler(grp_DataChange);
            }
            timer = new Timer(onTimer, null, 0, TICK);
        }

        void grp_DataChange(object sender, DataChangeEventArgs e)
        {
            writelock = true;
            var arr = e.Values;
            if (arr != null && currentTag != null)
            {
                foreach (HistoryData data in arr)
                {
                    if (data.ID == currentTag.ID)
                    {
                        source1.AppendAsync(Dispatcher, data);
                        timestamp = DateTime.Now;
                    }
                }
            }
            writelock = false;
        }

        void onTimer(object state)
        {
            if (!writelock && (DateTime.Now - timestamp).TotalMilliseconds > TICK)
            {
                HistoryData data = HistoryData.Empty;
                if (currentTag != null)
                {
                    data.Value = currentTag.Value;
                    data.TimeStamp = DateTime.Now;
                    source1.AppendAsync(Dispatcher, data);
                }
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            IGroup grp = App.Server.GetGroupByName(null);
            if (grp != null)
            {
                grp.DataChange -= new DataChangeEventHandler(grp_DataChange);
            }
            timer.Dispose();
            this.SaveWindowState();
        }

        LineGraph graph;
        private void comb1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                currentTag = null;
                source1.Collection.Clear();
                var str = (KeyValuePair<short, string>)e.AddedItems[0];
                graph.Description = new PenDescription(str.Value ?? "");
                currentTag = App.Server[str.Key];
                int index = App.Server.GetItemProperties(str.Key);
                if (index >= 0)
                {
                    hilevel.Value = App.Server.MetaDataList[index].Maximum;
                    lolevel.Value = App.Server.MetaDataList[index].Minimum;
                }
                if (hilevel.Value == lolevel.Value)
                {
                    hilevel.Value = 100000;
                    lolevel.Value = -100000;
                }
                source1.SetSourceMapping(currentTag);
                chartPlotter1.FitToView();
            }
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            chartPlotter1.Background = new SolidColorBrush(e.NewValue);
        }
    }
}
