using System.Data;
using GymManagement.Api.Models;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    public class MemberAccountRepository : IMemberAccountRepository
    {
        private readonly DatabaseHelper _db;

        public MemberAccountRepository(DatabaseHelper db) => _db = db;

        public DataRow? GetAccountById(int accountId)
        {
            var dt = _db.ExecuteQuery("sp_MemberAccount_GetAccount",
                new SqlParameter("@AccountId", accountId));
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public Member? GetMemberByAccountId(int accountId)
        {
            var dt = _db.ExecuteQuery("sp_MemberAccount_GetMemberByAccountId",
                new SqlParameter("@AccountId", accountId));
            return dt.Rows.Count > 0 ? MapMember(dt.Rows[0]) : null;
        }

        public List<Member> FindUnlinkedMatches(int accountId)
        {
            var dt = _db.ExecuteQuery("sp_MemberAccount_FindUnlinkedMatches",
                new SqlParameter("@AccountId", accountId));

            var members = new List<Member>();
            foreach (DataRow row in dt.Rows) members.Add(MapMember(row));
            return members;
        }

        /// <summary>
        /// Kết quả của sp_MemberAccount_Link:
        /// -1 member không tồn tại, -2 account không tồn tại,
        /// -3 account không phải Member, -4 account đã link member khác, 0 thành công.
        /// </summary>
        public int Link(int memberId, int? accountId)
        {
            var result = _db.ExecuteScalar("sp_MemberAccount_Link",
                new SqlParameter("@MemberId", memberId),
                new SqlParameter("@AccountId", (object?)accountId ?? DBNull.Value));

            return Convert.ToInt32(result ?? 0);
        }

        public DataTable MemberOptions() => _db.ExecuteQuery("sp_MemberAccount_MemberOptions");

        public DataTable GetMyRegistrations(int memberId)
        {
            return _db.ExecuteQuery("sp_MemberPortal_GetMyRegistrations",
                new SqlParameter("@MemberId", memberId));
        }

        private static Member MapMember(DataRow row)
        {
            return new Member
            {
                MemberId = row["MemberId"] == DBNull.Value ? 0 : Convert.ToInt32(row["MemberId"]),
                BranchId = row["BranchId"] == DBNull.Value ? null : Convert.ToInt32(row["BranchId"]),
                AccountId = row["AccountId"] == DBNull.Value ? null : Convert.ToInt32(row["AccountId"]),
                FullName = row["FullName"]?.ToString() ?? string.Empty,
                Gender = row["Gender"] == DBNull.Value ? null : row["Gender"].ToString(),
                DateOfBirth = row["DateOfBirth"] == DBNull.Value ? null : Convert.ToDateTime(row["DateOfBirth"]),
                Phone = row["PhoneNumber"]?.ToString() ?? string.Empty,
                Email = row["Email"] == DBNull.Value ? null : row["Email"].ToString(),
                Address = row["Address"] == DBNull.Value ? null : row["Address"].ToString(),
                Avatar = row["ProfileImage"] == DBNull.Value ? null : row["ProfileImage"].ToString(),
                ReferrerId = row["ReferrerMemberId"] == DBNull.Value ? null : Convert.ToInt32(row["ReferrerMemberId"]),
                JoinDate = row["JoinDate"] == DBNull.Value ? default : Convert.ToDateTime(row["JoinDate"]),
                Status = row["Status"] != DBNull.Value && Convert.ToBoolean(row["Status"])
            };
        }
    }
}
