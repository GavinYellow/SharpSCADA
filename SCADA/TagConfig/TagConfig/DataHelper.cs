using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace TagConfig
{
    public static class DataHelper //:MarshalByRefObject
    {
        public static string m_ConnStr = null;

        public static void CallException(string message)
        {
            Program.AddErrorLog(new Exception(message));
        }

        public static DataTable ConvertTextToTable(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            DataTable mydt = new DataTable("");
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                using (var mysr = new StreamReader(stream))
                {
                    string strline = mysr.ReadLine();
                    string[] aryline = strline.Split('\t');
                    for (int i = 0; i < aryline.Length; i++)
                    {
                        aryline[i] = aryline[i].Replace("\"", "");
                        mydt.Columns.Add(new DataColumn(aryline[i] + i));
                    }
                    int intColCount = aryline.Length;
                    while ((strline = mysr.ReadLine()) != null)
                    {
                        aryline = strline.Split('\t');
                        DataRow mydr = mydt.NewRow();
                        for (int i = 0; i < intColCount; i++)
                        {
                            mydr[i] = aryline[i].Replace("\"", "");
                        }
                        mydt.Rows.Add(mydr);
                    }
                    return mydt;
                }
            }
        }

        public static DataTable ConvertCSVToTable(string str)
        {
            DataTable mydt = new DataTable("");
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                using (var mysr = new StreamReader(stream))
                {
                    string strline = mysr.ReadLine();

                    Regex reg = new Regex(@",(?=(?:[^\""]*\""[^\""]*\"")*(?![^\""]*\""))");
                    string[] aryline = reg.Split(strline);
                    for (int i = 0; i < aryline.Length; i++)
                    {
                        aryline[i] = aryline[i].Replace("\"", "");
                        mydt.Columns.Add(new DataColumn(aryline[i]));
                    }
                    int intColCount = aryline.Length;
                    while ((strline = mysr.ReadLine()) != null)
                    {
                        aryline = reg.Split(strline);

                        DataRow mydr = mydt.NewRow();
                        for (int i = 0; i < intColCount; i++)
                        {
                            mydr[i] = aryline[i].Replace("\"", "");
                        }
                        mydt.Rows.Add(mydr);
                    }
                    return mydt;
                }
            }
        }

        public static bool ConnectionTest()
        {
            //创建连接对象
            SqlConnection m_Conn = new SqlConnection(m_ConnStr);
            //mySqlConnection.ConnectionTimeout = 1;//设置连接超时的时间
            try
            {
                //Open DataBase
                //打开数据库
                m_Conn.Open();
                if (m_Conn.State == ConnectionState.Open)
                {
                    m_Conn.Close();
                    return true;
                }
            }
            catch (Exception e)
            {
                CallException(e.Message);
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
            try
            {
                SqlConnection m_Conn;
                m_Conn = new SqlConnection(m_ConnStr);
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(SQL, m_Conn);
                da.SelectCommand = cmd;
                da.Fill(ds);
            }
            catch (Exception e)
            {
                CallException(e.Message);
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
                CallException(e.Message);
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
            try
            {
                SqlConnection m_Conn;
                m_Conn = new SqlConnection(m_ConnStr);
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
                CallException(e.Message);
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
            try
            {
                SqlConnection m_Conn;
                m_Conn = new SqlConnection(m_ConnStr);
                SqlDataAdapter da = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand(SQL, m_Conn);
                da.SelectCommand = cmd;
                da.Fill(dt);
            }
            catch (Exception e)
            {
                CallException(e.Message);
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
            SqlConnection m_Conn;
            m_Conn = new SqlConnection(m_ConnStr);
            SqlTransaction sqlT = null;
            try
            {
                SqlCommand cmd = new SqlCommand(SQL, m_Conn);
                if (m_Conn.State == ConnectionState.Closed)
                    m_Conn.Open();
                cmd.Connection = m_Conn;
                sqlT = m_Conn.BeginTransaction();
                cmd.Transaction = sqlT;
                res = cmd.ExecuteNonQuery();
                sqlT.Commit();
                m_Conn.Close();
            }
            catch (Exception e)
            {
                if (sqlT != null)
                    sqlT.Rollback();
                m_Conn.Close();
                CallException(e.Message);
                return -1;
            }
            return res;
        }

        /// <summary>
        /// 执行一组INSERT、UPDATE、DELETE语句
        /// </summary>
        /// <param name="SQLs">T-SQL语句</param>
        /// <returns>返回影响的行数</returns>
        public static int ExecuteNonQuery(string[] SQLs)
        {
            int res = -1;
            SqlConnection m_Conn;
            m_Conn = new SqlConnection(m_ConnStr);
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
                m_Conn.Close();
            }
            catch (Exception e)
            {
                if (sqlT != null)
                    sqlT.Rollback();
                m_Conn.Close();
                CallException(e.Message);
                res = -1;
            }
            return res;
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
            SqlConnection m_Conn;
            m_Conn = new SqlConnection(m_ConnStr);
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
                m_Conn.Close();
            }
            catch (Exception e)
            {
                if (sqlT != null)
                    sqlT.Rollback();
                m_Conn.Close();
                CallException(e.Message);
                res = -1;
            }
            return res;
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
                CallException(e.Message);
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
                CallException(e.Message);
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
            object res = null;
            SqlTransaction sqlT = null;
            SqlConnection m_Conn = new SqlConnection(m_ConnStr);
            SqlCommand cmd = new SqlCommand(sSQL, m_Conn);
            try
            {
                if (m_Conn.State == ConnectionState.Closed)
                    m_Conn.Open();
                sqlT = m_Conn.BeginTransaction();
                cmd.Transaction = sqlT;
                res = cmd.ExecuteScalar();
                sqlT.Commit();
                m_Conn.Close();
            }
            catch (Exception e)
            {
                if (sqlT != null)
                    sqlT.Rollback();
                m_Conn.Close();
                CallException(e.Message);
                res = null;
            }
            return res;
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
            SqlConnection m_Conn = new SqlConnection(m_ConnStr);
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
                m_Conn.Close();
                return (int)param.Value;
            }
            catch (Exception e)
            {
                m_Conn.Close();
                CallException(e.Message);
                return -1;
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
            SqlConnection m_Conn;
            m_Conn = new SqlConnection(m_ConnStr);
            try
            {
                if (m_Conn.State == ConnectionState.Closed)
                    m_Conn.Open();
                SqlCommand cmd = new SqlCommand(ProName, m_Conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                m_Conn.Close();
                return true;
            }
            catch (Exception e)
            {
                m_Conn.Close();
                CallException(e.Message);
                return false;
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
            SqlConnection m_Conn = new SqlConnection(m_ConnStr); ;
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
                m_Conn.Close();
                returnValue = (int)param.Value;
                return ds;
            }
            catch (Exception e)
            {
                m_Conn.Close();
                CallException(e.Message);
                return null;
            }
        }

        public static DataSet ExecuteDataSetProcedure(string ProName, params SqlParameter[] ParaName)
        {
            SqlConnection m_Conn = new SqlConnection(m_ConnStr); ;
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
                m_Conn.Close();
                return ds;
            }

            catch (Exception e)
            {
                m_Conn.Close();
                CallException(e.Message);
                return null;
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
            SqlConnection m_Conn = new SqlConnection(m_ConnStr);
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
                m_Conn.Close();
                return ds;
            }

            catch (Exception e)
            {
                m_Conn.Close();
                CallException(e.Message);
                return null;
            }
        }

        public static DataTable ExecuteDataTableProcedure(string ProName, ref int returnValue, SqlParameter[] ParaName)
        {
            SqlConnection m_Conn = new SqlConnection(m_ConnStr);
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
                m_Conn.Close();
                returnValue = (int)param.Value;
                return ds;
            }

            catch (Exception e)
            {
                m_Conn.Close();
                CallException(e.Message);
                return null;
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
            SqlConnection m_Conn = new SqlConnection(m_ConnStr);

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
                m_Conn.Close();
                if (table.Rows.Count > 0)
                    return table.Rows[0];
                else
                    return table.NewRow();
            }
            catch (Exception e)
            {
                m_Conn.Close();
                CallException(e.Message);
                return null;
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
            SqlConnection m_Conn = new SqlConnection(m_ConnStr);

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
                m_Conn.Close();
                if (table.Rows.Count > 0)
                    return table.DefaultView[0];
                else
                    return table.DefaultView.AddNew();
            }
            catch (Exception e)
            {
                m_Conn.Close();
                CallException(e.Message);
                return null;
            }
        }

        #endregion ExecuteStoredProcedure
    }
}
