using System;
using System.Collections.Generic;
namespace TagConfig
{
    public class Scaling : IComparable<Scaling>
    {
        short _id;
        byte _type;
        float _euHi, _euLo, _rawHi, _rawLo;

        public short ID
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

        public byte ScaleType
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public float EUHi
        {
            get
            {
                return _euHi;
            }
            set
            {
                _euHi = value;
            }
        }

        public float EULo
        {
            get
            {
                return _euLo;
            }
            set
            {
                _euLo = value;
            }
        }

        public float RawHi
        {
            get
            {
                return _rawHi;
            }
            set
            {
                _rawHi = value;
            }
        }

        public float RawLo
        {
            get
            {
                return _rawLo;
            }
            set
            {
                _rawLo = value;
            }
        }

        public Scaling(short id, byte type, float euHi, float euLo, float rawHi, float rawLo)
        {
            ID = id;
            _type = type;
            _euHi = euHi;
            _euLo = euLo;
            _rawHi = rawHi;
            _rawLo = rawLo;
        }

        public int CompareTo(Scaling other)
        {
            return ID.CompareTo(other.ID);
        }

    }


    //TypeID,SourceID,AlarmType,EventType,ConditionType,Para,IsEnabled,Deadband,Delay,Comment
    public class Condition 
    {
        bool _enable;
        byte _eventType, _condType;
        string _source;
        int _typeId, _alarmType, _delay;
        float _para, _deadBand;
        string _comment;
        List<SubCondition> _subConds = new List<SubCondition>();

        public Condition(int typeId, string source, int alarmType, byte eventType, byte condType, float para, bool enable, float deadband, int delay, string comment = null)
        {
            _typeId = typeId;
            _source = source;
            _alarmType = alarmType;
            _enable = enable;
            _eventType = eventType;
            _condType = condType;
            _delay = delay;
            _para = para;
            _deadBand = deadband;
            _comment = comment;
        }

        public Condition(string source)
        {
            _source = source;
        }

        public int TypeId
        {
            get
            {
                return _typeId;
            }
            set
            {
                _typeId = value;
            }
        }

        public int AlarmType
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

        public bool IsEnabled
        {
            get
            {
                return _enable;
            }
            set
            {
                _enable = false;
            }
        }

        public byte EventType
        {
            get
            {
                return _eventType;
            }
            set
            {
                _eventType = value;
            }
        }

        public byte ConditionType
        {
            get
            {
                return _condType;
            }
            set
            {
                _condType = value;
            }
        }

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

        public List<SubCondition> SubConditions
        {
            get
            {
                return _subConds;
            }
        }

        public override string ToString()
        {
            return _source;
        }
    }

    public class SubCondition : IComparable<SubCondition>
    {
        bool _enabled;
        int _severity, _condId, _subType;
        float _threshold;
        string _message;

        public SubCondition(bool enable, int severity, int condId, int subType, float threshold, string message = null)
        {
            _enabled = enable;
            _severity = severity;
            _condId = condId;
            _subType = subType;
            _threshold = threshold;
            _message = message;
        }

        public SubCondition(int condId)
        {
            _condId = condId;
        }

        public int ConditionId
        {
            get
            {
                return _condId;
            }
            set
            {
                _condId = value;
            }
        }


        public int SubAlarmType
        {
            get
            {
                return _subType;
            }
            set
            {
                _subType = value;
            }
        }

        public float Threshold
        {
            get
            {
                return _threshold;
            }
            set
            {
                _threshold = value;
            }
        }


        public int Severity
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

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        public SubCondition(int condId, int type, float threshold, byte severity, string message, bool enabled)
        {
            _condId = condId;
            _subType = type;
            _threshold = threshold;
            _severity = severity;
            _message = message;
            _enabled = enabled;
        }

        public int CompareTo(SubCondition other)
        {
            return _condId.CompareTo(other._condId);
        }
    }
}
