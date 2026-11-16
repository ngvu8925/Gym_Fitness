-- ============================================================
-- Bước 11: Lịch học lớp (thứ mấy, giờ nào) + dữ liệu Member Portal
-- Chạy thủ công trên DB QL_GymFitness_Pro (KHÔNG tự chạy)
-- ============================================================

-- QUAN TRỌNG: khi chạy bằng sqlcmd, session mặc định QUOTED_IDENTIFIER OFF
-- và SP sẽ "đóng dấu" tùy chọn này lúc tạo -> có thể lỗi INSERT/UPDATE
-- trên bảng có filtered index/computed column. Phải bật ON trước khi tạo SP.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1) Thêm cột lịch học vào bảng Classes (nếu chưa có)
IF COL_LENGTH('Classes', 'DaysOfWeek') IS NULL
    ALTER TABLE Classes ADD DaysOfWeek NVARCHAR(50) NULL; -- VD: '2,4,6' (thứ 2, 4, 6)
GO
IF COL_LENGTH('Classes', 'StartTime') IS NULL
    ALTER TABLE Classes ADD StartTime TIME NULL;
GO
IF COL_LENGTH('Classes', 'EndTime') IS NULL
    ALTER TABLE Classes ADD EndTime TIME NULL;
GO

-- 2) Cập nhật sp_Class_Create nhận thêm lịch học
-- (bản cũ giữ nguyên logic, chỉ thêm 3 tham số optional)
IF OBJECT_ID('sp_Class_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Class_Create;
GO
CREATE PROCEDURE sp_Class_Create
    @BranchId INT = NULL,
    @ClassName NVARCHAR(200),
    @TrainerId INT = NULL,
    @Capacity INT,
    @DaysOfWeek NVARCHAR(50) = NULL,
    @StartTime TIME = NULL,
    @EndTime TIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Classes (BranchId, ClassName, TrainerId, Capacity, Status, DaysOfWeek, StartTime, EndTime)
    VALUES (@BranchId, @ClassName, @TrainerId, @Capacity, 1, @DaysOfWeek, @StartTime, @EndTime);
    SELECT SCOPE_IDENTITY() AS ClassId;
END
GO

-- 3) Cập nhật sp_Class_Update nhận thêm lịch học
IF OBJECT_ID('sp_Class_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Class_Update;
GO
CREATE PROCEDURE sp_Class_Update
    @ClassId INT,
    @BranchId INT = NULL,
    @ClassName NVARCHAR(200),
    @TrainerId INT = NULL,
    @Capacity INT,
    @DaysOfWeek NVARCHAR(50) = NULL,
    @StartTime TIME = NULL,
    @EndTime TIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Classes
    SET BranchId = @BranchId,
        ClassName = @ClassName,
        TrainerId = @TrainerId,
        Capacity = @Capacity,
        DaysOfWeek = @DaysOfWeek,
        StartTime = @StartTime,
        EndTime = @EndTime
    WHERE ClassId = @ClassId;
END
GO

-- 4) Lớp học hội viên đã đăng kí: tên lớp + HLV + lịch học
IF OBJECT_ID('sp_MemberPortal_GetMyClasses', 'P') IS NOT NULL DROP PROCEDURE sp_MemberPortal_GetMyClasses;
GO
CREATE PROCEDURE sp_MemberPortal_GetMyClasses
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cr.ClassRegistrationId,
           c.ClassId,
           c.ClassName,
           ISNULL(t.FullName, N'Chưa phân công') AS TrainerName,
           c.DaysOfWeek,
           c.StartTime,
           c.EndTime
    FROM ClassRegistrations cr
    INNER JOIN Classes c ON c.ClassId = cr.ClassId
    LEFT JOIN Trainers t ON t.TrainerId = c.TrainerId
    WHERE cr.MemberId = @MemberId
    ORDER BY cr.ClassRegistrationId DESC;
END
GO

-- 5) Lịch sử check-in gần đây của hội viên
IF OBJECT_ID('sp_MemberPortal_GetMyAttendance', 'P') IS NOT NULL DROP PROCEDURE sp_MemberPortal_GetMyAttendance;
GO
CREATE PROCEDURE sp_MemberPortal_GetMyAttendance
    @MemberId INT,
    @Top INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@Top) AttendanceId, CheckInTime, CheckOutTime
    FROM Attendances
    WHERE MemberId = @MemberId
    ORDER BY CheckInTime DESC;
END
GO
