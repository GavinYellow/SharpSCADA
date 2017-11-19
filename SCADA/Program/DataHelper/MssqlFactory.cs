using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DatabaseLib
{
    public class MssqlFactory : IDataFactory
    {
        public void CallException(string message)
        {
            DataHelper.AddErrorLog(new Exception(message));
        }

        public bool ConnectionTest()
        {
            //创建连接对象
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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

        public DbParameter CreateParam(string paramName, SqlDbType dbType, object objValue, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            SqlParameter parameter = new SqlParameter(paramName, dbType);
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
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public DataSet ExecuteDataset(string SQL, string TableName)
        {
            DataSet ds = new DataSet();
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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

        public DataSet ExecuteDataset(string[] SQLs, string[] TableNames)
        {
            DataSet ds = new DataSet();
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public DataTable ExecuteDataTable(string SQL)
        {
            DataTable dt = new DataTable();
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public int ExecuteNonQuery(string SQL)
        {
            int res = -1;
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public int ExecuteNonQuery(string[] SQLs)
        {
            int res = -1;
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public int ExecuteNonQuery(string[] SQLs, object[][] Pars)
        {
            int res = -1;
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public void FillDataSet(ref DataSet ds, string SQL, string TableName)
        {
            try
            {
                SqlConnection m_Conn;
                m_Conn = new SqlConnection(DataHelper.ConnectString);
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
        public DbDataReader ExecuteReader(string sSQL)
        {
            SqlConnection connection = new SqlConnection(DataHelper.ConnectString);
            SqlCommand command = new SqlCommand(sSQL, connection);
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }


        public DbDataReader ExecuteProcedureReader(string sSQL, params DbParameter[] ParaName)
        {
            SqlConnection connection = new SqlConnection(DataHelper.ConnectString);
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
        public object ExecuteScalar(string sSQL)
        {
            SqlTransaction sqlT = null;
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public int ExecuteStoredProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public bool ExecuteStoredProcedure(string ProName)
        {
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public DataSet ExecuteDataSetProcedure(string ProName, ref int returnValue, params DbParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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

        public DataSet ExecuteDataSetProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public DataTable ExecuteDataTableProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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

        public DataTable ExecuteDataTableProcedure(string ProName, ref int returnValue, DbParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public DataRow ExecuteDataRowProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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
        public DataRowView ExecuteDataRowViewProcedure(string ProName, params DbParameter[] ParaName)
        {
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
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

        public bool BulkCopy(IDataReader reader, string tableName, string command = null, SqlBulkCopyOptions options = SqlBulkCopyOptions.Default)
        {
            using (SqlConnection m_Conn = new SqlConnection(DataHelper.ConnectString))
            {
                SqlTransaction sqlT = null;
                try
                {
                    if (m_Conn.State == ConnectionState.Closed)
                        m_Conn.Open();
                    sqlT = m_Conn.BeginTransaction();
                    if (!string.IsNullOrEmpty(command))
                    {
                        SqlCommand cmd = new SqlCommand(command, m_Conn);
                        cmd.Transaction = sqlT;
                        cmd.ExecuteNonQuery();
                    }
                    SqlBulkCopy copy = new SqlBulkCopy(m_Conn, options, sqlT);
                    copy.DestinationTableName = tableName;
                    copy.BulkCopyTimeout = 100000;
                    //copy.BatchSize = _capacity;
                    copy.WriteToServer(reader);//如果写入失败，考虑不能无限增加线程数
                                               //Clear();
                    sqlT.Commit();
                    m_Conn.Close();
                    return true;
                }
                catch (Exception e)
                {
                    if (sqlT != null)
                        sqlT.Rollback();
                    m_Conn.Close();
                    CallException(e.Message);
                    return false;
                }
            }
        }
    }
}
