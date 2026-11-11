using System.Data;
using GymManagement.Api.DTOs;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class TrainerRepository : ITrainerRepository
    {
        private readonly DatabaseHelper _db;

        public TrainerRepository(DatabaseHelper db) => _db = db;

        public DataTable List(DTOs.Common.UserQueryParameters query)
        {
            var ps = new SqlParameter[]
            {
                new("@Keyword", query.NormalizedKeyword ?? (object)DBNull.Value),
                new("@Page", query.Page),
                new("@PageSize", query.PageSize),
                new("@SortDir", query.IsDescending ? "DESC" : "ASC")
            };
            return PagedQuery.Execute(_db, "sp_Trainer_GetAll", ps,
                legacyCall: () => _db.ExecuteQuery("sp_Trainer_GetAll"));
        }

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
        public void Update(int trainerId, TrainerRequest req)
        {
            var ps = new SqlParameter[]
            {
                new("@TrainerId", trainerId),
                new("@BranchId", (object?)req.BranchId ?? DBNull.Value),
                new("@FullName", req.FullName),
                new("@Specialty", (object?)req.Specialty ?? DBNull.Value),
                new("@PhoneNumber", (object?)req.Phone ?? DBNull.Value),
                new("@HireDate", (object?)req.HireDate ?? DBNull.Value)
            };
            _db.ExecuteNonQuery("sp_Trainer_Update", ps);
        }

        public void Delete(int trainerId)
        {
            _db.ExecuteNonQuery("sp_Trainer_Delete", new SqlParameter("@TrainerId", trainerId));
        }
    }
}
