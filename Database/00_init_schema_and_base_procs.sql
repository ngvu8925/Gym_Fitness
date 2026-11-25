-- ============================================================
-- 00_init_schema_and_base_procs.sql
-- Tạo database QL_GymFitness_Pro, toàn bộ bảng gốc, trigger,
-- dữ liệu mẫu tối thiểu và các Stored Procedures cơ sở còn thiếu.
-- Script chạy lại nhiều lần an toàn (idempotent).
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'QL_GymFitness_Pro') IS NULL
BEGIN
    CREATE DATABASE QL_GymFitness_Pro;
END
GO

USE QL_GymFitness_Pro;
GO

-- ===================== 1. TABLES =====================

IF OBJECT_ID(N'dbo.Branches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Branches (
        BranchId    INT IDENTITY(1,1) PRIMARY KEY,
        BranchName  NVARCHAR(200) NOT NULL,
        Address     NVARCHAR(500) NULL,
        PhoneNumber NVARCHAR(50)  NULL,
        Status      BIT           NOT NULL CONSTRAINT DF_Branches_Status DEFAULT 1
    );
END
GO

IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts (
        AccountId   INT IDENTITY(1,1) PRIMARY KEY,
        Username    NVARCHAR(100) NOT NULL,
        Password    NVARCHAR(255) NOT NULL,
        FullName    NVARCHAR(200) NOT NULL,
        Role        NVARCHAR(50)  NOT NULL CONSTRAINT DF_Accounts_Role DEFAULT N'Member',
        BranchId    INT           NULL CONSTRAINT FK_Accounts_Branches REFERENCES dbo.Branches(BranchId),
        PhoneNumber NVARCHAR(50)  NULL,
        Email       NVARCHAR(255) NULL,
        Status      BIT           NOT NULL CONSTRAINT DF_Accounts_Status DEFAULT 1,
        CreatedAt   DATETIME      NOT NULL CONSTRAINT DF_Accounts_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT UQ_Accounts_Username UNIQUE (Username)
    );
END
GO

IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Members (
        MemberId         INT IDENTITY(1,1) PRIMARY KEY,
        BranchId         INT           NULL CONSTRAINT FK_Members_Branches REFERENCES dbo.Branches(BranchId),
        AccountId        INT           NULL CONSTRAINT FK_Members_Accounts REFERENCES dbo.Accounts(AccountId),
        FullName         NVARCHAR(200) NOT NULL,
        Gender           NVARCHAR(20)  NULL,
        DateOfBirth      DATE          NULL,
        PhoneNumber      NVARCHAR(50)  NOT NULL CONSTRAINT DF_Members_PhoneNumber DEFAULT '',
        Email            NVARCHAR(255) NULL,
        Address          NVARCHAR(500) NULL,
        ProfileImage     NVARCHAR(500) NULL,
        ReferrerMemberId INT           NULL,
        JoinDate         DATE          NOT NULL CONSTRAINT DF_Members_JoinDate DEFAULT CAST(GETDATE() AS DATE),
        Status           BIT           NOT NULL CONSTRAINT DF_Members_Status DEFAULT 1
    );
END
GO

IF OBJECT_ID(N'dbo.MembershipPackages', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MembershipPackages (
        PackageId        INT IDENTITY(1,1) PRIMARY KEY,
        PackageName      NVARCHAR(200)  NOT NULL,
        PackageType      NVARCHAR(100)  NOT NULL CONSTRAINT DF_MembershipPackages_Type DEFAULT N'Cơ bản',
        DurationInMonths INT            NOT NULL,
        UnitPrice        DECIMAL(18, 2) NOT NULL,
        Description      NVARCHAR(500)  NULL,
        Status           BIT            NOT NULL CONSTRAINT DF_MembershipPackages_Status DEFAULT 1
    );
END
GO

IF OBJECT_ID(N'dbo.Promotions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Promotions (
        PromotionId     INT IDENTITY(1,1) PRIMARY KEY,
        PromoCode       NVARCHAR(50)   NOT NULL,
        DiscountPercent DECIMAL(5, 2)  NOT NULL CONSTRAINT DF_Promotions_Percent DEFAULT 0,
        DiscountAmount  DECIMAL(18, 2) NOT NULL CONSTRAINT DF_Promotions_Amount DEFAULT 0,
        StartDate       DATE           NULL,
        EndDate         DATE           NULL,
        Status          BIT            NOT NULL CONSTRAINT DF_Promotions_Status DEFAULT 1,
        CONSTRAINT UQ_Promotions_PromoCode UNIQUE (PromoCode)
    );
END
GO

IF OBJECT_ID(N'dbo.PackageRegistrations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PackageRegistrations (
        RegistrationId INT IDENTITY(1,1) PRIMARY KEY,
        MemberId       INT            NOT NULL CONSTRAINT FK_PkgReg_Members REFERENCES dbo.Members(MemberId),
        PackageId      INT            NOT NULL CONSTRAINT FK_PkgReg_Packages REFERENCES dbo.MembershipPackages(PackageId),
        BranchId       INT            NULL CONSTRAINT FK_PkgReg_Branches REFERENCES dbo.Branches(BranchId),
        AccountId      INT            NULL CONSTRAINT FK_PkgReg_Accounts REFERENCES dbo.Accounts(AccountId),
        PromotionId    INT            NULL CONSTRAINT FK_PkgReg_Promotions REFERENCES dbo.Promotions(PromotionId),
        DiscountAmount DECIMAL(18, 2) NOT NULL CONSTRAINT DF_PkgReg_Discount DEFAULT 0,
        StartDate      DATE           NOT NULL,
        EndDate        DATE           NOT NULL,
        Status         NVARCHAR(50)   NOT NULL CONSTRAINT DF_PkgReg_Status DEFAULT N'Còn hạn'
    );
END
GO

IF OBJECT_ID(N'dbo.PackageSuspensions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PackageSuspensions (
        SuspensionId   INT IDENTITY(1,1) PRIMARY KEY,
        RegistrationId INT           NOT NULL CONSTRAINT FK_PkgSusp_PkgReg REFERENCES dbo.PackageRegistrations(RegistrationId),
        SuspendDate    DATE          NOT NULL CONSTRAINT DF_PkgSusp_SuspendDate DEFAULT CAST(GETDATE() AS DATE),
        ResumeDate     DATE          NULL,
        Reason         NVARCHAR(255) NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments (
        PaymentId      INT IDENTITY(1,1) PRIMARY KEY,
        RegistrationId INT            NOT NULL CONSTRAINT FK_Payments_PkgReg REFERENCES dbo.PackageRegistrations(RegistrationId),
        BranchId       INT            NULL CONSTRAINT FK_Payments_Branches REFERENCES dbo.Branches(BranchId),
        AccountId      INT            NULL CONSTRAINT FK_Payments_Accounts REFERENCES dbo.Accounts(AccountId),
        Amount         DECIMAL(18, 2) NOT NULL,
        PaymentMethod  NVARCHAR(50)   NOT NULL CONSTRAINT DF_Payments_Method DEFAULT N'Tiền mặt',
        PaymentDate    DATETIME       NOT NULL CONSTRAINT DF_Payments_Date DEFAULT GETDATE()
    );
END
GO

IF OBJECT_ID(N'dbo.Trainers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Trainers (
        TrainerId   INT IDENTITY(1,1) PRIMARY KEY,
        BranchId    INT           NULL CONSTRAINT FK_Trainers_Branches REFERENCES dbo.Branches(BranchId),
        FullName    NVARCHAR(200) NOT NULL,
        Specialty   NVARCHAR(200) NULL,
        PhoneNumber NVARCHAR(50)  NULL,
        HireDate    DATE          NULL,
        Status      BIT           NOT NULL CONSTRAINT DF_Trainers_Status DEFAULT 1
    );
END
GO

IF OBJECT_ID(N'dbo.Classes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Classes (
        ClassId   INT IDENTITY(1,1) PRIMARY KEY,
        BranchId  INT           NULL CONSTRAINT FK_Classes_Branches REFERENCES dbo.Branches(BranchId),
        ClassName NVARCHAR(200) NOT NULL,
        TrainerId INT           NULL CONSTRAINT FK_Classes_Trainers REFERENCES dbo.Trainers(TrainerId),
        Capacity  INT           NOT NULL CONSTRAINT DF_Classes_Capacity DEFAULT 20,
        Status    BIT           NOT NULL CONSTRAINT DF_Classes_Status DEFAULT 1
    );
END
GO

IF OBJECT_ID(N'dbo.ClassRegistrations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassRegistrations (
        ClassRegistrationId INT IDENTITY(1,1) PRIMARY KEY,
        MemberId            INT      NOT NULL CONSTRAINT FK_ClassReg_Members REFERENCES dbo.Members(MemberId),
        ClassId             INT      NOT NULL CONSTRAINT FK_ClassReg_Classes REFERENCES dbo.Classes(ClassId),
        RegisteredAt        DATETIME NOT NULL CONSTRAINT DF_ClassReg_RegisteredAt DEFAULT GETDATE()
    );
END
GO

IF OBJECT_ID(N'dbo.Attendances', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Attendances (
        AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
        MemberId     INT      NOT NULL CONSTRAINT FK_Attendances_Members REFERENCES dbo.Members(MemberId),
        BranchId     INT      NULL CONSTRAINT FK_Attendances_Branches REFERENCES dbo.Branches(BranchId),
        CheckInTime  DATETIME NOT NULL CONSTRAINT DF_Attendances_CheckIn DEFAULT GETDATE(),
        CheckOutTime DATETIME NULL
    );
END
GO

IF OBJECT_ID(N'dbo.SystemLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SystemLogs (
        LogId       INT IDENTITY(1,1) PRIMARY KEY,
        AccountId   INT            NULL CONSTRAINT FK_SystemLogs_Accounts REFERENCES dbo.Accounts(AccountId),
        Action      NVARCHAR(40)   NOT NULL,
        TableName   NVARCHAR(100)  NULL,
        Description NVARCHAR(1000) NULL,
        Timestamp   DATETIME       NOT NULL CONSTRAINT DF_SystemLogs_Timestamp DEFAULT GETDATE()
    );
END
GO

-- ===================== 2. TRIGGER (cần có trước khi procedures_cancellations.sql chạy ALTER TRIGGER) =====================

IF OBJECT_ID(N'dbo.trg_CapNhatTrangThaiHetHan', N'TR') IS NULL
BEGIN
    EXEC(N'
        CREATE TRIGGER dbo.trg_CapNhatTrangThaiHetHan
        ON dbo.PackageRegistrations
        AFTER INSERT, UPDATE
        AS
        BEGIN
            SET NOCOUNT ON;
            UPDATE r SET r.Status = N''Hết hạn''
            FROM dbo.PackageRegistrations r
            INNER JOIN inserted i ON r.RegistrationId = i.RegistrationId
            WHERE r.EndDate < CAST(GETDATE() AS DATE) AND r.Status = N''Còn hạn'';
        END
    ');
END
GO

-- ===================== 3. BASE STORED PROCEDURES =====================

-- Members: Delete (soft delete)
IF OBJECT_ID(N'dbo.sp_Member_Delete', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Member_Delete;
GO
CREATE PROCEDURE dbo.sp_Member_Delete
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Members SET Status = 0 WHERE MemberId = @MemberId;
END
GO

-- MembershipPackages: Create
IF OBJECT_ID(N'dbo.sp_MembershipPackage_Create', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_MembershipPackage_Create;
GO
CREATE PROCEDURE dbo.sp_MembershipPackage_Create
    @PackageName NVARCHAR(200),
    @DurationInMonths INT,
    @UnitPrice DECIMAL(18, 2),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.MembershipPackages (PackageName, PackageType, DurationInMonths, UnitPrice, Description, Status)
    VALUES (@PackageName, N'Cơ bản', @DurationInMonths, @UnitPrice, @Description, 1);
    SELECT SCOPE_IDENTITY() AS PackageId;
END
GO

-- Trainers: Create
IF OBJECT_ID(N'dbo.sp_Trainer_Create', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Trainer_Create;
GO
CREATE PROCEDURE dbo.sp_Trainer_Create
    @BranchId INT = NULL,
    @FullName NVARCHAR(200),
    @Specialty NVARCHAR(200) = NULL,
    @PhoneNumber NVARCHAR(50) = NULL,
    @HireDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Trainers (BranchId, FullName, Specialty, PhoneNumber, HireDate, Status)
    VALUES (@BranchId, @FullName, @Specialty, @PhoneNumber, @HireDate, 1);
    SELECT SCOPE_IDENTITY() AS TrainerId;
END
GO

-- ClassRegistrations: Create
IF OBJECT_ID(N'dbo.sp_ClassRegistration_Create', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_ClassRegistration_Create;
GO
CREATE PROCEDURE dbo.sp_ClassRegistration_Create
    @MemberId INT,
    @ClassId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Capacity INT;
    SELECT @Capacity = Capacity FROM dbo.Classes WHERE ClassId = @ClassId AND Status = 1;
    IF @Capacity IS NULL
    BEGIN
        RAISERROR(N'Lớp học không tồn tại hoặc đã đóng.', 16, 1);
        RETURN;
    END

    DECLARE @CurrentCount INT = (
        SELECT COUNT(*) FROM dbo.ClassRegistrations
        WHERE ClassId = @ClassId
    );
    IF @CurrentCount >= @Capacity
    BEGIN
        RAISERROR(N'Lớp học đã đủ sĩ số.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.ClassRegistrations (MemberId, ClassId, RegisteredAt)
    VALUES (@MemberId, @ClassId, GETDATE());

    SELECT SCOPE_IDENTITY() AS ClassRegistrationId;
END
GO

-- PackageRegistrations: Create + tạo hóa đơn Payments
IF OBJECT_ID(N'dbo.sp_PackageRegistration_Create', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_PackageRegistration_Create;
GO
CREATE PROCEDURE dbo.sp_PackageRegistration_Create
    @MemberId INT,
    @PackageId INT,
    @BranchId INT = NULL,
    @StartDate DATE,
    @AccountId INT = NULL,
    @PromoCode NVARCHAR(50) = NULL,
    @PaymentMethod NVARCHAR(50) = N'Tiền mặt'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Months INT, @UnitPrice DECIMAL(18, 2);
    SELECT @Months = DurationInMonths, @UnitPrice = UnitPrice
    FROM dbo.MembershipPackages
    WHERE PackageId = @PackageId AND Status = 1;

    IF @Months IS NULL
    BEGIN
        RAISERROR(N'Gói tập không tồn tại hoặc đã ngừng kinh doanh.', 16, 1);
        RETURN;
    END

    DECLARE @PromotionId INT = NULL;
    DECLARE @DiscountAmount DECIMAL(18, 2) = 0;

    IF @PromoCode IS NOT NULL AND LTRIM(RTRIM(@PromoCode)) <> ''
    BEGIN
        DECLARE @Pct DECIMAL(5, 2) = 0, @Amt DECIMAL(18, 2) = 0;
        SELECT @PromotionId = PromotionId, @Pct = DiscountPercent, @Amt = DiscountAmount
        FROM dbo.Promotions
        WHERE PromoCode = @PromoCode
          AND Status = 1
          AND (StartDate IS NULL OR StartDate <= CAST(GETDATE() AS DATE))
          AND (EndDate IS NULL OR EndDate >= CAST(GETDATE() AS DATE));

        IF @PromotionId IS NOT NULL
        BEGIN
            SET @DiscountAmount = CASE WHEN @Amt > 0 THEN @Amt ELSE ROUND(@UnitPrice * @Pct / 100.0, 0) END;
            IF @DiscountAmount > @UnitPrice SET @DiscountAmount = @UnitPrice;
        END
    END

    DECLARE @EndDate DATE = DATEADD(MONTH, @Months, @StartDate);
    DECLARE @FinalAmount DECIMAL(18, 2) = @UnitPrice - @DiscountAmount;

    INSERT INTO dbo.PackageRegistrations
        (MemberId, PackageId, BranchId, AccountId, PromotionId, DiscountAmount, StartDate, EndDate, Status)
    VALUES
        (@MemberId, @PackageId, @BranchId, @AccountId, @PromotionId, @DiscountAmount, @StartDate, @EndDate, N'Còn hạn');

    DECLARE @RegistrationId INT = SCOPE_IDENTITY();

    INSERT INTO dbo.Payments (RegistrationId, BranchId, AccountId, Amount, PaymentMethod, PaymentDate)
    VALUES (@RegistrationId, @BranchId, @AccountId, @FinalAmount, ISNULL(@PaymentMethod, N'Tiền mặt'), GETDATE());

    SELECT @RegistrationId AS RegistrationId, @DiscountAmount AS DiscountAmount;
END
GO

-- PackageRegistrations: Suspend
IF OBJECT_ID(N'dbo.sp_PackageRegistration_Suspend', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_PackageRegistration_Suspend;
GO
CREATE PROCEDURE dbo.sp_PackageRegistration_Suspend
    @RegistrationId INT,
    @Reason NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.PackageRegistrations WHERE RegistrationId = @RegistrationId AND Status = N'Còn hạn')
    BEGIN
        RAISERROR(N'Chỉ có thể tạm ngưng gói tập đang còn hạn.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.PackageSuspensions (RegistrationId, SuspendDate, Reason)
    VALUES (@RegistrationId, CAST(GETDATE() AS DATE), @Reason);

    UPDATE dbo.PackageRegistrations
    SET Status = N'Tạm ngưng'
    WHERE RegistrationId = @RegistrationId;
END
GO

-- PackageRegistrations: Resume
IF OBJECT_ID(N'dbo.sp_PackageRegistration_Resume', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_PackageRegistration_Resume;
GO
CREATE PROCEDURE dbo.sp_PackageRegistration_Resume
    @SuspensionId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @RegistrationId INT, @SuspendDate DATE;
    SELECT @RegistrationId = RegistrationId, @SuspendDate = SuspendDate
    FROM dbo.PackageSuspensions
    WHERE SuspensionId = @SuspensionId AND ResumeDate IS NULL;

    IF @RegistrationId IS NULL
    BEGIN
        RAISERROR(N'Không tìm thấy lượt tạm ngưng hợp lệ.', 16, 1);
        RETURN;
    END

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    DECLARE @PausedDays INT = DATEDIFF(DAY, @SuspendDate, @Today);
    IF @PausedDays < 0 SET @PausedDays = 0;

    UPDATE dbo.PackageSuspensions SET ResumeDate = @Today WHERE SuspensionId = @SuspensionId;
    UPDATE dbo.PackageRegistrations
    SET EndDate = DATEADD(DAY, @PausedDays, EndDate),
        Status = N'Còn hạn'
    WHERE RegistrationId = @RegistrationId;
END
GO

-- Expiring soon packages
IF OBJECT_ID(N'dbo.sp_Member_GetExpiringSoon', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Member_GetExpiringSoon;
GO
CREATE PROCEDURE dbo.sp_Member_GetExpiringSoon
    @Days INT = 7
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    SELECT m.MemberId, m.FullName, m.PhoneNumber, p.PackageName, r.StartDate, r.EndDate
    FROM dbo.PackageRegistrations r
    INNER JOIN dbo.Members m ON m.MemberId = r.MemberId
    INNER JOIN dbo.MembershipPackages p ON p.PackageId = r.PackageId
    WHERE r.Status = N'Còn hạn'
      AND r.EndDate >= @Today
      AND r.EndDate <= DATEADD(DAY, @Days, @Today)
    ORDER BY r.EndDate ASC;
END
GO

-- Attendance: Check-in
IF OBJECT_ID(N'dbo.sp_Attendance_CheckIn', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Attendance_CheckIn;
GO
CREATE PROCEDURE dbo.sp_Attendance_CheckIn
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (
        SELECT 1 FROM dbo.PackageRegistrations
        WHERE MemberId = @MemberId
          AND Status = N'Còn hạn'
          AND EndDate >= CAST(GETDATE() AS DATE)
    )
    BEGIN
        RAISERROR(N'Hội viên chưa có gói tập còn hiệu lực để check-in.', 16, 1);
        RETURN;
    END

    DECLARE @BranchId INT = (SELECT BranchId FROM dbo.Members WHERE MemberId = @MemberId);
    INSERT INTO dbo.Attendances (MemberId, BranchId, CheckInTime)
    VALUES (@MemberId, @BranchId, GETDATE());
END
GO

-- Attendance: Check-out
IF OBJECT_ID(N'dbo.sp_Attendance_CheckOut', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Attendance_CheckOut;
GO
CREATE PROCEDURE dbo.sp_Attendance_CheckOut
    @AttendanceId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Attendances
    SET CheckOutTime = GETDATE()
    WHERE AttendanceId = @AttendanceId AND CheckOutTime IS NULL;
END
GO

-- Reports: Monthly Revenue
IF OBJECT_ID(N'dbo.sp_Report_MonthlyRevenue', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Report_MonthlyRevenue;
GO
CREATE PROCEDURE dbo.sp_Report_MonthlyRevenue
    @Year INT,
    @Month INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @Year AS [Year],
           @Month AS [Month],
           ISNULL(SUM(Amount), 0) AS TotalRevenue
    FROM dbo.Payments
    WHERE YEAR(PaymentDate) = @Year AND MONTH(PaymentDate) = @Month;
END
GO

-- Reports: All Months Revenue
IF OBJECT_ID(N'dbo.sp_Report_AllMonthsRevenue', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Report_AllMonthsRevenue;
GO
CREATE PROCEDURE dbo.sp_Report_AllMonthsRevenue
AS
BEGIN
    SET NOCOUNT ON;
    SELECT YEAR(PaymentDate) AS [Year],
           MONTH(PaymentDate) AS [Month],
           SUM(Amount) AS TotalRevenue
    FROM dbo.Payments
    GROUP BY YEAR(PaymentDate), MONTH(PaymentDate)
    ORDER BY YEAR(PaymentDate) ASC, MONTH(PaymentDate) ASC;
END
GO

-- Reports: Best Selling Packages
IF OBJECT_ID(N'dbo.sp_Report_BestSellingPackages', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Report_BestSellingPackages;
GO
CREATE PROCEDURE dbo.sp_Report_BestSellingPackages
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.PackageName,
           COUNT(r.RegistrationId) AS RegistrationCount,
           ISNULL(SUM(pay.Amount), 0) AS Revenue
    FROM dbo.MembershipPackages p
    LEFT JOIN dbo.PackageRegistrations r ON r.PackageId = p.PackageId
    LEFT JOIN dbo.Payments pay ON pay.RegistrationId = r.RegistrationId
    WHERE p.Status = 1
    GROUP BY p.PackageId, p.PackageName
    ORDER BY RegistrationCount DESC, Revenue DESC;
END
GO

-- Reports: Revenue By Branch
IF OBJECT_ID(N'dbo.sp_Report_RevenueByBranch', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_Report_RevenueByBranch;
GO
CREATE PROCEDURE dbo.sp_Report_RevenueByBranch
AS
BEGIN
    SET NOCOUNT ON;
    SELECT b.BranchId,
           b.BranchName,
           ISNULL(SUM(pay.Amount), 0) AS TotalRevenue
    FROM dbo.Branches b
    LEFT JOIN dbo.Payments pay ON pay.BranchId = b.BranchId
    GROUP BY b.BranchId, b.BranchName
    ORDER BY TotalRevenue DESC;
END
GO

-- ===================== 4. SEED DATA TỐI THIỂU =====================

IF NOT EXISTS (SELECT 1 FROM dbo.Branches)
BEGIN
    INSERT INTO dbo.Branches (BranchName, Address, PhoneNumber, Status)
    VALUES
        (N'Chi nhánh Trung tâm', N'123 Đường Lê Lợi, Quận 1', '0901000001', 1),
        (N'Chi nhánh Thủ Đức', N'456 Võ Văn Ngân, TP. Thủ Đức', '0901000002', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.MembershipPackages)
BEGIN
    INSERT INTO dbo.MembershipPackages (PackageName, PackageType, DurationInMonths, UnitPrice, Description, Status)
    VALUES
        (N'Gói Cơ Bản 1 Tháng', N'Cơ bản', 1, 500000, N'Tập gym không giới hạn khung giờ trong 1 tháng', 1),
        (N'Gói Tiêu Chuẩn 3 Tháng', N'Tiêu chuẩn', 3, 1350000, N'Tập gym + lớp nhóm trong 3 tháng', 1),
        (N'Gói VIP 12 Tháng', N'VIP', 12, 4800000, N'Full dịch vụ Gym, Yoga, Sauna trong 12 tháng', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Trainers)
BEGIN
    INSERT INTO dbo.Trainers (BranchId, FullName, Specialty, PhoneNumber, HireDate, Status)
    VALUES
        (1, N'Nguyễn Văn Hùng', N'Thể hình & Tăng cơ', '0902000001', '2025-01-15', 1),
        (1, N'Trần Thị Mai', N'Yoga & Pilates', '0902000002', '2025-03-01', 1);
END
GO

-- Seed tài khoản admin (hash SHA-256 của "Admin@123", tự động nâng cấp sang BCrypt khi đăng nhập lần đầu)
IF NOT EXISTS (SELECT 1 FROM dbo.Accounts WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.Accounts (Username, Password, FullName, Role, BranchId, Status, CreatedAt)
    VALUES (
        'admin',
        'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7',
        N'System Administrator',
        'Admin',
        1,
        1,
        GETDATE()
    );
END
GO
