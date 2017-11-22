using System.Data;
using System.Data.Common;

namespace DatabaseLib
{
    public interface IDataFactory
    {
        bool BulkCopy(IDataReader reader, string tableName, string command = null);
        void CallException(string message);
        bool ConnectionTest();
        DbParameter CreateParam(string paramName, SqlDbType dbType, object objValue, int size = 0, ParameterDirection direction = ParameterDirection.Input);
        DataRow ExecuteDataRowProcedure(string ProName, params DbParameter[] ParaName);
        DataRowView ExecuteDataRowViewProcedure(string ProName, params DbParameter[] ParaName);
        DataSet ExecuteDataset(string SQL);
        DataSet ExecuteDataset(string[] SQLs, string[] TableNames);
        DataSet ExecuteDataset(string SQL, string TableName);
        DataSet ExecuteDataSetProcedure(string ProName, params DbParameter[] ParaName);
        DataSet ExecuteDataSetProcedure(string ProName, ref int returnValue, params DbParameter[] ParaName);
        DataTable ExecuteDataTable(string SQL);
        DataTable ExecuteDataTableProcedure(string ProName, params DbParameter[] ParaName);
        DataTable ExecuteDataTableProcedure(string ProName, ref int returnValue, DbParameter[] ParaName);
        int ExecuteNonQuery(string[] SQLs);
        int ExecuteNonQuery(string SQL);
        int ExecuteNonQuery(string[] SQLs, object[][] Pars);
        DbDataReader ExecuteProcedureReader(string sSQL, params DbParameter[] ParaName);
        DbDataReader ExecuteReader(string sSQL);
        object ExecuteScalar(string sSQL);
        bool ExecuteStoredProcedure(string ProName);
        int ExecuteStoredProcedure(string ProName, params DbParameter[] ParaName);
        void FillDataSet(ref DataSet ds, string SQL, string TableName);
    }
}