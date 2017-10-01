using System.Data.SqlClient;

namespace TagConfig
{
    public static class ExMethods
    {

        public static string GetNullableString(this SqlDataReader dataReader, int index)
        {
            var svr = dataReader.GetSqlString(index);
            return svr.IsNull ? null : svr.Value;
        }
    }
}
