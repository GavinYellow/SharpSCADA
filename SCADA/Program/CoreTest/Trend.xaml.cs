using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DataService;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace CoreTest
{
    /// <summary>
    /// Interaction logic for DynamicChart.xaml
    /// </summary>
    public partial class Trend : Window
    {
        int SMALLTICK = 10;
        int LARGETICK = 60;
        SortedList<short, ObservableDataSource<HistoryData>> sortlist = new SortedList<short, ObservableDataSource<HistoryData>>();
        SortedList<short, IPlotterElement> plotterlist = new SortedList<short, IPlotterElement>();

        public Trend()
        {
            InitializeComponent();
            this.SetWindowState();
            LARGETICK = ConfigCache.HdaLargeTick;
            SMALLTICK = ConfigCache.HdaSmallTick;
            this.CommandBindings.Add(new CommandBinding(MyCommands.First, (sender, e) =>
            {
                dtstart.Value -= TimeSpan.FromMinutes(LARGETICK);
                dtend.Value -= TimeSpan.FromMinutes(LARGETICK);
                BindingData();
            }, CmdCanExecute));
            this.CommandBindings.Add(new CommandBinding(MyCommands.Previous, (sender, e) =>
            {
                dtstart.Value -= TimeSpan.FromMinutes(SMALLTICK);
                dtend.Value -= TimeSpan.FromMinutes(SMALLTICK);
                BindingData();
            }, CmdCanExecute));
            this.CommandBindings.Add(new CommandBinding(MyCommands.Next, (sender, e) =>
            {
                dtstart.Value += TimeSpan.FromMinutes(SMALLTICK);
                dtend.Value += TimeSpan.FromMinutes(SMALLTICK);
                BindingData();
            }, CmdCanExecute));
            this.CommandBindings.Add(new CommandBinding(MyCommands.Last, (sender, e) =>
            {
                dtstart.Value += TimeSpan.FromMinutes(LARGETICK);
                dtend.Value += TimeSpan.FromMinutes(LARGETICK);
                BindingData();
            }, CmdCanExecute));
            this.CommandBindings.Add(new CommandBinding(MyCommands.Query, (sender, e) =>
            {
                BindingData();
            }, CmdCanExecute));
            dtstart.Value = DateTime.Now.AddMinutes(-LARGETICK);
            dtend.Value = DateTime.Now;
            chklist.ItemsSource = App.Server.ArchiveList;
            var brush = chartPlotter1.Background as SolidColorBrush;
            if (brush != null)
                colorpicker.SelectedColor = brush.Color;
            cursor.XTextMapping = x => hTimeSpanAxis.ConvertFromDouble(x).ToString();
        }

        void BindingData()
        {
            if (!dtstart.Value.HasValue || !dtend.Value.HasValue) return;
            DateTime start = dtstart.Value.Value;
            DateTime end = dtend.Value.Value;
            if ((end - start).Days > 7)
            {
                end = start.AddDays(7);
                dtend.Value = end;
            }
            else if (start >= end)
            {
                end = start.AddDays(1);
                dtend.Value = end;
            }
            HashSet<short> hash = new HashSet<short>();
            foreach (var item in sortlist)
            {
                item.Value.Collection.Clear();
                hash.Add(item.Key);
            }

            foreach (KeyValuePair<short, string> item in chklist.SelectedItems)
            {
                ObservableDataSource<HistoryData> source;
                if (!sortlist.TryGetValue(item.Key, out source))
                {
                    source = new ObservableDataSource<HistoryData>();
                    source.SetXMapping(X => hTimeSpanAxis.ConvertToDouble(X.TimeStamp));
                    source.SetSourceMapping(App.Server[item.Key]);
                    if (comodel.SelectedIndex == 0
                        || (comodel.SelectedIndex == 1 && sortlist.Count == 0))
                    //|| (comodel.SelectedIndex == 2 && (sortlist.Count == 0 || chklist.SelectedItems.Count == 1)))
                    {
                        plotterlist[item.Key] = chartPlotter1.AddLineGraph(source, 2, item.Value);
                    }
                    else
                    {
                        var innerPlotter = new InjectedPlotter() { SetViewportBinding = false };
                        var axis = new HorizontalDateTimeAxis();
                        innerPlotter.Children.Add(axis);
                        innerPlotter.MainHorizontalAxis = axis;
                        innerPlotter.Children.Add(new VerticalAxis() { Placement = AxisPlacement.Right });
                        innerPlotter.Children.Add(new VerticalAxisTitle() { Content = item.Value, Placement = AxisPlacement.Right });
                        chartPlotter1.Children.Add(innerPlotter);
                        innerPlotter.AddLineGraph(source, 2, item.Value);
                        plotterlist.Add(item.Key, innerPlotter);
                    }
                    sortlist.Add(item.Key, source);
                }
                hash.Remove(item.Key);
                var data = App.Server.ReadRaw(start, end, item.Key);
                if (data != null)
                {
                    bool first = true;
                    var temp = new HistoryData(item.Key, QUALITIES.QUALITY_GOOD, Storage.Empty, start);
                    source.SuspendUpdate();
                    foreach (var p in data)
                    {
                        if (first)
                        {
                            temp.Value = p.Value;
                            source.Collection.Add(temp);
                            first = false;
                        }
                        source.Collection.Add(p);
                    }
                    if (source.Collection.Count == 0)
                        source.Collection.Add(temp);
                    temp = source.Collection[source.Collection.Count - 1];
                    temp.TimeStamp = end;
                    source.Collection.Add(temp);
                    source.ResumeUpdate();
                }
                chartPlotter1.FitToView();
            }
            foreach (short id in hash)
            {
                sortlist.Remove(id);
                IPlotterElement plotter;
                if (plotterlist.TryGetValue(id, out plotter))
                {
                    var chart = plotter as Plotter;
                    if (chart != null)
                    {
                        for (int i = chart.Children.Count - 1; i >= 0; i--)
                        {
                            chart.Children[i].RemoveFromPlotter();
                        }
                    }
                    plotter.RemoveFromPlotter();
                    plotterlist.Remove(id);
                }
            }
        }

        void CmdCanExecute(object target, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dtstart.Value.HasValue && dtend.Value.HasValue;
            //e.CanExecute = App.Principal.IsInRole("中控员");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.SaveWindowState();
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            chartPlotter1.Background = new SolidColorBrush(e.NewValue);
        }

        private void comodel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (var source in sortlist.Values)
            {
                source.Collection.Clear();
            }
            foreach (var plotter in plotterlist.Values)
            {
                var chart = plotter as Plotter;
                if (chart != null)
                {
                    for (int i = chart.Children.Count - 1; i >= 0; i--)
                    {
                        chart.Children[i].RemoveFromPlotter();
                    }
                }
                plotter.RemoveFromPlotter();
            }
            sortlist.Clear();
            plotterlist.Clear();
            BindingData();
        }

        private void chkshow_Click(object sender, RoutedEventArgs e)
        {
            if (chkshow.IsChecked == true)
            {
                cursor.Visibility = Visibility.Visible;
                axiscuror.ShowHorizontalLine = true; axiscuror.ShowVerticalLine = true;
            }
            else
            {
                cursor.Visibility = Visibility.Collapsed;
                axiscuror.ShowHorizontalLine = false; axiscuror.ShowVerticalLine = false;
            }
        }
    }
}

