using DatabaseLib;
using DataService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CoreTest
{
    public partial class AlarmSet : Window
    {
        QueueCollection<AlarmItem> collection;//数据类型

        public AlarmSet()
        {
            InitializeComponent();
            this.SetWindowState();
            //list2.Height = Math.Max(this.Height - 350, 200);
            dtstart.Value = DateTime.Now.AddDays(-1);
            dtend.Value = DateTime.Now;
            this.CommandBindings.Add(new CommandBinding(MyCommands.Clear, (sender, e) =>
            {
                var tag = App.Server["_AlarmAck"];
                if (tag != null)
                {
                    tag.Write(true);
                    tag.Write(false);
                }
                collection.Clear();
            }, (sender, e) =>
            {
                e.CanExecute = collection != null;
            }));
            this.CommandBindings.Add(new CommandBinding(MyCommands.Commit, (sender, e) =>
            {
                switch (e.Parameter.ToString())
                {
                    case "0":
                        {
                            var cond = list0.SelectedItem as ICondition;
                            if (cond != null)
                            {
                                App.Server.AckConditions(cond);
                            }
                            //run1.Text = list0.Items.Count.ToString();
                        }
                        break;
                    case "1":
                        {
                            var conds = App.Server.ActivedConditionList;
                            var condarray = new ICondition[conds.Count];
                            conds.CopyTo(condarray, 0);
                            App.Server.AckConditions(condarray);
                            //run1.Text = "0";
                        }
                        break;
                }

            }, (sender, e) =>
            {
                e.CanExecute = list0.SelectedItem != null;
            }));
            this.CommandBindings.Add(new CommandBinding(MyCommands.Query, (sender, e) =>
            {
                List<AlarmItem> alist = new List<AlarmItem>();
                using (var reader = DataHelper.Instance.ExecuteProcedureReader("GetAlarm",
                    DataHelper.CreateParam("@StartTime", SqlDbType.DateTime, dtstart.Value.Value),
                    DataHelper.CreateParam("@EndTime", SqlDbType.DateTime, dtend.Value.Value)))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            alist.Add(new AlarmItem(reader.GetDateTime(0), reader.GetNullableString(1), reader.GetValue(2), (SubAlarmType)reader.GetInt32(3),
                              (Severity)reader.GetInt32(4), reader.GetInt32(5), reader.GetString(6)));
                        }
                    }
                }
                list1.ItemsSource = alist;
                //run2.Text = list1.Items.Count.ToString();
                //exp1.IsExpanded = true;
                //exp2.IsExpanded = false;
            }, (sender, e) =>
            {
                e.CanExecute = true;
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            collection = App.Server.AlarmList as QueueCollection<AlarmItem>;
            if (collection != null)
            {
                list2.ItemsSource = collection;
                collection.CollectionChanged += new NotifyCollectionChangedEventHandler(collection_CollectionChanged);
                //run3.Text = list2.Items.Count.ToString();
            }
            list0.ItemsSource = App.Server.ActivedConditionList;
            //run1.Text = list0.Items.Count.ToString();
            list3.ItemsSource = App.Events;
            var brush = Background as SolidColorBrush;
            if (brush != null)
                colorpicker.SelectedColor = brush.Color;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.SaveWindowState();
        }

        void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                list2.ScrollIntoView(e.NewItems[0]);
                //run3.Text = list2.Items.Count.ToString();
            }
            //run1.Text = list0.Items.Count.ToString();
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Background = new SolidColorBrush(e.NewValue);
        }

    }
}
