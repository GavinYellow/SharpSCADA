using System;
using System.ComponentModel;

namespace DataService
{
   
    public class AlarmItem : IComparable<AlarmItem>, INotifyPropertyChanged
    {
        int _condiId;
       
        Severity _severity;
        SubAlarmType _alarmType;
        DateTime _startTime;
        TimeSpan _duration;       
        object _alarmValue;
        string _alarmText;
        string _source;

        public SubAlarmType SubAlarmType
        {
            get
            {
                return _alarmType;
            }
            set
            {
                _alarmType = value;
            }
        }

        public Severity Severity
        {
            get
            {
                return _severity;
            }
            set
            {
                _severity = value;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                _startTime = value;
            }
        }

        public int ConditionId
        {
            get
            {
                return _condiId;
            }
            set
            {
                _condiId = value;
            }
        }

        public TimeSpan Duration
        {
            get
            {
                //return _endTime-_startTime;
                return _duration;
            }
            set
            {
                _duration = value;
                OnPropertyChanged("Duration");
            }
        }

        public object AlarmValue
        {
            get
            {
                return _alarmValue;
            }
            set
            {
                _alarmValue = value;
            }
        }

        public string AlarmText
        {
            get
            {
                return _alarmText;
            }
            set
            {
                _alarmText = value;
            }
        }

        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        public AlarmItem(DateTime time, string alarmText, object alarmValue, SubAlarmType type, Severity severity, int condId, string source)
        {
            this._startTime = time;
            this._alarmType = type;
            this._alarmText = alarmText;
            this._alarmValue = alarmValue;
            this._severity = severity;
            this._condiId = condId;
            this._source = source;
        }

        public AlarmItem()
        {
            this._startTime = DateTime.Now;
            this._alarmType = SubAlarmType.None;
            this._alarmText = string.Empty;
            this._severity = Severity.Normal;
            this._condiId = -1;
            this._source = string.Empty;
        }

        #region IComparable<AlarmItem> Members

        public int CompareTo(AlarmItem other)
        {
            return this._startTime.CompareTo(other._startTime);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {

                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

}
