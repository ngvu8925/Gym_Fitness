using System.Data;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class DatabaseHelper
    {
        private readonly string _connString;

        public DatabaseHelper(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' chưa được cấu hình. " +
                    "Đặt trong user secrets: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<value>\"");
        }

        public DataTable ExecuteQuery(string spName, params SqlParameter[] parameters)
        {
            return ExecuteQuery(spName, parameters, isStoredProcedure: true);
        }

        public DataTable ExecuteQuery(string sql, SqlParameter[] parameters, bool isStoredProcedure)
        {
            var dt = new DataTable();
            using var conn = new SqlConnection(_connString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public int ExecuteNonQuery(string spName, params SqlParameter[] parameters)
        {
            return ExecuteNonQuery(spName, parameters, isStoredProcedure: true);
        }

        public int ExecuteNonQuery(string sql, SqlParameter[] parameters, bool isStoredProcedure)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string spName, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = new SqlCommand(spName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteScalar();
        }
    }
}
