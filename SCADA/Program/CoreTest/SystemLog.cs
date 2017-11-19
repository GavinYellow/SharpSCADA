using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DatabaseLib;
using DataService;

namespace CoreTest
{
    public class SystemLog : IEvent
    {
        public bool IsEnabled
        {
            get;
            set;
        }

        public bool IsAcked
        {
            get;
            set;
        }

        public bool IsActived
        {
            get;
            set;
        }

        Severity _severity;
        public Severity Severity
        {
            get { return _severity; }
        }

        EventType _eventtype;
        public EventType EventType
        {
            get { return EventType.Simple; }
        }

        DateTime _time;
        public DateTime LastActive
        {
            get { return _time; }
            set { _time = value; }
        }

        string _comment;
        public string Comment
        {
            get { return _comment; }
        }

        string _source;
        public string Source
        {
            get { return _source; }
        }

        public SystemLog(EventType eventtype, DateTime time, string source = null, string comment = null, Severity severity = Severity.Normal)
        {
            _eventtype = eventtype;
            _time = time;
            _severity = severity;
            _source = source;
            _comment = comment;
        }

        public int Save()
        {
            return AddLog(this);
        }

        public static int AddLog(SystemLog log)
        {
            string sql = string.Format("INSERT INTO dbo.LOG_EVENT(EVENTTYPE,SEVERITY,ACTIVETIME,SOURCE,COMMENT) VALUES({0},{1},'{2}','{3}','{4}');",
               (int)log._eventtype, (int)log._severity, log._time, log._source, log._comment);
            return DataHelper.Instance.ExecuteNonQuery(sql);
        }

        public static SystemLog FindFirstEvent(EventType eventtype, DateTime? firsttime = null, string source = null)
        {
            string cond1 = firsttime == null ? "" : " AND ACTIVETIME>='" + firsttime + "'";
            string cond2 = string.IsNullOrEmpty(source) ? "" : " AND SOURCE='" + source + "'";
            string sql = string.Format("SELECT TOP 1 SEVERITY,ACTIVETIME,COMMENT FROM dbo.LOG_EVENT WHERE EVENTTYPE={0} {1} {2} ORDER BY ACTIVETIME",
               (int)eventtype, cond1, cond2);
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    return new SystemLog(eventtype, reader.GetDateTime(1), source, reader.GetNullableString(2), (Severity)reader.GetInt32(0));
                }
            }
            return null;
        }

        public static SystemLog FindLastEvent(EventType eventtype, DateTime? lasttime = null, string source = null)
        {
            string cond1 = lasttime == null ? "" : " AND ACTIVETIME<='" + lasttime + "'";
            string cond2 = string.IsNullOrEmpty(source) ? "" : " AND SOURCE='" + source + "'";
            string sql = string.Format("SELECT TOP 1 SEVERITY,ACTIVETIME,COMMENT FROM dbo.LOG_EVENT WHERE EVENTTYPE={0} {1} {2} ORDER BY ACTIVETIME DESC",
               (int)eventtype, cond1, cond2);
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    return new SystemLog(eventtype, reader.GetDateTime(1), source, reader.GetNullableString(2), (Severity)reader.GetInt32(0));
                }
            }
            return null;
        }

        public static List<SystemLog> FindEvents(EventType eventtype, string source = null, DateTime? startime = null, DateTime? endtime = null)
        {
            List<SystemLog> list = new List<SystemLog>();
            string cond1 = string.IsNullOrEmpty(source) ? "" : " AND SOURCE='" + source + "'";
            string cond2 = startime == null ? "" : " AND ACTIVETIME>='" + startime + "'";
            string cond3 = endtime == null ? "" : " AND ACTIVETIME<='" + endtime + "'";
            string sql = string.Format("SELECT SEVERITY,ACTIVETIME,COMMENT FROM dbo.LOG_EVENT WHERE EVENTTYPE={0} {1} {2} {3} ORDER BY ACTIVETIME",
               (int)eventtype, cond1, cond2, cond3);
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    list.Add(new SystemLog(eventtype, reader.GetDateTime(1), source, reader.GetNullableString(2), (Severity)reader.GetInt32(0)));
                }
            }
            return list;
        }

        public static void GetEventTime(EventType eventtype, string source, string comment, out DateTime? startime, out DateTime? endtime)
        {
            var parm1 = DataHelper.CreateParam("@STARTTIME", SqlDbType.DateTime, null, 0, ParameterDirection.Output);
            var parm2 = DataHelper.CreateParam("@ENDTIME", SqlDbType.DateTime, null, 0, ParameterDirection.Output);
            if (DataHelper.Instance.ExecuteStoredProcedure("GetEventTime",
                 DataHelper.CreateParam("@EVENTTYPE", SqlDbType.Int, (int)eventtype),
                 DataHelper.CreateParam("@SOURCE", SqlDbType.NVarChar, source, 50),
                 DataHelper.CreateParam("@COMMENT", SqlDbType.NVarChar, comment, 50),
               parm1, parm2) >= 0)
            {
                if (parm1.Value == DBNull.Value)
                    startime = null;
                else startime = (DateTime)parm1.Value;
                if (parm2.Value == DBNull.Value)
                    endtime = DateTime.Now;
                else endtime = (DateTime)parm2.Value;
            }
            else
                startime = endtime = null;
        }
    }
}

