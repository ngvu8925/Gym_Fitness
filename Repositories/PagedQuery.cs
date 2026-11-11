using System.Data;
using Microsoft.Data.SqlClient;

namespace GymManagement.Api.Repositories
{
    /// <summary>
    /// Gọi SP có tham số phân trang; nếu SP trên DB chưa nâng cấp (không nhận
    /// @Page/@PageSize/@SortDir) thì tự quay về gọi SP cũ để API không bị chết.
    /// </summary>
    public static class PagedQuery
    {
        /// <summary>
        /// Chạy SP bản phân trang; lỗi 8144/8145 (sai/thừa tham số SP) => chạy legacyCall.
        /// </summary>
        public static DataTable Execute(
            DatabaseHelper db,
            string spName,
            SqlParameter[] pagedParameters,
            Func<DataTable> legacyCall)
        {
            try
            {
                return db.ExecuteQuery(spName, pagedParameters);
            }
            catch (SqlException ex) when (ex.Number is 8144 or 8145)
            {
                return legacyCall();
            }
        }
    }
}
