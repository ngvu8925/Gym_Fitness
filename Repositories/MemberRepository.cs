using System.Data;
using GymManagement.Api.DTOs;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly DatabaseHelper _db;

        public MemberRepository(DatabaseHelper db) => _db = db;

        public int Add(MemberRequest member)
        {
            var ps = new SqlParameter[]
            {
                new("@BranchId", (object?)member.BranchId ?? DBNull.Value),
                new("@FullName", member.FullName),
                new("@Gender", (object?)member.Gender ?? DBNull.Value),
                new("@DateOfBirth", (object?)member.DateOfBirth ?? DBNull.Value),
                new("@PhoneNumber", member.Phone),
                new("@Email", (object?)member.Email ?? DBNull.Value),
                new("@Address", (object?)member.Address ?? DBNull.Value)
            };
            var dt = _db.ExecuteQuery("sp_Member_Create", ps);
            return Convert.ToInt32(dt.Rows[0]["MemberId"]);
        }

        public void Update(int memberId, MemberRequest member)
        {
            var ps = new SqlParameter[]
            {
                new("@MemberId", memberId),
                new("@BranchId", (object?)member.BranchId ?? DBNull.Value),
                new("@FullName", member.FullName),
                new("@Gender", (object?)member.Gender ?? DBNull.Value),
                new("@DateOfBirth", (object?)member.DateOfBirth ?? DBNull.Value),
                new("@PhoneNumber", member.Phone),
                new("@Email", (object?)member.Email ?? DBNull.Value),
                new("@Address", (object?)member.Address ?? DBNull.Value)
            };
            _db.ExecuteNonQuery("sp_Member_Update", ps);
        }

        public DataTable GetById(int memberId)
        {
            return _db.ExecuteQuery("sp_Member_GetById",
                new SqlParameter("@MemberId", memberId));
        }

        public void Delete(int memberId)
        {
            _db.ExecuteNonQuery("sp_Member_Delete", new SqlParameter("@MemberId", memberId));
        }

        public DataTable Search(string? keyword)
        {
            string? search = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
            return _db.ExecuteQuery("sp_Member_Search",
                new SqlParameter("@Keyword", (object?)search ?? DBNull.Value));
        }
    }
}
