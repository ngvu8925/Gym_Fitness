-- ============================================================
-- Task 06: Bảo mật đăng nhập — giới hạn lần sai + khóa tạm + lịch sử
-- Chạy thủ công trên DB QL_GymFitness_Pro (KHÔNG tự chạy)
--
-- Schema chỉ THÊM CỘT MỚI (additive): tài khoản hiện tại vẫn đăng nhập
-- bình thường (FailedLoginCount = 0, LockoutEnd = NULL).
-- KHÔNG ghi mật khẩu/hash vào bất kỳ log nào.
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1) Accounts: đếm lần sai + thời điểm mở khóa (NULL = không bị khóa)
IF COL_LENGTH('Accounts', 'FailedLoginCount') IS NULL
    ALTER TABLE dbo.Accounts
    ADD FailedLoginCount INT NOT NULL CONSTRAINT DF_Accounts_FailedLoginCount DEFAULT 0;
GO
IF COL_LENGTH('Accounts', 'LockoutEnd') IS NULL
    ALTER TABLE dbo.Accounts ADD LockoutEnd DATETIME NULL;
GO

-- 2) Ghi nhận 1 lần đăng nhập SAI; đủ ngưỡng thì khóa tạm @LockoutMinutes phút.
--    Trả về cột trạng thái: FailedLoginCount, LockoutEnd (NULL nếu không khóa),
--    IsLocked (1/0) để code đọc ngay không cần query lại.
IF OBJECT_ID('sp_Account_LoginFailed', 'P') IS NOT NULL DROP PROCEDURE sp_Account_LoginFailed;
GO
CREATE PROCEDURE dbo.sp_Account_LoginFailed
    @AccountId INT,
    @MaxFailedAttempts INT,
    @LockoutMinutes INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Accounts
    SET FailedLoginCount = FailedLoginCount + 1,
        LockoutEnd = CASE
            WHEN FailedLoginCount + 1 >= @MaxFailedAttempts
            THEN DATEADD(MINUTE, @LockoutMinutes, GETDATE())
            ELSE LockoutEnd
        END
    WHERE AccountId = @AccountId;

    SELECT FailedLoginCount, LockoutEnd,
           CASE WHEN LockoutEnd IS NOT NULL AND LockoutEnd > GETDATE() THEN 1 ELSE 0 END AS IsLocked
    FROM dbo.Accounts WHERE AccountId = @AccountId;
END
GO

-- 3) Đăng nhập thành công: reset bộ đếm + xóa khóa.
IF OBJECT_ID('sp_Account_LoginSucceeded', 'P') IS NOT NULL DROP PROCEDURE sp_Account_LoginSucceeded;
GO
CREATE PROCEDURE dbo.sp_Account_LoginSucceeded
    @AccountId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Accounts
    SET FailedLoginCount = 0, LockoutEnd = NULL
    WHERE AccountId = @AccountId;
END
GO

-- 4) Lịch sử bảo mật (Task 06): tái sử dụng bảng SystemLogs có sẵn.
--    AccountId NULL khi không xác định được tài khoản (username không tồn tại).
--    KHÔNG BAO GIỜ ghi mật khẩu/hash vào @Description.
IF OBJECT_ID('sp_SystemLog_Insert', 'P') IS NOT NULL DROP PROCEDURE sp_SystemLog_Insert;
GO
CREATE PROCEDURE dbo.sp_SystemLog_Insert
    @AccountId INT = NULL,
    @Action NVARCHAR(40),
    @TableName NVARCHAR(100) = NULL,
    @Description NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.SystemLogs (AccountId, Action, TableName, Description, Timestamp)
    VALUES (@AccountId, @Action, @TableName, @Description, GETDATE());
    SELECT SCOPE_IDENTITY() AS LogId;
END
GO
