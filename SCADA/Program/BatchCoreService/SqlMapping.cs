using System;
using System.Collections.Generic;
using System.Data;
using DataService;

namespace BatchCoreService
{
    internal class AlarmDataReader : IDataReader
    {
        IEnumerator<AlarmItem> _enumer;

        public AlarmDataReader(IEnumerable<AlarmItem> list)
        {
            this._enumer = list.GetEnumerator();
        }

        #region IDataReader Members

        public void Close()
        {

        }
        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            DataTable table = new DataTable("AlarmItem");
            table.Columns.Add("StartTime", typeof(DateTime));
            table.Columns.Add("Source", typeof(string));
            table.Columns.Add("ConditionId", typeof(int));
            table.Columns.Add("AlarmText", typeof(string));
            table.Columns.Add("AlarmValue", typeof(object));
            table.Columns.Add("Duration", typeof(int));
            table.Columns.Add("Severity", typeof(int));
            table.Columns.Add("SubAlarmType", typeof(int));
            return table;
        }
        public bool IsClosed
        {
            get { return false; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return _enumer.MoveNext();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return 8; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            switch (i)
            {
                case 0:
                    return typeof(DateTime);
                case 1:
                    return typeof(string);
                case 2:
                    return typeof(int);
                case 3:
                    return typeof(string);
                case 4:
                    return typeof(object);
                case 5:
                    return typeof(int);
                case 6:
                    return typeof(int);
                case 7:
                    return typeof(int);
                default:
                    return typeof(string);
            }
        }

        public float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }
        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            switch (i)
            {
                case 0:
                    return "StartTime";
                case 1:
                    return "Source";
                case 2:
                    return "ConditionId";
                case 3:
                    return "AlarmText";
                case 4:
                    return "AlarmValue";
                case 5:
                    return "Duration";
                case 6:
                    return "Severity";
                case 7:
                    return "SubAlarmType";
                default:
                    return "";
            }
        }

        public int GetOrdinal(string name)
        {
            switch (name)
            {
                case "StartTime":
                    return 0;
                case "Source":
                    return 1;
                case "ConditionId":
                    return 2;
                case "AlarmText":
                    return 3;
                case "AlarmValue":
                    return 4;
                case "Duration":
                    return 5;
                case "Severity":
                    return 6;
                case "SubAlarmType":
                    return 7;
                default:
                    return -1;
            }
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return _enumer.Current.StartTime;
                case 1:
                    return _enumer.Current.Source;
                case 2:
                    return _enumer.Current.ConditionId;
                case 3:
                    return _enumer.Current.AlarmText;
                case 4:
                    return _enumer.Current.AlarmValue;
                case 5:
                    return _enumer.Current.Duration.Seconds;
                case 6:
                    return _enumer.Current.Severity;
                case 7:
                    return _enumer.Current.SubAlarmType;
                default:
                    return null;
            }
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            switch (i)
            {
                case 0:
                    return _enumer.Current.StartTime == DateTime.MinValue;
                case 1:
                    return string.IsNullOrEmpty(_enumer.Current.Source);
                case 2:
                    return _enumer.Current.ConditionId == 0;
                case 3:
                    return string.IsNullOrEmpty(_enumer.Current.AlarmText);
                case 4:
                    return _enumer.Current.AlarmValue == null;
                case 5:
                case 6:
                case 7:
                default:
                    return false;
            }
        }

        public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        #endregion
    }

    internal class HDASqlReader : IDataReader
    {
        IEnumerator<HistoryData> _enumer;
        IDataServer _server;

        public HDASqlReader(IEnumerable<HistoryData> list, IDataServer server)
        {
            this._enumer = list.GetEnumerator();
            _server = server;
        }

        #region IDataReader Members

        public void Close()
        {

        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            DataTable table = new DataTable("Log_HData");
            table.Columns.Add("ID", typeof(short));
            table.Columns.Add("TimeStamp", typeof(DateTime));
            table.Columns.Add("Value", typeof(float));
            return table;
        }
        public bool IsClosed
        {
            get { return false; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return _enumer.MoveNext();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return 3; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            return this;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            switch (i)
            {
                case 0:
                    return typeof(Int16);
                case 1:
                    return typeof(DateTime);
                case 2:
                    return typeof(Single);
                default:
                    return typeof(Int32);
            }
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }
        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            switch (i)
            {
                case 0:
                    return "ID";
                case 1:
                    return "TimeStamp";
                case 2:
                    return "Value";
                default:
                    return string.Empty;
            }
        }

        public int GetOrdinal(string name)
        {
            switch (name)
            {
                case "ID":
                    return 0;
                case "TimeStamp":
                    return 1;
                case "Value":
                    return 2;
                default:
                    return -1;
            }
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return _enumer.Current.ID;
                case 1:
                    return _enumer.Current.TimeStamp;
                case 2:
                    var index = _server.GetItemProperties(_enumer.Current.ID);
                    if (index < 0) return 0f;
                    switch (_server.MetaDataList[index].DataType)
                    {
                        case DataType.FLOAT:
                            var ff = _enumer.Current.Value.Single;
                            return ff > -2E-38 && ff < 2E-38 ? 0f : ff;
                        case DataType.BOOL:
                            return _enumer.Current.Value.Boolean ? 1f : 0f;
                        case DataType.DWORD:
                            return _enumer.Current.Value.DWord;
                        case DataType.INT:
                            return _enumer.Current.Value.Int32;
                        case DataType.WORD:
                            return _enumer.Current.Value.Word;
                        case DataType.SHORT:
                            return _enumer.Current.Value.Int16;
                        case DataType.BYTE:
                            return _enumer.Current.Value.Byte;
                        default:
                            return 0f;
                    }
                default:
                    return 0f; ;
            }
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return false;
        }

        public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        #endregion
    }
}
