-- ============================================================
-- Task 03: Hủy đăng ký lớp học (Member) + Hủy gói tập (Admin)
-- Chạy thủ công trên DB QL_GymFitness_Pro (KHÔNG tự chạy)
--
-- Thay đổi schema chỉ là THÊM CỘT MỚI (additive):
--   * ClassRegistrations.Status      -> giữ lịch sử, hủy = cập nhật trạng thái
--   * PackageRegistrations.CancelDate/CancelReason -> ghi nhận thời điểm + lý do hủy
-- Dữ liệu hiện có: dòng cũ được điền Status = N'Đã đăng ký' (DEFAULT),
-- CancelDate/CancelReason NULL. Không xóa/sửa dữ liệu nào.
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1) ClassRegistrations.Status: trạng thái đăng ký lớp
IF COL_LENGTH('ClassRegistrations', 'Status') IS NULL
    ALTER TABLE dbo.ClassRegistrations
    ADD Status NVARCHAR(20) NOT NULL
        CONSTRAINT DF_ClassRegistrations_Status DEFAULT N'Đã đăng ký';
GO

-- 2) PackageRegistrations: thời điểm + lý do hủy (NULL = chưa hủy)
IF COL_LENGTH('PackageRegistrations', 'CancelDate') IS NULL
    ALTER TABLE dbo.PackageRegistrations ADD CancelDate DATE NULL;
GO
IF COL_LENGTH('PackageRegistrations', 'CancelReason') IS NULL
    ALTER TABLE dbo.PackageRegistrations ADD CancelReason NVARCHAR(255) NULL;
GO

-- 3) Member hủy đăng ký lớp của CHÍNH MÌNH
--    WHERE MemberId = @MemberId -> hội viên khác không thể hủy hộ.
--    Trả về số dòng cập nhật (0 = không thuộc hội viên / đã hủy rồi).
IF OBJECT_ID('sp_ClassRegistration_Cancel', 'P') IS NOT NULL DROP PROCEDURE sp_ClassRegistration_Cancel;
GO
CREATE PROCEDURE dbo.sp_ClassRegistration_Cancel
    @ClassRegistrationId INT, @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.ClassRegistrations
    SET Status = N'Đã hủy'
    WHERE ClassRegistrationId = @ClassRegistrationId
      AND MemberId = @MemberId
      AND Status <> N'Đã hủy';
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

-- 4) Danh sách lớp của member: chỉ hiện đăng ký còn hiệu lực (lịch sử hủy được giữ)
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
      AND cr.Status = N'Đã đăng ký'
    ORDER BY cr.ClassRegistrationId DESC;
END
GO

-- 5) Admin hủy gói: chỉ gói CÒN HẠN hoặc ĐANG TẠM NGƯNG được hủy.
--    KHÔNG xóa Payments (lịch sử thanh toán bảo toàn), KHÔNG hoàn tiền.
IF OBJECT_ID('sp_PackageRegistration_Cancel', 'P') IS NOT NULL DROP PROCEDURE sp_PackageRegistration_Cancel;
GO
CREATE PROCEDURE dbo.sp_PackageRegistration_Cancel
    @RegistrationId INT, @Reason NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PackageRegistrations
    SET Status = N'Đã hủy', CancelDate = GETDATE(), CancelReason = @Reason
    WHERE RegistrationId = @RegistrationId
      AND Status IN (N'Còn hạn', N'Tạm ngưng');
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

-- 6) (Task 03, sửa trong quá trình kiểm thử) Trigger trg_CapNhatTrangThaiHetHan
--    gắn trên bảng đã đổi tên thành PackageRegistrations nhưng thân trigger vẫn
--    tham chiếu tên CŨ (DangKyGoiTap / MaDK / NgayKetThuc / TrangThai) ->
--    mọi INSERT/UPDATE vào PackageRegistrations lỗi "Invalid object name
--    'DangKyGoiTap'". Tạo lại trigger với tên đối tượng hiện tại.
ALTER TRIGGER trg_CapNhatTrangThaiHetHan
ON dbo.PackageRegistrations
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    -- Tự chuyển gói hết hạn: EndDate đã qua -> Status = N'Hết hạn'
    UPDATE r SET r.Status = N'Hết hạn'
    FROM dbo.PackageRegistrations r
    INNER JOIN inserted i ON r.RegistrationId = i.RegistrationId
    WHERE r.EndDate < CAST(GETDATE() AS DATE) AND r.Status = N'Còn hạn';
END
GO
