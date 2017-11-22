using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DataService
{
    public delegate void AlarmEventHandler(object sender, AlarmItem e);

    public interface IEvent
    {
        bool IsEnabled { get; set; }
        bool IsAcked { get; set; }
        bool IsActived { get; set; }
        Severity Severity { get; }
        EventType EventType { get; }
        DateTime LastActive { get; set; }
        string Comment { get; }
    }

    public abstract class ICondition : IEvent, IDisposable, IComparable<ICondition>, IEquatable<ICondition>
    {
        public const string ALARMSTOP = "Alarm Clear";
        protected bool _enable, _ack, _active;
        protected int _id;
        protected DateTime _timeStamp;
        protected SubAlarmType _tempType;
        protected SubCondition[] _subConditions;

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _enable;
            }
            set
            {
                _enable = value;
            }
        }

        public bool IsActived
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
            }
        }

        public bool IsAcked
        {
            get
            {
                return _ack;
            }
            set
            {
                _ack = value;
                if (_ack)
                {
                    _tempType = SubAlarmType.None;
                    _lastAckTime = DateTime.Now;
                    if (_ack && AlarmAck != null)
                    {
                        foreach (EventHandler deleg in AlarmAck.GetInvocationList())
                        {
                            deleg.BeginInvoke(this, EventArgs.Empty, null, null);
                        }
                    }
                }
            }
        }

        public abstract AlarmType AlarmType
        {
            get;
        }

        public EventType EventType
        {
            get
            {
                return EventType.ConditionEvent;
            }
        }


        protected ConditionType _conditionType;
        public ConditionType ConditionType
        {
            get
            {
                return _conditionType;
            }
            set
            {
                _conditionType = value;
            }
        }

        protected DateTime _lastAckTime, _condLastActive, _lastInactive;
        public DateTime LastAckTime
        {
            get
            {
                return _lastAckTime;
            }
            set
            {
                _lastAckTime = value;
            }
        }

        public DateTime SubCondLastActive
        {
            get
            {
                return _current.StartTime;
            }
            //set
            //{
            //    _subCondLastActive = value;
            //}
        }

        public DateTime LastActive
        {
            get
            {
                return _condLastActive;
            }
            set
            {
                _condLastActive = value;
            }
        }

        public DateTime LastInactive
        {
            get
            {
                return _lastInactive;
            }
            set
            {
                _lastInactive = value;
            }
        }

        protected AlarmItem _current;

        protected SubAlarmType _activeSub;
        public SubAlarmType ActiveSubCondition
        {
            get
            {
                return _activeSub;
            }
            protected set
            {
                _activeSub = value;
            }
        }

        public Severity Severity
        {
            get
            {
                return _current.Severity;
            }
        }

        public string Message
        {
            get
            {
                return _current.AlarmText;
            }
        }

        public abstract string Value
        {
            get;
        }

        protected float _para;
        public float Para
        {
            get
            {
                return _para;
            }
            set
            {
                _para = value;
            }
        }

        protected float _deadBand;
        public float DeadBand
        {
            get
            {
                return _deadBand;
            }
            set
            {
                _deadBand = value;
            }
        }

        protected int _delay;
        public int Delay
        {
            get
            {
                return _delay;
            }
            set
            {
                _delay = value;
            }
        }

        protected string _comment;
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;
            }
        }

        protected string _source;
        public string Source
        {
            get
            {
                return _source;
            }
        }

        public IList<SubCondition> SubConditions
        {
            get
            {
                return _subConditions;
            }
        }

        public AlarmEventHandler AlarmActive;
        public EventHandler AlarmAck;

        protected ICondition(int id, ConditionType conditionType, string source, string comment, float para, float deadBand, int delay)
        {
            this._id = id;
            this._conditionType = conditionType;
            this._para = para;
            this._source = source;
            this._comment = comment;
            this._deadBand = deadBand;
            this._delay = delay;
            this._current = new AlarmItem();
        }

        public abstract bool AddSubCondition(SubCondition condition);

        public abstract bool RemoveSubCondition(SubCondition condition);

        protected abstract void OnActive(SubCondition condition, Storage value, DateTime timeStamp);

        protected abstract void OnInActive(Storage value);

        public int CompareTo(ICondition other)
        {
            int comp1 = ((int)this.Severity).CompareTo((int)other.Severity);
            return comp1 == 0 ? this.LastActive.CompareTo(other.LastActive) : -comp1;
        }

        public bool Equals(ICondition other)
        {
            if (other == null) return false;
            return this._id == other._id;
        }

        public virtual void Dispose()
        {
            _current = null;
            AlarmAck = null;
            AlarmActive = null; ;
        }
    }

    public abstract class SimpleCondition : ICondition, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public override string Value
        {
            get
            {
                return _tag == null ? null : _tag.ToString();
            }
        }

        protected ITag _tag;
        public ITag Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                if (_tag != null)
                    _tag.ValueChanging -= CheckAlarm;
                _tag = value;
                if (_tag != null)
                    _tag.ValueChanging += CheckAlarm;
            }
        }

        protected SimpleCondition(int id, ConditionType conditionType, string source, string comment, float para, float deadBand, int delay) :
            base(id, conditionType, source, comment, para, deadBand, delay)
        {
        }

        protected override void OnActive(SubCondition condition, Storage value, DateTime timeStamp)
        {
            if (condition.SubAlarmType != ActiveSubCondition)
            {
                if (condition.SubAlarmType != _tempType)
                {
                    _timeStamp = timeStamp;
                    _tempType = condition.SubAlarmType;
                }
                if (_delay == 0 || (timeStamp - _timeStamp).TotalMilliseconds > _delay)
                {
                    if (ActiveSubCondition == SubAlarmType.None)
                    {
                        _active = true;
                        _condLastActive = timeStamp;
                    }
                    _ack = false;
                    ActiveSubCondition = condition.SubAlarmType;
                    _current.Duration = timeStamp - SubCondLastActive;
                    _current = new AlarmItem(timeStamp, condition.Message, _tag.GetValue(value), ActiveSubCondition, condition.Severity, _id, _source);
                    if (AlarmActive != null)
                    {
                        foreach (AlarmEventHandler deleg in AlarmActive.GetInvocationList())
                        {
                            deleg.BeginInvoke(this, _current, null, null);
                        }
                    }
                    RaiseChanged("Value");
                }
            }
            else
            {
                RaiseChanged("Value");
            }
        }

        protected override void OnInActive(Storage value)
        {
            if (ActiveSubCondition != SubAlarmType.None)
            {
                _active = false;
                ActiveSubCondition = SubAlarmType.None;
                _current.Duration = DateTime.Now - SubCondLastActive;
                _current = new AlarmItem(DateTime.Now, string.Concat("【", _current.AlarmText, "】", ALARMSTOP), _tag.GetValue(value), SubAlarmType.None, Severity.Normal, _id, _source);
                if (AlarmActive != null)
                {
                    foreach (AlarmEventHandler deleg in AlarmActive.GetInvocationList())
                    {
                        deleg.BeginInvoke(this, _current, null, null);
                    }
                }
            }
        }

        protected void RaiseChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            PropertyChanged = null;
            if (_tag != null)
                _tag.ValueChanging -= CheckAlarm;
        }

        protected abstract void CheckAlarm(object sender, ValueChangingEventArgs<Storage> e);

        public override AlarmType AlarmType
        {
            get
            {
                return AlarmType.None;
            }
        }

        public override bool AddSubCondition(SubCondition condition)
        {
            return true;
        }

        public override bool RemoveSubCondition(SubCondition condition)
        {
            return true;
        }
    }

    public sealed class ComplexCondition : ICondition, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public override string Value
        {
            get
            {
                return Convert.ToString(_current.AlarmValue);
            }
        }

        public ComplexCondition(int id, string source, string comment, float deadBand, int delay) :
            base(id, ConditionType.Absolute, source, comment, 0f, deadBand, delay)
        {
            _subConditions = new SubCondition[1]
                {
                    new SubCondition(SubAlarmType.Dsc)
                };
        }

        protected override void OnActive(SubCondition condition, Storage value, DateTime timeStamp)
        {
            if (condition.SubAlarmType != ActiveSubCondition)
            {
                if (condition.SubAlarmType != _tempType)
                {
                    _timeStamp = timeStamp;
                    _tempType = condition.SubAlarmType;
                }
                if (_delay == 0 || (timeStamp - _timeStamp).TotalMilliseconds > _delay)
                {
                    if (ActiveSubCondition == SubAlarmType.None)
                    {
                        _active = true;
                        _condLastActive = timeStamp;
                    }
                    _ack = false;
                    ActiveSubCondition = condition.SubAlarmType;
                    _current.Duration = timeStamp - SubCondLastActive;
                    _current = new AlarmItem(timeStamp, condition.Message, true, ActiveSubCondition, condition.Severity, _id, _source);
                    if (AlarmActive != null)
                    {
                        foreach (AlarmEventHandler deleg in AlarmActive.GetInvocationList())
                        {
                            deleg.BeginInvoke(this, _current, null, null);
                        }
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                }
            }
            else
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        protected override void OnInActive(Storage value)
        {
            if (ActiveSubCondition != SubAlarmType.None)
            {
                _active = false;
                ActiveSubCondition = SubAlarmType.None;
                _current.Duration = DateTime.Now - SubCondLastActive;
                _current = new AlarmItem(DateTime.Now, string.Concat("【", _current.AlarmText, "】", ALARMSTOP), false, SubAlarmType.None, Severity.Normal, _id, _source);
                if (AlarmActive != null)
                {
                    foreach (AlarmEventHandler deleg in AlarmActive.GetInvocationList())
                    {
                        deleg.BeginInvoke(this, _current, null, null);
                    }
                }
            }
        }

        public Action SetFunction(Delegate tagChanged)
        {
            var _func = tagChanged as Func<bool>;
            if (_func != null)
            {
                return delegate
                {
                    if (_enable)
                    {
                        SubCondition condition = _subConditions[0];
                        if (condition.IsEnabled)
                        {
                            if (_func())
                            {
                                OnActive(condition, Storage.Empty, DateTime.Now);
                                return;
                            }
                        }
                        OnInActive(Storage.Empty);
                    }
                };
            }
            else
                return null;
        }

        public override void Dispose()
        {
            base.Dispose();
            PropertyChanged = null;
        }

        public override AlarmType AlarmType
        {
            get
            {
                return AlarmType.Complex;
            }
        }

        public override bool AddSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.Dsc:
                    _subConditions[0] = condition;
                    return true;
                default:
                    return false;
            }
        }

        public override bool RemoveSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.Dsc:
                    _subConditions[0] = SubCondition.Empty;
                    return true;
                default:
                    return false;
            }
        }

    }

    public sealed class LevelAlarm : SimpleCondition
    {
        public override AlarmType AlarmType
        {
            get { return AlarmType.Level; }
        }

        public LevelAlarm(int id, string source, string comment, float deadBand = 0f, int delay = 0) :
            base(id, ConditionType.Absolute, source, comment, 0, deadBand, delay)
        {
            _subConditions = new SubCondition[4]
                {
                     new SubCondition(SubAlarmType.HiHi),
                     new SubCondition(SubAlarmType.High),
                     new SubCondition(SubAlarmType.LoLo),
                     new SubCondition(SubAlarmType.Low)
                };
        }

        protected override void CheckAlarm(object sender, ValueChangingEventArgs<Storage> e)
        {
            if (_enable)
            {
                SubCondition condition;
                float value = _tag.ScaleToValue(e.NewValue);
                for (int i = 0; i < _subConditions.Length; i++)
                {
                    if (_subConditions[i].IsEnabled)
                    {
                        condition = _subConditions[i];
                        if (i < 2)//Hi Alarm
                        {
                            if (value > condition.Threshold)
                            {
                                OnActive(condition, e.NewValue, e.NewTimeStamp);
                                return;
                            }
                            else if (_deadBand > 0 && ActiveSubCondition == condition.SubAlarmType && value > condition.Threshold - _deadBand)
                            {
                                return;
                            }
                        }
                        else//Low Alarm
                        {
                            if (value < condition.Threshold)
                            {
                                OnActive(condition, e.NewValue, e.NewTimeStamp);
                                return;
                            }
                            else if (_deadBand > 0 && ActiveSubCondition == condition.SubAlarmType && value > condition.Threshold + _deadBand)
                            {
                                return;
                            }
                        }
                    }
                }
                OnInActive(e.NewValue);
            }
        }

        public override bool AddSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.HiHi:
                    _subConditions[0] = condition;
                    return true;
                case SubAlarmType.High:
                    _subConditions[1] = condition;
                    return true;
                case SubAlarmType.LoLo:
                    _subConditions[2] = condition;
                    return true;
                case SubAlarmType.Low:
                    _subConditions[3] = condition;
                    return true;
                default:
                    return false;
            }
        }

        public override bool RemoveSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.HiHi:
                    _subConditions[0] = SubCondition.Empty;
                    return true;
                case SubAlarmType.High:
                    _subConditions[1] = SubCondition.Empty;
                    return true;
                case SubAlarmType.LoLo:
                    _subConditions[2] = SubCondition.Empty;
                    return true;
                case SubAlarmType.Low:
                    _subConditions[3] = SubCondition.Empty;
                    return true;
                default:
                    return false;
            }
        }
    }

    public sealed class DevAlarm : SimpleCondition
    {
        public override AlarmType AlarmType
        {
            get { return AlarmType.Dev; }
        }

        public DevAlarm(int id, ConditionType conditionType, string source, string comment, float para, float deadBand = 0f, int delay = 0) :
            base(id, conditionType, source, comment, para, deadBand, delay)
        {
            _subConditions = new SubCondition[2]
                { 
                    new SubCondition(SubAlarmType.MajDev),
                    new SubCondition(SubAlarmType.MinDev)          
                };
        }

        protected override void CheckAlarm(object sender, ValueChangingEventArgs<Storage> e)
        {
            if (_enable)
            {
                float value = _tag.ScaleToValue(e.NewValue) - _para;
                SubCondition condition;
                for (int i = 0; i < _subConditions.Length; i++)
                {
                    if (_subConditions[i].IsEnabled)
                    {
                        condition = _subConditions[i];
                        if (value > (_conditionType == ConditionType.Absolute ? condition.Threshold : _para * condition.Threshold))
                        {
                            OnActive(condition, e.NewValue, e.NewTimeStamp);
                            return;
                        }
                        else if (_deadBand > 0 && ActiveSubCondition == condition.SubAlarmType &&
                            ((_conditionType == ConditionType.Absolute && value > condition.Threshold - _deadBand)
                                || (_conditionType == ConditionType.Percent && value > _para * (condition.Threshold - _deadBand))))
                        {
                            return;
                        }
                    }
                }
                OnInActive(e.NewValue);
            }
        }

        public override bool AddSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.MajDev:
                    _subConditions[0] = condition;
                    return true;
                case SubAlarmType.MinDev:
                    _subConditions[1] = condition;
                    return true;
                default:
                    return false;
            }
        }

        public override bool RemoveSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.MajDev:
                    _subConditions[0] = SubCondition.Empty;
                    return true;
                case SubAlarmType.MinDev:
                    _subConditions[1] = SubCondition.Empty;
                    return true;
                default:
                    return false;
            }
        }
    }

    public sealed class DigitAlarm : SimpleCondition
    {
        public override AlarmType AlarmType
        {
            get { return AlarmType.Dsc; }
        }

        public DigitAlarm(int id, string source = null, string comment = null, int delay = 0) :
            base(id, ConditionType.Absolute, source, comment, 0, 0f, delay)
        {
            _subConditions = new SubCondition[1]
                {
                    new SubCondition(SubAlarmType.Dsc)
                };
        }

        protected override void CheckAlarm(object sender, ValueChangingEventArgs<Storage> e)
        {
            if (_enable)
            {
                SubCondition condition = _subConditions[0];
                if (condition.IsEnabled)
                {
                    if (e.NewValue.Boolean == condition.Threshold > 0)
                    {
                        OnActive(condition, e.NewValue, e.NewTimeStamp);
                        return;
                    }
                }
                OnInActive(e.NewValue);
            }
        }

        public override bool AddSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.Dsc:
                    _subConditions[0] = condition;
                    return true;
                default:
                    return false;
            }
        }

        public override bool RemoveSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.Dsc:
                    _subConditions[0] = SubCondition.Empty;
                    return true;
                default:
                    return false;
            }
        }
    }

    public sealed class WordDigitAlarm : SimpleCondition
    {
        public override AlarmType AlarmType
        {
            get { return AlarmType.WordDsc; }
        }

        public WordDigitAlarm(int id, string source = null, string comment = null, int delay = 0) :
            base(id, ConditionType.Absolute, source, comment, 0, 0f, delay)
        {
            _subConditions = new SubCondition[16];
            for (int i = 0; i < 16; i++)
            {
                _subConditions[i].SubAlarmType = SubAlarmType.Dsc;
                _subConditions[i].Threshold = i;
            };
        }

        protected override void CheckAlarm(object sender, ValueChangingEventArgs<Storage> e)
        {
            if (_enable)
            {
                short newvalue = e.NewValue.Int16;
                short oldvlaue = e.OldValue.Int16;
                if (newvalue == 0 && oldvlaue != 0)
                {
                    OnInActive(e.NewValue);
                    return;
                }
                for (short i = 0; i < _subConditions.Length; i++)
                {
                    SubCondition condition = _subConditions[i];
                    if (condition.IsEnabled)
                    {
                        int mask = 1 << i;
                        int newval = mask & newvalue;
                        int oldval = mask & oldvlaue;
                        if (newval != 0 && oldval == 0)
                        {
                            OnActive(condition, new Storage { Int16 = i }, e.NewTimeStamp);
                        }
                    }
                }
            }
        }

        public override bool AddSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.Dsc:
                    int index = (int)condition.Threshold;
                    if (index >= 0 && index < 16)
                        _subConditions[index] = condition;
                    return true;
                default:
                    return false;
            }
        }

        public override bool RemoveSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.Dsc:
                    int index = (int)condition.Threshold;
                    if (index >= 0 && index < 16)
                        _subConditions[index] = SubCondition.Empty;
                    return true;
                default:
                    return false;
            }
        }

        protected override void OnActive(SubCondition condition, Storage value, DateTime timeStamp)
        {
            if (ActiveSubCondition == SubAlarmType.None)
            {
                _active = true;
                _condLastActive = timeStamp;
            }
            _ack = false;
            ActiveSubCondition = condition.SubAlarmType;
            _current = new AlarmItem(timeStamp, condition.Message, value.Int16, ActiveSubCondition, condition.Severity, _id, _source);
            if (AlarmActive != null)
            {
                foreach (AlarmEventHandler deleg in AlarmActive.GetInvocationList())
                {
                    deleg.BeginInvoke(this, _current, null, null);
                }
            }
            RaiseChanged("Value");
        }

        protected override void OnInActive(Storage value)
        {
            if (ActiveSubCondition != SubAlarmType.None)
            {
                _active = false;
                ActiveSubCondition = SubAlarmType.None;
                _current.Duration = DateTime.Now - LastActive;
                _current = new AlarmItem(DateTime.Now, string.Concat("【", Comment, "】", ALARMSTOP), 0, SubAlarmType.None, Severity.Normal, _id, _source);
                if (AlarmActive != null)
                {
                    foreach (AlarmEventHandler deleg in AlarmActive.GetInvocationList())
                    {
                        deleg.BeginInvoke(this, _current, null, null);
                    }
                }
            }
        }
    }

    public sealed class QualitiesAlarm : SimpleCondition
    {
        public override AlarmType AlarmType
        {
            get { return AlarmType.Quality; }
        }

        public QualitiesAlarm(int id, string source, string comment, int delay = 0) :
            base(id, ConditionType.Absolute, source, comment, 0, 0f, delay)
        {
            _subConditions = new SubCondition[1]
                {
                    new SubCondition(SubAlarmType.BadPV)
                };
        }

        protected override void CheckAlarm(object sender, ValueChangingEventArgs<Storage> e)
        {
            if (_enable)
            {
                SubCondition condition = _subConditions[0];
                if (condition.IsEnabled)
                {
                    if (e.Quality != QUALITIES.QUALITY_GOOD)
                        OnActive(condition, e.NewValue, e.NewTimeStamp);
                    return;
                }
                OnInActive(e.NewValue);
            }
        }

        public override bool AddSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.BadPV:
                    _subConditions[0] = condition;
                    return true;
                default:
                    return false;
            }
        }

        public override bool RemoveSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.BadPV:
                    _subConditions[0] = SubCondition.Empty;
                    return true;
                default:
                    return false;
            }
        }
    }

    public sealed class ROCAlarm : SimpleCondition
    {
        public override AlarmType AlarmType
        {
            get { return AlarmType.ROC; }
        }


        public ROCAlarm(int id, string souce, string comment, float deadBand = 0f, int delay = 0) :
            base(id, ConditionType.Percent, souce, comment, 0, deadBand, delay)
        {
            _subConditions = new SubCondition[2]
                {
                    new SubCondition(SubAlarmType.MajROC),
                    new SubCondition(SubAlarmType.MinROC)
                };
        }

        protected override void CheckAlarm(object sender, ValueChangingEventArgs<Storage> e)
        {
            if (_enable)
            {
                float value = (float)((_tag.ScaleToValue(e.NewValue) - e.OldValue.Single) / (e.NewTimeStamp - e.OldTimeStamp).TotalMilliseconds);
                SubCondition condition;
                for (int i = 0; i < _subConditions.Length; i++)
                {
                    if (_subConditions[i].IsEnabled)
                    {
                        condition = _subConditions[i];
                        if (value > condition.Threshold)
                        {
                            OnActive(condition, e.NewValue, e.NewTimeStamp);
                            return;
                        }
                        else if (_deadBand > 0 && ActiveSubCondition == condition.SubAlarmType && value > condition.Threshold - _deadBand)
                        {
                            return;
                        }
                    }
                }
                OnInActive(e.NewValue);
            }
        }

        public override bool AddSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.MajROC:
                    _subConditions[0] = condition;
                    return true;
                case SubAlarmType.MinROC:
                    _subConditions[1] = condition;
                    return true;
                default:
                    return false;
            }
        }

        public override bool RemoveSubCondition(SubCondition condition)
        {
            switch (condition.SubAlarmType)
            {
                case SubAlarmType.MajROC:
                    _subConditions[0] = SubCondition.Empty;
                    return true;
                case SubAlarmType.MinROC:
                    _subConditions[1] = SubCondition.Empty;
                    return true;
                default:
                    return false;
            }
        }
    }

    public struct SubCondition
    {
        public bool IsEnabled;

        public SubAlarmType SubAlarmType;

        public Severity Severity;

        public float Threshold;

        public string Message;

        public SubCondition(SubAlarmType type, float threshold = 0f, Severity severity = Severity.Normal, string message = "", bool enabled = true)
        {
            this.SubAlarmType = type;
            this.Threshold = threshold;
            this.Severity = severity;
            this.Message = message;
            this.IsEnabled = enabled;
        }

        public static readonly SubCondition Empty = new SubCondition(SubAlarmType.None, 0f, Severity.Normal, "正常", false);
    }

    public class CompareCondBySource : IComparer<ICondition>
    {
        public int Compare(ICondition x, ICondition y)
        {
            if (x == null || x.Source == null)
            {
                return y == null || y.Source == null ? 0 : 1;
            }
            else
            {
                return y == null || y.Source == null ? 1 : x.Source.CompareTo(y.Source);
            }
        }
    }
}
