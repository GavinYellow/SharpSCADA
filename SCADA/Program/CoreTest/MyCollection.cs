using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace CoreTest
{
    public class QueueCollection<T> : IEnumerable<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        Queue<T> _queue;

        public int Count
        {
            get
            {
                return _queue.Count;
            }
        }

        public QueueCollection()
        {
            this._queue = new Queue<T>();
        }

        public QueueCollection(Queue<T> queue)
        {
            this._queue = queue;
        }

        public QueueCollection(int capacity)
        {
            this._queue = new Queue<T>(capacity);
        }

        public T Dequeue()
        {
            var value = _queue.Dequeue();
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, value, 0);
            return value;
        }

        public void Enqueue(T value)
        {
            _queue.Enqueue(value);
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, value, _queue.Count);
        }


        public void Clear()
        {
            _queue.Clear();
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                try
                {
                    // Walk thru invocation list
                    foreach (NotifyCollectionChangedEventHandler handler in CollectionChanged.GetInvocationList())
                    {
                        DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
                        // If the subscriber is a DispatcherObject and different thread
                        if (dispatcherObject != null && !dispatcherObject.CheckAccess())
                        {
                            // Invoke handler in the target dispatcher's thread
                            dispatcherObject.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, handler, this, e);
                        }
                        else // Execute handler as is
                            handler(this, e);
                    }
                }
                catch (Exception err) { }
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }


        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        #endregion
    }

    public class QueueListSource<T> : IPointDataSource
    {
        private class QueueListIterator : IPointEnumerator, IDisposable
        {
            private readonly QueueListSource<T> dataSource;
            private readonly IEnumerator<T> enumerator;

            public QueueListIterator(QueueListSource<T> dataSource)
            {
                this.dataSource = dataSource;
                this.enumerator = dataSource.collection.GetEnumerator();
            }
            public bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }
            public void GetCurrent(ref Point p)
            {
                this.dataSource.FillPoint(this.enumerator.Current, ref p);
            }
            public void ApplyMappings(DependencyObject target)
            {
                this.dataSource.ApplyMappings(target, this.enumerator.Current);
            }
            public void Dispose()
            {
                this.enumerator.Dispose();
                GC.SuppressFinalize(this);
            }
        }
        private bool collectionChanged;
        private bool updatesEnabled = true;
        private int _capacity;
        private readonly QueueCollection<T> collection;
        private readonly List<Mapping<T>> mappings = new List<Mapping<T>>();
        private Func<T, double> xMapping;
        private Func<T, double> yMapping;
        private Func<T, Point> xyMapping;
        private FIFOHandler handle;
        public event EventHandler DataChanged;
        public QueueCollection<T> Collection
        {
            get
            {
                return this.collection;
            }
        }
        public QueueListSource(int capacity)
        {
            _capacity = capacity;
            collection = new QueueCollection<T>(capacity);
            this.collection.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnCollectionChanged);
            handle = new FIFOHandler(FIFO);
            if (typeof(T) == typeof(Point))
            {
                this.xyMapping = ((T t) => (Point)((object)t));
            }
        }
        public QueueListSource(IEnumerable<T> data)
            : this(data.Count())
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            foreach (T current in data)
            {
                this.collection.Enqueue(current);
            }
        }
        public void SuspendUpdate()
        {
            this.updatesEnabled = false;
        }
        public void ResumeUpdate()
        {
            this.updatesEnabled = true;
            if (this.collectionChanged)
            {
                this.collectionChanged = false;
                this.RaiseDataChanged();
            }
        }
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.updatesEnabled)
            {
                this.RaiseDataChanged();
                return;
            }
            this.collectionChanged = true;
        }
        public void AppendMany(IEnumerable<T> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            this.updatesEnabled = false;
            foreach (T current in data)
            {
                this.collection.Enqueue(current);
            }
            this.updatesEnabled = true;
            this.RaiseDataChanged();
        }
        //已重写
        public void AppendAsync(Dispatcher dispatcher, T item)
        {
            dispatcher.Invoke(handle, DispatcherPriority.Normal, item);
        }

        public void SetXMapping(Func<T, double> mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }
            this.xMapping = mapping;
        }
        public void SetYMapping(Func<T, double> mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }
            this.yMapping = mapping;
        }
        public void SetXYMapping(Func<T, Point> mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }
            this.xyMapping = mapping;
        }

        private void FIFO(T item)
        {
            if (this.collection.Count >= _capacity)
            {
                this.collection.Dequeue();
            }
            this.collection.Enqueue(item);
            this.RaiseDataChanged();
        }
        private void FillPoint(T elem, ref Point point)
        {
            if (this.xyMapping != null)
            {
                point = this.xyMapping(elem);
                return;
            }
            if (this.xMapping != null)
            {
                point.X = this.xMapping(elem);
            }
            if (this.yMapping != null)
            {
                point.Y = this.yMapping(elem);
            }
        }
        private void ApplyMappings(DependencyObject target, T elem)
        {
            if (target != null)
            {
                foreach (Mapping<T> current in this.mappings)
                {
                    target.SetValue(current.Property, current.F(elem));
                }
            }
        }
        public IPointEnumerator GetEnumerator(DependencyObject context)
        {
            return new QueueListSource<T>.QueueListIterator(this);
        }
        private void RaiseDataChanged()
        {
            if (this.DataChanged != null)
            {
                this.DataChanged(this, EventArgs.Empty);
            }
        }

        public delegate void FIFOHandler(T item);
    }


    internal sealed class Mapping<TSource>
    {
        internal DependencyProperty Property
        {
            get;
            set;
        }
        internal Func<TSource, object> F
        {
            get;
            set;
        }
    }

    public sealed class ReverseObservableQueue<T> : List<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public ReverseObservableQueue(int capacity)
            : base(capacity)
        {
            this.capacity = capacity;
        }

        int capacity;
        object async = new object();

        public void ReverseEnqueue(T item)
        {
            lock (async)
            {
                while (Count >= capacity) RemoveAt(Count - 1);
                Insert(0, item);
                this.OnPropertyChanged("Count");
                this.OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, 0));
            }
        }

        public T ReverseDequeue()
        {
            lock (async)
            {
                if (base.Count > 1)
                {
                    T item = this[Count - 1];
                    Remove(item);
                    this.OnPropertyChanged("Count");
                    this.OnPropertyChanged("Item[]");
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, Count - 1));
                    return item;
                }
                return default(T);
            }
        }

        public new void Clear()
        {
            lock (async)
            {
                base.Clear();
                this.OnPropertyChanged("Count");
                this.OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                try
                {
                    var Handlers = CollectionChanged.GetInvocationList();
                    foreach (NotifyCollectionChangedEventHandler handler in Handlers)
                    {
                        DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
                        // If the subscriber is a DispatcherObject and different thread
                        if (dispatcherObject != null && !dispatcherObject.CheckAccess())
                        {
                            // Invoke handler in the target dispatcher's thread
                            dispatcherObject.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, handler, this, e);
                        }
                        else // Execute handler as is
                            handler(this, e);
                    }
                }
                catch (Exception err) { }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        // Override the event so this class can access it
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Be nice - use BlockReentrancy like MSDN said
            using (BlockReentrancy())
            {
                NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;
                if (eventHandler == null)
                    return;

                Delegate[] delegates = eventHandler.GetInvocationList();
                // Walk thru invocation list
                foreach (NotifyCollectionChangedEventHandler handler in delegates)
                {
                    DispatcherObject dispatcherObject = handler.Target as DispatcherObject;
                    // If the subscriber is a DispatcherObject and different thread
                    if (dispatcherObject != null && dispatcherObject.CheckAccess() == false)
                    {
                        // Invoke handler in the target dispatcher's thread
                        dispatcherObject.Dispatcher.Invoke(DispatcherPriority.DataBind, handler, this, e);
                    }
                    else // Execute handler as is
                        handler(this, e);
                }
            }
        }
    }
}
