-- ============================================================
-- Task 02 bugfix: sp_Member_Create / sp_Member_Update cho phép
-- các trường tùy chọn NULL (Gender, DateOfBirth, PhoneNumber,
-- Email, Address) để khớp với code gửi DBNull khi trống.
-- (Trước đây đăng kí không nhập SĐT -> lỗi "expects parameter
-- @PhoneNumber which was not supplied".)
-- Giữ nguyên tên SP, chỉ cho thêm default NULL. Chạy lại an toàn.
-- Chạy thủ công trên DB QL_GymFitness_Pro (KHÔNG tự chạy)
-- ============================================================

-- QUAN TRỌNG: khi chạy bằng sqlcmd, session mặc định QUOTED_IDENTIFIER OFF
-- và SP sẽ "đóng dấu" tùy chọn này lúc tạo -> INSERT/UPDATE vào bảng có
-- filtered index sẽ lỗi. Phải bật ON trước khi tạo SP.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('sp_Member_Create', 'P') IS NOT NULL DROP PROCEDURE sp_Member_Create;
GO
CREATE PROCEDURE dbo.sp_Member_Create
    @BranchId INT = NULL,
    @FullName NVARCHAR(100),
    @Gender NVARCHAR(10) = NULL,
    @DateOfBirth DATE = NULL,
    @PhoneNumber VARCHAR(15) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Address NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- PhoneNumber là cột NOT NULL trong schema bảng (không có DEFAULT),
    -- nên khi người dùng không nhập SĐT ta lưu chuỗi rỗng thay vì NULL.
    INSERT INTO dbo.Members (BranchId, FullName, Gender, DateOfBirth, PhoneNumber, Email, Address)
    VALUES (@BranchId, @FullName, @Gender, @DateOfBirth, ISNULL(@PhoneNumber, ''), @Email, @Address);
    SELECT SCOPE_IDENTITY() AS MemberId;
END
GO

IF OBJECT_ID('sp_Member_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Member_Update;
GO
CREATE PROCEDURE dbo.sp_Member_Update
    @MemberId INT,
    @BranchId INT = NULL,
    @FullName NVARCHAR(100),
    @Gender NVARCHAR(10) = NULL,
    @DateOfBirth DATE = NULL,
    @PhoneNumber VARCHAR(15) = NULL,
    @Email NVARCHAR(100) = NULL,
    @Address NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- PhoneNumber là cột NOT NULL trong schema bảng -> giữ chuỗi rỗng thay vì NULL.
    UPDATE dbo.Members
    SET BranchId = @BranchId, FullName = @FullName, Gender = @Gender, DateOfBirth = @DateOfBirth,
        PhoneNumber = ISNULL(@PhoneNumber, ''), Email = @Email, Address = @Address
    WHERE MemberId = @MemberId;
END
GO
