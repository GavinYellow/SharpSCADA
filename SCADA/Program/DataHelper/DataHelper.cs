using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DatabaseLib
{

    public class DataHelper //:MarshalByRefObject
    {
        static string m_ConnStr = @"Data Source=.\SQLEXPRESS;Initial Catalog=SharpSCADA;Integrated Security=True";
        static string m_Path = @"D:\HDA";
        static string m_host = Environment.MachineName;

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

        const string CFGPATH = @"C:\DataConfig\host.cfg";
        const string INIPATH = @"C:\DataConfig\host.ini";
        const string DATALOGSOURCE = "Data Operations";
        const string DATALOGNAME = "Data Log";
        const int STRINGMAX = 255;

        static EventLog Log;

        static DataHelper()
        {
            if (!EventLog.SourceExists(DATALOGSOURCE))
                EventLog.CreateEventSource(DATALOGSOURCE, DATALOGNAME);
            Log = new EventLog(DATALOGNAME);
            try
            {
                if (File.Exists(CFGPATH))
                {
                    using (StreamReader objReader = new StreamReader(CFGPATH))
                    {
                        m_host = objReader.ReadLine();
                        m_ConnStr = objReader.ReadLine();
                        m_Path = objReader.ReadLine();
                    }
                }
                else if (File.Exists(INIPATH))
                {
                    StringBuilder sb = new StringBuilder(STRINGMAX);
                    WinAPI.GetPrivateProfileString("HOST", "SERVER", m_host, sb, STRINGMAX, INIPATH);
                    m_host = sb.ToString();
                    WinAPI.GetPrivateProfileString("DATABASE", "CONNSTRING", m_ConnStr, sb, STRINGMAX, INIPATH);
                    m_ConnStr = sb.ToString();
                    WinAPI.GetPrivateProfileString("DATABASE", "ARCHIVE", m_Path, sb, STRINGMAX, INIPATH);
                    m_Path = sb.ToString();
                }
                IPAddress addr;
                if (string.IsNullOrEmpty(m_host) || !IPAddress.TryParse(m_host, out addr))
                {
                    m_host = Environment.MachineName;
                }
            }
            catch (Exception e)
            {
                AddErrorLog(e);
            }
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

        public static void CallException(string message)
        {
            AddErrorLog(new Exception(message));
        }

        public static bool ConnectionTest()
        {
            //创建连接对象
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                //mySqlConnection.ConnectionTimeout = 1;//设置连接超时的时间
                try
                {
                    //Open DataBase
                    //打开数据库
                    m_Conn.Open();
                    if (m_Conn.State == ConnectionState.Open)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    CallException(e.Message);
                }
            }
            //mySqlConnection   is   a   SqlConnection   object 
            return false;
        }

        public static SqlParameter CreateInputParam(string paramName, SqlDbType dbType, object objValue)
        {
            SqlParameter parameter = new SqlParameter(paramName, dbType);
            if (objValue == null)
            {
                parameter.IsNullable = true;
                parameter.Value = DBNull.Value;
                return parameter;
            }
            parameter.Value = objValue;
            return parameter;
        }

        #region ExecuteDataset  //执行查询语句，返回一个记录集

        /// <summary>
        /// 返回记录集
        /// </summary>
        /// <param name="SQL">用于返回记录集的SQL语句</param>
        /// <returns>记录集</returns>
        public static DataSet ExecuteDataset(string SQL)
        {
            DataSet ds = new DataSet();
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                try
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    SqlCommand cmd = new SqlCommand(SQL, m_Conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                }
                catch (Exception e)
                {
                    CallException(SQL + "        " + e.Message);
                }
            }
            return ds;
        }


        /// <summary>
        /// 返回记录集
        /// </summary>
        /// <param name="SQL">用于返回记录集的SQL语句</param>
        /// <param name="TableName">映射表名</param>
        /// <returns>记录集</returns>
        public static DataSet ExecuteDataset(string SQL, string TableName)
        {
            DataSet ds = new DataSet();
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                try
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    SqlCommand cmd = new SqlCommand(SQL, m_Conn);
                    da.SelectCommand = cmd;
                    da.Fill(ds, TableName);
                }
                catch (Exception e)
                {
                    CallException(SQL + "        " + e.Message);
                }
            }
            return ds;
        }

        /// <summary>
        /// 返回包含多个表的记录集
        /// </summary>
        /// <param name="SQLs">用于返回记录集的SQL语句</param>
        /// <param name="TableNames">映射表名</param>
        /// <returns>记录集</returns>

        public static DataSet ExecuteDataset(string[] SQLs, string[] TableNames)
        {
            DataSet ds = new DataSet();
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                try
                {
                    for (int i = 0; i < SQLs.Length; i++)
                    {

                        SqlDataAdapter da = new SqlDataAdapter();
                        SqlCommand cmd = new SqlCommand(SQLs[i], m_Conn);
                        da.SelectCommand = cmd;
                        da.Fill(ds, TableNames[i]);
                    }
                }
                catch (Exception e)
                {
                    CallException(SQLs + "        " + e.Message);
                }
            }
            return ds;
        }

        #endregion ExecuteDataset

        /// <summary>
        /// 返回表
        /// </summary>
        /// <param name="SQL">用于返回记录集的SQL语句</param>
        /// <returns>记录集</returns>
        public static DataTable ExecuteDataTable(string SQL)
        {
            DataTable dt = new DataTable();
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                try
                {
                    SqlDataAdapter da = new SqlDataAdapter();
                    SqlCommand cmd = new SqlCommand(SQL, m_Conn);
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                }
                catch (Exception e)
                {
                    CallException(SQL + "        " + e.Message);
                }
            }
            return dt;
        }

        #region ExecuteNonQuery //执行非查询语句

        /// <summary>
        /// 执行一条INSERT、UPDATE、DELETE语句
        /// </summary>
        /// <param name="SQL">T-SQL语句</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(string SQL)
        {
            int res = -1;
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                SqlTransaction sqlT = null;
                try
                {
                    using (SqlCommand cmd = new SqlCommand(SQL, m_Conn))
                    {
                        if (m_Conn.State == ConnectionState.Closed)
                            m_Conn.Open();
                        cmd.Connection = m_Conn;
                        sqlT = m_Conn.BeginTransaction();
                        cmd.Transaction = sqlT;
                        res = cmd.ExecuteNonQuery();
                        sqlT.Commit();
                    }
                }
                catch (Exception e)
                {
                    if (sqlT != null)
                        sqlT.Rollback();
                    CallException(SQL + "   " + e.Message);
                    return -1;
                }
                return res;
            }
        }

        /// <summary>
        /// 执行一组INSERT、UPDATE、DELETE语句
        /// </summary>
        /// <param name="SQLs">T-SQL语句</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(string[] SQLs)
        {
            int res = -1;
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                SqlTransaction sqlT = null;
                SqlCommand cmd = new SqlCommand();
                try
                {
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    cmd.Connection = m_Conn;
                    sqlT = m_Conn.BeginTransaction();
                    cmd.Transaction = sqlT;
                    for (int i = 0; i < SQLs.Length; i++)
                    {
                        cmd.CommandText = SQLs[i];
                        res = cmd.ExecuteNonQuery();
                    }
                    sqlT.Commit();
                }
                catch (Exception e)
                {
                    if (sqlT != null)
                        sqlT.Rollback();
                    CallException(SQLs + "        " + e.Message);
                    res = -1;
                }
                return res;
            }
        }

        /// <summary>
        /// 执行一组INSERT、UPDATE、DELETE语句
        /// </summary>
        /// <param name="SQLs">T-SQL语句</param>
        /// <param name="Pars">执行参数</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(string[] SQLs, object[][] Pars)
        {
            int res = -1;
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                SqlTransaction sqlT = null;
                SqlCommand cmd = new SqlCommand();
                try
                {
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    cmd.Connection = m_Conn;
                    sqlT = m_Conn.BeginTransaction();
                    cmd.Transaction = sqlT;
                    for (int i = 0; i < SQLs.Length; i++)
                    {
                        cmd.CommandText = SQLs[i];
                        cmd.Parameters.Clear();
                        for (int j = 0; j < Pars[i].Length; j++)
                        {
                            cmd.Parameters.AddWithValue("@p" + j.ToString(), Pars[i][j]);
                        }
                        res = cmd.ExecuteNonQuery();
                    }
                    sqlT.Commit();
                }
                catch (Exception e)
                {
                    if (sqlT != null)
                        sqlT.Rollback();
                    CallException(SQLs + "        " + e.Message);
                    res = -1;
                }
                return res;
            }
        }

        #endregion ExecuteNonQuery

        #region FillDataSet //填充一个记录集

        /// <summary>
        /// 用指定的SQL语句来填充一个记录集
        /// </summary>
        /// <param name="ds">记录集</param>
        /// <param name="SQL">SELECT语句</param>
        /// <param name="TableName">映射表名</param>
        public static void FillDataSet(ref DataSet ds, string SQL, string TableName)
        {
            try
            {
                SqlConnection m_Conn;
                m_Conn = new SqlConnection(m_ConnStr);
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(SQL, m_Conn);
                da.SelectCommand = cmd;
                da.Fill(ds, TableName);
            }
            catch (Exception e)
            {
                CallException(SQL + "        " + e.Message);
            }
        }

        #endregion FillDataSet

        #region
        // <summary>
        /// 返回一个SqlDataReader
        /// </summary>
        public static SqlDataReader ExecuteReader(string sSQL)
        {
            SqlConnection connection = new SqlConnection(m_ConnStr);
            SqlCommand command = new SqlCommand(sSQL, connection);
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }


        public static SqlDataReader ExecuteProcedureReader(string sSQL, params SqlParameter[] ParaName)
        {
            SqlConnection connection = new SqlConnection(m_ConnStr);
            SqlCommand command = new SqlCommand(sSQL, connection);
            command.CommandType = CommandType.StoredProcedure;
            if (ParaName != null)
            {
                command.Parameters.AddRange(ParaName);
            }
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception e)
            {
                CallException(sSQL + "        " + e.Message);
                return null;
            }
        }
        #endregion

        #region ExecuteScalar //执行查询，并返回查询所返回的结果集中第一行的第一列

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列
        /// </summary>
        /// <param name="sSQL">SQL语句</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sSQL)
        {
            SqlTransaction sqlT = null;
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                SqlCommand cmd = new SqlCommand(sSQL, m_Conn);
                try
                {
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    sqlT = m_Conn.BeginTransaction();
                    cmd.Transaction = sqlT;
                    var res = cmd.ExecuteScalar();
                    sqlT.Commit();
                    if (res == DBNull.Value) res = null;
                    return res;
                }
                catch (Exception e)
                {
                    if (sqlT != null)
                        sqlT.Rollback();
                    CallException(sSQL + "        " + e.Message);
                    return null;
                }
            }
        }

        #endregion ExecuteScalar

        #region ExecuteStoredProcedure //执行一个存储过程

        /// <summary>
        /// 执行一个带参数的存储过程
        /// </summary>
        /// <param name="ProName">存储过程名</param>
        /// <param name="ParaName">参数名称</param>
        /// <param name="ParaDir">参数方向，Input参数是输入参数 InputOutput参数既能输入，也能输出 Output参数是输出参数 ReturnValue参数存储过程返回值。</param>
        /// <param name="Para">参数对象数组</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static int ExecuteStoredProcedure(string ProName, params SqlParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(ProName, m_Conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    SqlParameter param = new SqlParameter();
                    cmd.Parameters.Add(param);
                    param.Direction = ParameterDirection.ReturnValue;
                    if (m_Conn.State == ConnectionState.Closed)
                    {
                        m_Conn.Open();
                    }
                    cmd.ExecuteNonQuery();
                    return (int)param.Value;
                }
                catch (Exception e)
                {
                    CallException(ProName + "        " + e.Message);
                    return -1;
                }
            }
        }

        /// <summary>
        /// 执行一个没有参数和返回值的存储过程（默认参数类型）
        /// </summary>
        /// <param name="ProName">存储过程名</param>
        /// <param name="Para">参数对象数组</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static bool ExecuteStoredProcedure(string ProName)
        {
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                try
                {
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    SqlCommand cmd = new SqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    CallException(ProName + "        " + e.Message);
                    return false;
                }
            }
        }

        /// <summary>
        /// 执行一个带参数的存储过程，并返回数据集
        /// </summary>
        /// <param name="ProName">存储过程名</param>
        /// <param name="ParaName">参数名称</param>
        /// <param name="Para">参数对象数组</param>
        /// <param name="ds">执行过程中返回的数据集</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static DataSet ExecuteDataSetProcedure(string ProName, ref int returnValue, params SqlParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                DataSet ds = new DataSet();
                try
                {
                    SqlCommand cmd = new SqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    SqlParameter param = new SqlParameter { Direction = ParameterDirection.ReturnValue };
                    cmd.Parameters.Add(param);
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    returnValue = (int)param.Value;
                    return ds;
                }
                catch (Exception e)
                {
                    CallException(ProName + "        " + e.Message);
                    return null;
                }
            }
        }

        public static DataSet ExecuteDataSetProcedure(string ProName, params SqlParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                DataSet ds = new DataSet();
                try
                {
                    SqlCommand cmd = new SqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    return ds;
                }

                catch (Exception e)
                {
                    CallException(ProName + "        " + e.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// 执行一个带参数的存储过程，并返回数据集
        /// </summary>
        /// <param name="ProName">存储过程名</param>
        /// <param name="ParaName">参数名称</param>
        /// <param name="Para">参数对象数组</param>
        /// <param name="ds">执行过程中返回的数据集</param>
        /// <returns>成功返回true，失败返回false</returns>
        /// 
        public static DataTable ExecuteDataTableProcedure(string ProName, params SqlParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                DataTable ds = new DataTable();
                try
                {
                    SqlCommand cmd = new SqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    return ds;
                }
                catch (Exception e)
                {
                    CallException(ProName + "        " + e.Message);
                    return null;
                }
            }
        }

        public static DataTable ExecuteDataTableProcedure(string ProName, ref int returnValue, SqlParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                DataTable ds = new DataTable();
                try
                {
                    SqlCommand cmd = new SqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    SqlParameter param = new SqlParameter { Direction = ParameterDirection.ReturnValue };
                    cmd.Parameters.Add(param);
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds);
                    returnValue = (int)param.Value;
                    return ds;
                }
                catch (Exception e)
                {
                    CallException(ProName + "        " + e.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// 执行一个带参数的存储过程,同时输出一行
        /// </summary>
        /// <param name="ProName">存储过程名</param>
        /// <param name="ParaName">参数名称</param>
        /// <param name="Para">参数对象数组</param>
        /// <returns>返回整数</returns>		
        public static DataRow ExecuteDataRowProcedure(string ProName, params SqlParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    DataTable table = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(table);
                    if (table.Rows.Count > 0)
                        return table.Rows[0];
                    else
                        return table.NewRow();
                }
                catch (Exception e)
                {
                    CallException(ProName + "        " + e.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// 执行一个带参数的存储过程,同时输出一行
        /// </summary>
        /// <param name="ProName">存储过程名</param>
        /// <param name="ParaName">参数名称</param>
        /// <param name="Para">参数对象数组</param>
        /// <returns>返回整数</returns>		
        public static DataRowView ExecuteDataRowViewProcedure(string ProName, params SqlParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(m_ConnStr))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    DataTable table = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(table);
                    if (table.Rows.Count > 0)
                        return table.DefaultView[0];
                    else
                        return table.DefaultView.AddNew();
                }
                catch (Exception e)
                {
                    CallException(ProName + "        " + e.Message);
                    return null;
                }
            }
        }

        #endregion ExecuteStoredProcedure
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
