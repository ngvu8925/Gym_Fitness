using System.Data;
using GymManagement.Api.DTOs;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class TrainerRepository
    {
        private readonly DatabaseHelper _db;

        public TrainerRepository(DatabaseHelper db) => _db = db;

        public DataTable List() => _db.ExecuteQuery("sp_Trainer_GetAll");

        public int Add(TrainerRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@BranchId", (object?)req.BranchId ?? DBNull.Value),
                new("@FullName", req.FullName),
                new("@Specialty", (object?)req.Specialty ?? DBNull.Value),
                new("@PhoneNumber", (object?)req.Phone ?? DBNull.Value),
                new("@HireDate", (object?)req.HireDate ?? DBNull.Value)
            };
            var dt = _db.ExecuteQuery("sp_Trainer_Create", ps);
            return Convert.ToInt32(dt.Rows[0]["TrainerId"]);
        }
    }
}
