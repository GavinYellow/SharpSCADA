using System;
using System.Collections.Generic;

namespace DataService
{
    [Flags]
    public enum DevStatus
    {
        None = 0,
        RUN = 0x0001,
        STOP = 0x0002,
        OLS = 0x0004,
        CLS = 0x0008,
        PAUSE = 0x0010,
        READY = 0x0020,
        DUMP = 0x0040,
        COMPLETE = 0x0080,
        ALARM = 0x0100,
    }

    [Flags]
    public enum DevCommand
    {
        START = 0x0001,
        STOP = 0x0002,
        ABORT = 0x0004,
        PAUSE = 0x0008,
        HOLD = 0x0010,
        WET = 0x0020,
        DISCH = 0x0040,
        DUMP = 0x0080,
    }

    public interface IDevice : IComparable<IDevice>
    {
        string Name { get; }
        string Description { get; }
        DeviceTypes DeviceType { get; }
        int ExtStatus { get; }
        DevStatus Status { get; }
        int Execute(DevCommand command);
    }

    //可以根据订单的源仓目标仓，二分法搜索到仓的列表。得到仓的参数和状态;部分设备是互斥的（如闸门，分配器，三通），马达等是共用的。


    public sealed class RouteVex : IComparable<RouteVex>, IEqualityComparer<RouteVex>
    {
        string _source;
        public string Source
        {
            get { return _source; }
        }

        string _dest;
        public string Dest
        {
            get { return _dest; }
        }

        RouteVex _front;
        public RouteVex Front
        {
            get { return _front; }
            set { _front = value; }
        }

        Func<bool> _func;
        Func<int> _funcWrite;
        public bool IsRun
        {
            get { return _func == null ? true : _func(); }
        }

        public RouteVex(string source, string dest = "")
        {
            _source = source;
            _dest = dest;
        }

        public RouteVex(string source, string dest, string startSignal, string runSignal, IDataServer srv)
        {
            _source = source;
            _dest = dest;
            var eval = srv.Eval;
            if (!string.IsNullOrEmpty(runSignal))
            {
                _func = eval.Eval(runSignal) as Func<bool>;
            }
            if (!string.IsNullOrEmpty(startSignal))
            {
                _funcWrite = eval.WriteEval(startSignal) as Func<int>;
            }
        }

        public int Start()
        {
            return _funcWrite == null ? 0 : _funcWrite();
        }

        public int CompareTo(RouteVex other)
        {
            if (this._source == null) return 0;
            if (other._dest == null) return 1;
            int comp = this._source.CompareTo(other._source);
            return comp == 0 ? this._dest.CompareTo(other._dest) : comp;
        }

        public override string ToString()
        {
            return string.Concat(_source, ",", _dest);
        }

        public bool Equals(RouteVex x, RouteVex y)
        {
            if (x == null || y == null) return false;
            return ((x.Source == null && y.Source == null) || (x.Source == y.Source)) &&
                ((x.Dest == null && y.Dest == null) || (x.Dest == y.Dest));
        }

        public int GetHashCode(RouteVex obj)
        {
            return obj.Source.GetHashCode() ^ obj.Dest.GetHashCode();
        }
    }

    //正在执行的路径，保存到一个XML文件，包含当前执行的节点
    public sealed class Route : LinkedList<RouteVex>, ICloneable
    {
        LinkedListNode<RouteVex> _current;
        public LinkedListNode<RouteVex> Current
        {
            get { return _current; }
            set { _current = value; }
        }

        string _start;
        public string Start
        {
            get { return _start; }
            set { _start = value; }
        }

        string _end;
        public string End
        {
            get { return _end; }
            set { _end = value; }
        }

        RouteState _state;
        public RouteState State
        {
            get { return _state; }
            set { _state = value; }
        }

        string[] _options;
        public string[] Options
        {
            get { return _options; }
            set { _options = value; }
        }


        public Route(string source, string dest, string[] options = null)
        {
            _start = source;
            _end = dest;
            _options = options;
        }

        public object Clone()
        {
            Route rt = new Route(_start, _end, _options);
            foreach (var routeVex in this)
            {
                rt.AddLast(routeVex);
            }
            return rt;
        }
    }

    public enum DeviceTypes
    {
        Misc,
        Motor = 1,
        Valve = 2,
        Gate = 3,
        Divert = 4,
        FourWays = 5,
        Distribute = 6,
        Bin = 7,
        Analog = 8,
        Scale = 9,
        Conveyor = 10,
        Elevator = 11,
        Mixer = 12,
        Grind = 13,
        Pellet = 14,
        Extruder = 15,
        CheckScale = 16,
        Liquid = 17,
        HandAdd = 18,
        Sifter = 19,
        Cyclone = 20,
        AirHammer = 21
    }

    public enum RouteState
    {
        Idle,
        Run,
        Stop,
        Suspend,
        Complete,
        Abort,
    }
}
