-- ============================================================
-- Bước 8: Stored procedures cho CRUD còn thiếu
--   Classes / Trainers / Packages: Update + Delete (soft delete Status = 0)
--   Members: GetById
-- Chạy thủ công trên DB QL_GymFitness_Pro (KHÔNG tự chạy)
-- ============================================================

-- ===================== CLASSES =====================
IF OBJECT_ID('sp_Class_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Class_Update;
GO
CREATE PROCEDURE sp_Class_Update
    @ClassId INT,
    @BranchId INT = NULL,
    @ClassName NVARCHAR(200),
    @TrainerId INT = NULL,
    @Capacity INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Classes
    SET BranchId = @BranchId,
        ClassName = @ClassName,
        TrainerId = @TrainerId,
        Capacity = @Capacity
    WHERE ClassId = @ClassId;
END
GO

IF OBJECT_ID('sp_Class_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Class_Delete;
GO
CREATE PROCEDURE sp_Class_Delete
    @ClassId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Classes SET Status = 0 WHERE ClassId = @ClassId;
END
GO

-- ===================== TRAINERS =====================
IF OBJECT_ID('sp_Trainer_Update', 'P') IS NOT NULL DROP PROCEDURE sp_Trainer_Update;
GO
CREATE PROCEDURE sp_Trainer_Update
    @TrainerId INT,
    @BranchId INT = NULL,
    @FullName NVARCHAR(200),
    @Specialty NVARCHAR(200) = NULL,
    @PhoneNumber NVARCHAR(50) = NULL,
    @HireDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Trainers
    SET BranchId = @BranchId,
        FullName = @FullName,
        Specialty = @Specialty,
        PhoneNumber = @PhoneNumber,
        HireDate = @HireDate
    WHERE TrainerId = @TrainerId;
END
GO

IF OBJECT_ID('sp_Trainer_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_Trainer_Delete;
GO
CREATE PROCEDURE sp_Trainer_Delete
    @TrainerId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Trainers SET Status = 0 WHERE TrainerId = @TrainerId;
END
GO

-- ===================== PACKAGES =====================
IF OBJECT_ID('sp_MembershipPackage_Update', 'P') IS NOT NULL DROP PROCEDURE sp_MembershipPackage_Update;
GO
CREATE PROCEDURE sp_MembershipPackage_Update
    @PackageId INT,
    @PackageName NVARCHAR(200),
    @PackageType NVARCHAR(100) = NULL,
    @DurationInMonths INT,
    @UnitPrice DECIMAL(18,0),
    @Description NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MembershipPackages
    SET PackageName = @PackageName,
        PackageType = ISNULL(@PackageType, PackageType),
        DurationInMonths = @DurationInMonths,
        UnitPrice = @UnitPrice,
        Description = @Description
    WHERE PackageId = @PackageId;
END
GO

IF OBJECT_ID('sp_MembershipPackage_Delete', 'P') IS NOT NULL DROP PROCEDURE sp_MembershipPackage_Delete;
GO
CREATE PROCEDURE sp_MembershipPackage_Delete
    @PackageId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE MembershipPackages SET Status = 0 WHERE PackageId = @PackageId;
END
GO

-- ===================== MEMBERS =====================
IF OBJECT_ID('sp_Member_GetById', 'P') IS NOT NULL DROP PROCEDURE sp_Member_GetById;
GO
CREATE PROCEDURE sp_Member_GetById
    @MemberId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MemberId, BranchId, AccountId, FullName, Gender, DateOfBirth,
           PhoneNumber, Email, Address, ProfileImage, ReferrerMemberId, JoinDate, Status
    FROM Members
    WHERE MemberId = @MemberId;
END
GO
