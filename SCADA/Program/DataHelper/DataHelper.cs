using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DatabaseLib
{
    public static class DataHelper
    {
        static string m_ConnStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=SharpSCADA;Integrated Security=True";
        static string m_Path = @"D:\HDA";
        static string m_host = Environment.MachineName;
        static string m_type = "MSSQL";
        //数据库工厂接口  
        const string CFGPATH = @"C:\DataConfig\host.cfg";
        const string INIPATH = @"C:\DataConfig\host.ini";
        const string DATALOGSOURCE = "Data Operations";
        const string DATALOGNAME = "Data Log";
        const int STRINGMAX = 255;

        static EventLog Log;
        #region GetInstance
        private static IDataFactory _ins;

        public static IDataFactory Instance
        {
            get
            {
                return _ins;
            }
        }

        public static string HostName
        {
            get { return m_host; }
        }

        public static string ConnectString
        {
            get { return m_ConnStr; }
        }

        public static string HdaPath
        {
            get { return m_Path; }
        }
        #endregion
        /// <summary>  
        /// 数据库工厂构造函数  
        /// </summary>  
        /// <param name="dbtype">数据库枚举</param>  
        static DataHelper()
        {
            if (!EventLog.SourceExists(DATALOGSOURCE))
                EventLog.CreateEventSource(DATALOGSOURCE, DATALOGNAME);
            Log = new EventLog(DATALOGNAME);
            try
            {
                if (File.Exists(INIPATH))
                {
                    StringBuilder sb = new StringBuilder(STRINGMAX);
                    WinAPI.GetPrivateProfileString("HOST", "SERVER", m_host, sb, STRINGMAX, INIPATH);
                    m_host = sb.ToString();
                    WinAPI.GetPrivateProfileString("DATABASE", "CONNSTRING", m_ConnStr, sb, STRINGMAX, INIPATH);
                    m_ConnStr = sb.ToString();
                    WinAPI.GetPrivateProfileString("DATABASE", "ARCHIVE", m_Path, sb, STRINGMAX, INIPATH);
                    m_Path = sb.ToString();
                    WinAPI.GetPrivateProfileString("DATABASE", "TYPE", m_type, sb, STRINGMAX, INIPATH);
                    m_type = sb.ToString();
                }
                else if (File.Exists(CFGPATH))
                {
                    using (StreamReader objReader = new StreamReader(CFGPATH))
                    {
                        m_host = objReader.ReadLine();
                        m_ConnStr = objReader.ReadLine();
                        m_Path = objReader.ReadLine();
                    }
                }
                IPAddress addr;
                if (string.IsNullOrEmpty(m_host) || !IPAddress.TryParse(m_host, out addr))
                {
                    m_host = Environment.MachineName;
                }
                switch (m_type.ToUpper())
                {
                    case "MSSQL":
                        _ins = new MssqlFactory();
                        break;
                    case "MYSQL":
                        _ins = new MysqlFactory();
                        break;
                    default:
                        _ins = new MssqlFactory();
                        break;
                }
            }
            catch (Exception e)
            {
                AddErrorLog(e);
            }
        }

        public static DbParameter CreateParam(string paramName, SqlDbType dbType, object objValue, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            return _ins.CreateParam(paramName, dbType, objValue, size, direction);
        }

        public static string DataTableToCsv(DataTable table)
        {
            //以半角逗号（即,）作分隔符，列为空也要表达其存在。  
            //列内容如存在半角逗号（即,）则用半角引号（即""）将该字段值包含起来。  
            //列内容如存在半角引号（即"）则应替换成半角双引号（""）转义，并用半角引号（即""）将该字段值包含起来。  
            StringBuilder sb = new StringBuilder();
            DataColumn colum;
            foreach (DataRow row in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    colum = table.Columns[i];
                    if (i != 0) sb.Append(",");
                    var txt = row[colum] == null ? "" : row[colum].ToString();
                    if (colum.DataType == typeof(string) && txt.Contains(","))
                    {
                        sb.Append("\"" + txt.Replace("\"", "\"\"") + "\"");
                    }
                    else sb.Append(txt);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string ReaderToCsv(IDataReader reader)
        {  
            StringBuilder sb = new StringBuilder();
            var colcount = reader.FieldCount;
            while (reader.Read())
            {
                for (int i = 0; i < colcount; i++)
                {
                    if (i != 0) sb.Append(",");
                    var txt = reader[i] == null ? "" : reader[i].ToString();
                    if (txt.Contains(","))
                    {
                        sb.Append("\"" + txt.Replace("\"", "\"\"") + "\"");
                    }
                    else sb.Append(txt);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static void AddErrorLog(Exception e)
        {
            string err = "";
            Exception exp = e;
            while (exp != null)
            {
                err += string.Format("\n {0}", exp.Message);
                exp = exp.InnerException;
            }
            err += string.Format("\n {0}", e.StackTrace);
            Log.Source = DATALOGSOURCE;
            Log.WriteEntry(err, EventLogEntryType.Error);
        }

        public static string GetNullableString(this DbDataReader reader, int index)
        {
            SqlDataReader dataReader = reader as SqlDataReader;
            if (dataReader != null)
            {
                var svr = dataReader.GetSqlString(index);
                return svr.IsNull ? null : svr.Value;
            }
            else return reader.GetString(index);
        }

        public static DateTime? GetNullableTime(this DbDataReader reader, int index)
        {
            SqlDataReader dataReader = reader as SqlDataReader;
            if (dataReader == null) return reader.GetDateTime(index);
            var svr = dataReader.GetSqlDateTime(index);
            return svr.IsNull ? default(Nullable<DateTime>) : svr.Value;
        }

        public static int GetTimeTick(this DbDataReader reader, int index)
        {
            SqlDataReader dataReader = reader as SqlDataReader;
            if (dataReader != null)
            {
                return dataReader.GetSqlDateTime(index).TimeTicks;
            }
            var datetime = reader.GetDateTime(index);
            var value = datetime.Subtract(new DateTime(1900, 1, 1));
            long num2 = value.Ticks - value.Days * 864000000000;
            if (num2 < 0)
                num2 += 864000000000;
            int num3 = (int)(num2 / 10000.0 * 0.3 + 0.5);
            if (num3 > 300 * 60 * 60 * 24 - 1)
                num3 = 0;
            return num3;
        }

        public static object GetSqlValue(this DbDataReader reader, int index)
        {
            SqlDataReader dataReader = reader as SqlDataReader;
            if (dataReader != null)
            {
                return dataReader.GetSqlValue(index);
            }
            var mq = reader as MySqlDataReader;
            if (mq != null)
            {
                return mq.GetValue(index);
            }
            return "";
        }
    }


    public static class WinAPI
    {

        //参数说明：section：INI文件中的段落；key：INI文件中的关键字；val：INI文件中关键字的数值；filePath：INI文件的完整的路径和名称。
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filepath);

        //参数说明：section：INI文件中的段落名称；key：INI文件中的关键字；def：无法读取时候时候的缺省数值；retVal：读取数值；size：数值的大小；filePath：INI文件的完整路径和名称。
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);
    }
}