using System.Data;

namespace GymManagement.Api.DTOs
{
    /// <summary>
    /// Helper map DataRow -> DTO, hỗ trợ SP trả về tên cột tiếng Việt hoặc tiếng Anh.
    /// </summary>
    public static class DtoMapper
    {
        public static object? Get(DataRow row, params string[] names)
        {
            foreach (var name in names)
            {
                if (!row.Table.Columns.Contains(name)) continue;
                var value = row[name];
                if (value != DBNull.Value) return value;
            }
            return null;
        }

        public static int GetInt(DataRow row, params string[] names)
        {
            var value = Get(row, names);
            return value == null ? 0 : Convert.ToInt32(value);
        }

        public static int? GetNullableInt(DataRow row, params string[] names)
        {
            var value = Get(row, names);
            return value == null ? null : Convert.ToInt32(value);
        }

        public static string? GetString(DataRow row, params string[] names)
            => Get(row, names)?.ToString();

        public static bool? GetNullableBool(DataRow row, params string[] names)
        {
            var value = Get(row, names);
            return value == null ? null : Convert.ToBoolean(value);
        }

        public static DateTime? GetNullableDate(DataRow row, params string[] names)
        {
            var value = Get(row, names);
            return value == null ? null : Convert.ToDateTime(value);
        }

        public static decimal? GetNullableDecimal(DataRow row, params string[] names)
        {
            var value = Get(row, names);
            return value == null ? null : Convert.ToDecimal(value);
        }

        public static decimal GetDecimal(DataRow row, params string[] names)
        {
            var value = Get(row, names);
            return value == null ? 0 : Convert.ToDecimal(value);
        }

        public static List<T> MapList<T>(DataTable dt, Func<DataRow, T> map)
        {
            var list = new List<T>();
            foreach (DataRow row in dt.Rows) list.Add(map(row));
            return list;
        }
    }
}
