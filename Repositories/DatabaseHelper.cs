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
                ?? "Data Source=localhost\\SQLEXPRESS;Initial Catalog=QL_GymFitness_Pro;User ID=sa;Password=123123;TrustServerCertificate=True;";
        }

        public DatabaseHelper()
        {
            _connString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=QL_GymFitness_Pro;User ID=sa;Password=123123;TrustServerCertificate=True;";
        }

        public DataTable ExecuteQuery(string spName, params SqlParameter[] parameters)
        {
            var dt = new DataTable();
            using var conn = new SqlConnection(_connString);
            using var cmd = new SqlCommand(spName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public int ExecuteNonQuery(string spName, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(_connString);
            using var cmd = new SqlCommand(spName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
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