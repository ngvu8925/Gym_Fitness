using System.Data;

namespace GymManagement.Api.Controllers
{
    public static class DataTableExtensions
    {
        private static readonly Dictionary<string, string> ColumnAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            // Member mappings
            ["MaHV"] = "memberId",
            ["MemberId"] = "memberId",
            ["HoTen"] = "fullName",
            ["FullName"] = "fullName",
            ["GioiTinh"] = "gender",
            ["Gender"] = "gender",
            ["NgaySinh"] = "dateOfBirth",
            ["DateOfBirth"] = "dateOfBirth",
            ["SDT"] = "phone",
            ["PhoneNumber"] = "phone",
            ["Email"] = "email",
            ["DiaChi"] = "address",
            ["Address"] = "address",
            ["AnhDaiDien"] = "avatar",
            ["ProfileImage"] = "avatar",
            ["MaNguoiGioiThieu"] = "referrerId",
            ["ReferrerMemberId"] = "referrerId",
            ["NgayThamGia"] = "joinDate",
            ["JoinDate"] = "joinDate",
            ["TrangThai"] = "status",
            ["Status"] = "status",

            // Branch mappings
            ["MaCN"] = "branchId",
            ["BranchId"] = "branchId",
            ["TenCN"] = "branchName",
            ["BranchName"] = "branchName",

            // Package mappings
            ["MaGoi"] = "packageId",
            ["PackageId"] = "packageId",
            ["TenGoi"] = "packageName",
            ["PackageName"] = "packageName",
            ["LoaiGoi"] = "packageType",
            ["PackageType"] = "packageType",
            ["SoThang"] = "months",
            ["DurationInMonths"] = "months",
            ["DonGia"] = "price",
            ["UnitPrice"] = "price",
            ["MoTa"] = "description",
            ["Description"] = "description",

            // Class mappings
            ["MaLop"] = "classId",
            ["ClassId"] = "classId",
            ["TenLop"] = "className",
            ["ClassName"] = "className",
            ["MaHLV"] = "trainerId",
            ["TrainerId"] = "trainerId",
            ["TenHLV"] = "trainerName",
            ["TrainerName"] = "trainerName",
            ["SucChua"] = "capacity",
            ["Capacity"] = "capacity",

            // Trainer mappings
            ["ChuyenMon"] = "specialty",
            ["Specialty"] = "specialty",
            ["NgayVaoLam"] = "hireDate",
            ["HireDate"] = "hireDate",

            // Attendance mappings
            ["MaDD"] = "attendanceId",
            ["AttendanceId"] = "attendanceId",
            ["NgayCheckIn"] = "checkInAt",
            ["CheckInTime"] = "checkInAt",
            ["NgayCheckOut"] = "checkOutAt",
            ["CheckOutTime"] = "checkOutAt",

            // Account mappings
            ["MaTK"] = "accountId",
            ["AccountId"] = "accountId",
            ["TenDangNhap"] = "username",
            ["Username"] = "username",
            ["VaiTro"] = "role",
            ["Role"] = "role",
            ["NgayTao"] = "createdAt",
            ["CreatedAt"] = "createdAt",

            // Registration mappings
            ["MaDK"] = "registrationId",
            ["RegistrationId"] = "registrationId",
            ["MaKM"] = "promotionId",
            ["PromotionId"] = "promotionId",
            ["SoTienGiam"] = "discountAmount",
            ["DiscountAmount"] = "discountAmount",
            ["NgayBatDau"] = "startDate",
            ["StartDate"] = "startDate",
            ["NgayKetThuc"] = "endDate",
            ["EndDate"] = "endDate",
            ["MaDKL"] = "classRegistrationId",
            ["ClassRegistrationId"] = "classRegistrationId",
            ["MaTN"] = "pauseId",
            ["SuspensionId"] = "pauseId",

            // Report mappings
            ["TongDoanhThu"] = "totalRevenue",
            ["TotalRevenue"] = "totalRevenue",
            ["Thang"] = "month",
            ["Month"] = "month",
            ["Nam"] = "year",
            ["Year"] = "year",
            ["SoLuotDangKy"] = "registrationCount",
            ["RegistrationCount"] = "registrationCount",
            ["DoanhThu"] = "revenue",
            ["Revenue"] = "revenue"
        };

        public static List<Dictionary<string, object?>> ToJsonList(this DataTable dt)
        {
            var list = new List<Dictionary<string, object?>>();
            foreach (DataRow row in dt.Rows)
            {
                var item = new Dictionary<string, object?>();
                foreach (DataColumn col in dt.Columns)
                {
                    string key = ColumnAliases.TryGetValue(col.ColumnName, out var alias)
                        ? alias
                        : ToCamelCase(col.ColumnName);
                    item[key] = row[col] == DBNull.Value ? null : row[col];
                }
                list.Add(item);
            }
            return list;
        }

        private static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}
