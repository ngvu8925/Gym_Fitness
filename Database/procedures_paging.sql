-- ============================================================
-- Bước 13: Phân trang DB cho các endpoint list
--   sp_Member_Search / sp_MembershipPackage_GetAll /
--   sp_Class_GetAll / sp_Trainer_GetAll
-- Thêm tham số OPTIONAL: @Page, @PageSize, @SortDir (+ @Keyword cho 3 SP list).
--   - Không truyền @Page  => trả TẤT CẢ (tương thích 100% code cũ)
--   - Truyền @Page        => trả đúng trang, kèm cột TotalCount (COUNT(*) OVER())
-- Chạy thủ công trên DB QL_GymFitness_Pro (KHÔNG tự chạy)
-- ============================================================

-- ===================== MEMBERS =====================
IF OBJECT_ID('sp_Member_Search', 'P') IS NOT NULL DROP PROCEDURE sp_Member_Search;
GO
CREATE PROCEDURE dbo.sp_Member_Search
    @Keyword NVARCHAR(100) = NULL,
    @Page INT = NULL,
    @PageSize INT = 10,
    @SortDir NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    -- Chế độ cũ: không phân trang
    IF @Page IS NULL
    BEGIN
        SELECT * FROM dbo.Members
        WHERE Status = 1
          AND (@Keyword IS NULL OR FullName LIKE '%' + @Keyword + '%' OR PhoneNumber LIKE '%' + @Keyword + '%' OR Email LIKE '%' + @Keyword + '%')
        ORDER BY FullName;
        RETURN;
    END

    -- Chế độ phân trang: trả đúng trang + tổng số bản ghi
    DECLARE @Skip INT = (@Page - 1) * @PageSize;

    IF @SortDir = 'DESC'
        SELECT *, COUNT(*) OVER() AS TotalCount
        FROM dbo.Members
        WHERE Status = 1
          AND (@Keyword IS NULL OR FullName LIKE '%' + @Keyword + '%' OR PhoneNumber LIKE '%' + @Keyword + '%' OR Email LIKE '%' + @Keyword + '%')
        ORDER BY FullName DESC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;
    ELSE
        SELECT *, COUNT(*) OVER() AS TotalCount
        FROM dbo.Members
        WHERE Status = 1
          AND (@Keyword IS NULL OR FullName LIKE '%' + @Keyword + '%' OR PhoneNumber LIKE '%' + @Keyword + '%' OR Email LIKE '%' + @Keyword + '%')
        ORDER BY FullName ASC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ===================== PACKAGES =====================
IF OBJECT_ID('sp_MembershipPackage_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_MembershipPackage_GetAll;
GO
CREATE PROCEDURE dbo.sp_MembershipPackage_GetAll
    @Keyword NVARCHAR(100) = NULL,
    @Page INT = NULL,
    @PageSize INT = 10,
    @SortDir NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    IF @Page IS NULL
    BEGIN
        SELECT * FROM dbo.MembershipPackages
        WHERE Status = 1
          AND (@Keyword IS NULL OR PackageName LIKE '%' + @Keyword + '%' OR PackageType LIKE '%' + @Keyword + '%')
        ORDER BY PackageName;
        RETURN;
    END

    DECLARE @Skip INT = (@Page - 1) * @PageSize;

    IF @SortDir = 'DESC'
        SELECT *, COUNT(*) OVER() AS TotalCount
        FROM dbo.MembershipPackages
        WHERE Status = 1
          AND (@Keyword IS NULL OR PackageName LIKE '%' + @Keyword + '%' OR PackageType LIKE '%' + @Keyword + '%')
        ORDER BY PackageName DESC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;
    ELSE
        SELECT *, COUNT(*) OVER() AS TotalCount
        FROM dbo.MembershipPackages
        WHERE Status = 1
          AND (@Keyword IS NULL OR PackageName LIKE '%' + @Keyword + '%' OR PackageType LIKE '%' + @Keyword + '%')
        ORDER BY PackageName ASC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ===================== CLASSES =====================
IF OBJECT_ID('sp_Class_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Class_GetAll;
GO
CREATE PROCEDURE dbo.sp_Class_GetAll
    @Keyword NVARCHAR(100) = NULL,
    @Page INT = NULL,
    @PageSize INT = 10,
    @SortDir NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    IF @Page IS NULL
    BEGIN
        SELECT c.ClassId, c.ClassName, c.Capacity, c.BranchId,
               t.TrainerId, t.FullName AS TrainerName
        FROM dbo.Classes c
        LEFT JOIN dbo.Trainers t ON t.TrainerId = c.TrainerId
        WHERE c.Status = 1
          AND (@Keyword IS NULL OR c.ClassName LIKE '%' + @Keyword + '%' OR t.FullName LIKE '%' + @Keyword + '%')
        ORDER BY c.ClassName;
        RETURN;
    END

    DECLARE @Skip INT = (@Page - 1) * @PageSize;

    IF @SortDir = 'DESC'
        SELECT c.ClassId, c.ClassName, c.Capacity, c.BranchId,
               t.TrainerId, t.FullName AS TrainerName,
               COUNT(*) OVER() AS TotalCount
        FROM dbo.Classes c
        LEFT JOIN dbo.Trainers t ON t.TrainerId = c.TrainerId
        WHERE c.Status = 1
          AND (@Keyword IS NULL OR c.ClassName LIKE '%' + @Keyword + '%' OR t.FullName LIKE '%' + @Keyword + '%')
        ORDER BY c.ClassName DESC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;
    ELSE
        SELECT c.ClassId, c.ClassName, c.Capacity, c.BranchId,
               t.TrainerId, t.FullName AS TrainerName,
               COUNT(*) OVER() AS TotalCount
        FROM dbo.Classes c
        LEFT JOIN dbo.Trainers t ON t.TrainerId = c.TrainerId
        WHERE c.Status = 1
          AND (@Keyword IS NULL OR c.ClassName LIKE '%' + @Keyword + '%' OR t.FullName LIKE '%' + @Keyword + '%')
        ORDER BY c.ClassName ASC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ===================== TRAINERS =====================
IF OBJECT_ID('sp_Trainer_GetAll', 'P') IS NOT NULL DROP PROCEDURE sp_Trainer_GetAll;
GO
CREATE PROCEDURE dbo.sp_Trainer_GetAll
    @Keyword NVARCHAR(100) = NULL,
    @Page INT = NULL,
    @PageSize INT = 10,
    @SortDir NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    IF @Page IS NULL
    BEGIN
        SELECT * FROM dbo.Trainers
        WHERE Status = 1
          AND (@Keyword IS NULL OR FullName LIKE '%' + @Keyword + '%' OR Specialty LIKE '%' + @Keyword + '%')
        ORDER BY FullName;
        RETURN;
    END

    DECLARE @Skip INT = (@Page - 1) * @PageSize;

    IF @SortDir = 'DESC'
        SELECT *, COUNT(*) OVER() AS TotalCount
        FROM dbo.Trainers
        WHERE Status = 1
          AND (@Keyword IS NULL OR FullName LIKE '%' + @Keyword + '%' OR Specialty LIKE '%' + @Keyword + '%')
        ORDER BY FullName DESC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;
    ELSE
        SELECT *, COUNT(*) OVER() AS TotalCount
        FROM dbo.Trainers
        WHERE Status = 1
          AND (@Keyword IS NULL OR FullName LIKE '%' + @Keyword + '%' OR Specialty LIKE '%' + @Keyword + '%')
        ORDER BY FullName ASC
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO
