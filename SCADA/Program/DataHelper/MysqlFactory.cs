using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;

namespace DatabaseLib
{
    public class MysqlFactory : IDataFactory
    {
        public void CallException(string message)
        {
            DataHelper.AddErrorLog(new Exception(message));
        }

        public bool ConnectionTest()
        {
            //创建连接对象
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                //myMySqlConnection.ConnectionTimeout = 1;//设置连接超时的时间
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
            //myMySqlConnection   is   a   MySqlConnection   object 
            return false;
        }

        public MySqlDbType ConvertType(SqlDbType dbType)
        {
            switch (dbType)
            {
                case SqlDbType.BigInt:
                    return MySqlDbType.Int64;
                case SqlDbType.Binary:
                    return MySqlDbType.Binary;
                case SqlDbType.Bit:
                    return MySqlDbType.Bit;
                case SqlDbType.Date:
                    return MySqlDbType.Date;
                case SqlDbType.SmallDateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTime:
                    return MySqlDbType.DateTime;
                case SqlDbType.DateTimeOffset:
                    return MySqlDbType.Time;
                case SqlDbType.Decimal:
                    return MySqlDbType.Decimal;
                case SqlDbType.Float:
                    return MySqlDbType.Double;
                case SqlDbType.Image:
                    return MySqlDbType.Binary;
                case SqlDbType.Int:
                    return MySqlDbType.Int32;
                case SqlDbType.Money:
                    return MySqlDbType.Float;
                case SqlDbType.NText:
                case SqlDbType.Text:
                    return MySqlDbType.Text;
                case SqlDbType.Real:
                    return MySqlDbType.Float;
                case SqlDbType.SmallInt:
                    return MySqlDbType.Int16;
                case SqlDbType.Structured:
                    return MySqlDbType.Set;
                case SqlDbType.Time:
                    return MySqlDbType.Time;
                case SqlDbType.Timestamp:
                    return MySqlDbType.Timestamp;
                case SqlDbType.TinyInt:
                    return MySqlDbType.Byte;
                case SqlDbType.VarBinary:
                    return MySqlDbType.VarBinary;
                case SqlDbType.Char:
                case SqlDbType.NVarChar:
                case SqlDbType.VarChar:
                    return MySqlDbType.VarChar;
                default:
                    return MySqlDbType.VarChar;
            }
        }

        public DbParameter CreateParam(string paramName, SqlDbType dbType, object objValue, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            if (string.IsNullOrEmpty(paramName)) return null;
            if (paramName[0] == '@') paramName = 'p' + paramName.TrimStart('@');
            MySqlParameter parameter = new MySqlParameter(paramName, ConvertType(dbType));
            if (size > 0) parameter.Size = size;
            if (objValue == null)
            {
                if (direction == ParameterDirection.Output)
                {
                    parameter.Direction = direction;
                    return parameter;
                }
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
        public DataSet ExecuteDataset(string SQL)
        {
            DataSet ds = new DataSet();
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                try
                {
                    var da = new MySqlDataAdapter();
                    MySqlCommand cmd = new MySqlCommand(SQL, m_Conn);
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
        public DataSet ExecuteDataset(string SQL, string TableName)
        {
            DataSet ds = new DataSet();
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                try
                {
                    var da = new MySqlDataAdapter();
                    MySqlCommand cmd = new MySqlCommand(SQL, m_Conn);
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

        public DataSet ExecuteDataset(string[] SQLs, string[] TableNames)
        {
            DataSet ds = new DataSet();
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                try
                {
                    for (int i = 0; i < SQLs.Length; i++)
                    {
                        var da = new MySqlDataAdapter();
                        MySqlCommand cmd = new MySqlCommand(SQLs[i], m_Conn);
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
        public DataTable ExecuteDataTable(string SQL)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                try
                {
                    var da = new MySqlDataAdapter();
                    MySqlCommand cmd = new MySqlCommand(SQL, m_Conn);
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
        public int ExecuteNonQuery(string SQL)
        {
            int res = -1;
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                MySqlTransaction sqlT = null; MySqlBulkLoader loader = new MySqlBulkLoader(m_Conn);
                //loader.Columns
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(SQL, m_Conn))
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
        public int ExecuteNonQuery(string[] SQLs)
        {
            int res = -1;
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                MySqlTransaction sqlT = null;
                MySqlCommand cmd = new MySqlCommand();
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
        public int ExecuteNonQuery(string[] SQLs, object[][] Pars)
        {
            int res = -1;
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                MySqlTransaction sqlT = null;
                MySqlCommand cmd = new MySqlCommand();
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
        public void FillDataSet(ref DataSet ds, string SQL, string TableName)
        {
            try
            {
                MySqlConnection m_Conn;
                m_Conn = new MySqlConnection(DataHelper.ConnectString);
                var da = new MySqlDataAdapter();
                MySqlCommand cmd = new MySqlCommand(SQL, m_Conn);
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
        /// 返回一个MySqlDataReader
        /// </summary>
        public DbDataReader ExecuteReader(string sSQL)
        {
            MySqlConnection connection = new MySqlConnection(DataHelper.ConnectString);
            MySqlCommand command = new MySqlCommand(sSQL, connection);
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }


        public DbDataReader ExecuteProcedureReader(string sSQL, params DbParameter[] ParaName)
        {
            MySqlConnection connection = new MySqlConnection(DataHelper.ConnectString);
            MySqlCommand command = new MySqlCommand(sSQL, connection);
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
        public object ExecuteScalar(string sSQL)
        {
            MySqlTransaction sqlT = null;
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                MySqlCommand cmd = new MySqlCommand(sSQL, m_Conn);
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
        public int ExecuteStoredProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(ProName, m_Conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    MySqlParameter param = new MySqlParameter();
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
        public bool ExecuteStoredProcedure(string ProName)
        {
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                try
                {
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    MySqlCommand cmd = new MySqlCommand(ProName, m_Conn);
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
        public DataSet ExecuteDataSetProcedure(string ProName, ref int returnValue, params DbParameter[] ParaName)
        {
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                DataSet ds = new DataSet();
                try
                {
                    MySqlCommand cmd = new MySqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    MySqlParameter param = new MySqlParameter { Direction = ParameterDirection.ReturnValue };
                    cmd.Parameters.Add(param);
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    var da = new MySqlDataAdapter();
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

        public DataSet ExecuteDataSetProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                DataSet ds = new DataSet();
                try
                {
                    MySqlCommand cmd = new MySqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    var da = new MySqlDataAdapter();
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
        public DataTable ExecuteDataTableProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                DataTable ds = new DataTable();
                try
                {
                    MySqlCommand cmd = new MySqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    var da = new MySqlDataAdapter();
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

        public DataTable ExecuteDataTableProcedure(string ProName, ref int returnValue, DbParameter[] ParaName)
        {
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                DataTable ds = new DataTable();
                try
                {
                    MySqlCommand cmd = new MySqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    MySqlParameter param = new MySqlParameter { Direction = ParameterDirection.ReturnValue };
                    cmd.Parameters.Add(param);
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    var da = new MySqlDataAdapter();
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
        public DataRow ExecuteDataRowProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    DataTable table = new DataTable();
                    var da = new MySqlDataAdapter();
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
        public DataRowView ExecuteDataRowViewProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                try
                {
                    MySqlCommand cmd = new MySqlCommand(ProName, m_Conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (ParaName != null)
                    {
                        cmd.Parameters.AddRange(ParaName);
                    }
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    DataTable table = new DataTable();
                    var da = new MySqlDataAdapter();
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

        public bool BulkCopy(IDataReader reader, string tableName, string command = null, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default)
        {
            using (MySqlConnection m_Conn = new MySqlConnection(DataHelper.ConnectString))
            {
                MySqlTransaction sqlT = null;
                try
                {
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    sqlT = m_Conn.BeginTransaction();
                    if (!string.IsNullOrEmpty(command))
                    {
                        MySqlCommand cmd = new MySqlCommand(command, m_Conn);
                        cmd.Transaction = sqlT;
                        cmd.ExecuteNonQuery();
                    }
                    string tmpPath = Path.GetTempFileName();
                    string csv = DataHelper.ReaderToCsv(reader);
                    File.WriteAllText(tmpPath, csv);
                    MySqlBulkLoader copy = new MySqlBulkLoader(m_Conn)
                    {
                        FieldTerminator = ",",
                        FieldQuotationCharacter = '"',
                        EscapeCharacter = '"',
                        LineTerminator = "\r\n",
                        FileName = tmpPath,
                        NumberOfLinesToSkip = 0,
                        TableName = tableName,
                    };
                    //copy.BatchSize = _capacity;
                    copy.Load();//如果写入失败，考虑不能无限增加线程数
                                //Clear();
                    sqlT.Commit();
                    m_Conn.Close();
                    File.Delete(tmpPath);
                    return true;
                }
                catch (Exception e)
                {
                    if (sqlT != null)
                        sqlT.Rollback();
                    m_Conn.Close();
                    DataHelper.AddErrorLog(e);
                    return false;
                }
            }
        }

      
        #endregion ExecuteStoredProcedure
    }
}
