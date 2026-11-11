    -- ============================================================
    -- Bước 4: Stored procedures cho link Account <-> Member
    -- (thay thế phần trước đây dùng EF Core)
    -- Chạy thủ công trên DB QL_GymFitness_Pro (KHÔNG tự chạy)
    -- ============================================================

    -- Lấy thông tin account (dùng để check Role + match email/phone/fullname)
    IF OBJECT_ID('sp_MemberAccount_GetAccount', 'P') IS NOT NULL DROP PROCEDURE sp_MemberAccount_GetAccount;
    GO
    CREATE PROCEDURE sp_MemberAccount_GetAccount
        @AccountId INT
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT AccountId, Username, FullName, Role, Email, PhoneNumber
        FROM Accounts
        WHERE AccountId = @AccountId;
    END
    GO

    -- Lấy member profile đã link với account (AccountId IS NOT NULL)
    IF OBJECT_ID('sp_MemberAccount_GetMemberByAccountId', 'P') IS NOT NULL DROP PROCEDURE sp_MemberAccount_GetMemberByAccountId;
    GO
    CREATE PROCEDURE sp_MemberAccount_GetMemberByAccountId
        @AccountId INT
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT MemberId, BranchId, AccountId, FullName, Gender, DateOfBirth,
               PhoneNumber, Email, Address, ProfileImage, ReferrerMemberId, JoinDate, Status
        FROM Members
        WHERE AccountId = @AccountId;
    END
    GO

    -- Tự động match member CHƯA link với account, theo đúng thứ tự ưu tiên:
    --   email + phone  -> phải khớp cả hai
    --   email          -> khớp email
    --   phone          -> khớp phone
    --   còn lại        -> khớp FullName
    -- Trả về tối đa 2 hàng; service chỉ tự-link khi kết quả đúng 1 hàng.
    IF OBJECT_ID('sp_MemberAccount_FindUnlinkedMatches', 'P') IS NOT NULL DROP PROCEDURE sp_MemberAccount_FindUnlinkedMatches;
    GO
    CREATE PROCEDURE sp_MemberAccount_FindUnlinkedMatches
        @AccountId INT
    AS
    BEGIN
        SET NOCOUNT ON;

        DECLARE @Email NVARCHAR(255) = (SELECT NULLIF(LTRIM(RTRIM(Email)), '') FROM Accounts WHERE AccountId = @AccountId);
        DECLARE @Phone NVARCHAR(50)  = (SELECT NULLIF(LTRIM(RTRIM(PhoneNumber)), '') FROM Accounts WHERE AccountId = @AccountId);
        DECLARE @FullName NVARCHAR(200) = (SELECT NULLIF(LTRIM(RTRIM(FullName)), '') FROM Accounts WHERE AccountId = @AccountId);

        DECLARE @Result TABLE
        (
            MemberId INT, BranchId INT, AccountId INT, FullName NVARCHAR(200), Gender NVARCHAR(20),
            DateOfBirth DATE, PhoneNumber NVARCHAR(50), Email NVARCHAR(255), Address NVARCHAR(500),
            ProfileImage NVARCHAR(500), ReferrerMemberId INT, JoinDate DATE, Status BIT
        );

        IF @Email IS NOT NULL AND @Phone IS NOT NULL
            INSERT INTO @Result
            SELECT TOP 2 MemberId, BranchId, AccountId, FullName, Gender, DateOfBirth, PhoneNumber, Email,
                         Address, ProfileImage, ReferrerMemberId, JoinDate, Status
            FROM Members
            WHERE AccountId IS NULL AND Email = @Email AND PhoneNumber = @Phone;
        ELSE IF @Email IS NOT NULL
            INSERT INTO @Result
            SELECT TOP 2 MemberId, BranchId, AccountId, FullName, Gender, DateOfBirth, PhoneNumber, Email,
                         Address, ProfileImage, ReferrerMemberId, JoinDate, Status
            FROM Members
            WHERE AccountId IS NULL AND Email = @Email;
        ELSE IF @Phone IS NOT NULL
            INSERT INTO @Result
            SELECT TOP 2 MemberId, BranchId, AccountId, FullName, Gender, DateOfBirth, PhoneNumber, Email,
                         Address, ProfileImage, ReferrerMemberId, JoinDate, Status
            FROM Members
            WHERE AccountId IS NULL AND PhoneNumber = @Phone;
        ELSE IF @FullName IS NOT NULL
            INSERT INTO @Result
            SELECT TOP 2 MemberId, BranchId, AccountId, FullName, Gender, DateOfBirth, PhoneNumber, Email,
                         Address, ProfileImage, ReferrerMemberId, JoinDate, Status
            FROM Members
            WHERE AccountId IS NULL AND FullName = @FullName;

        SELECT MemberId, BranchId, AccountId, FullName, Gender, DateOfBirth, PhoneNumber, Email,
               Address, ProfileImage, ReferrerMemberId, JoinDate, Status
        FROM @Result;
    END
    GO

    -- Link / gỡ link account với member (truyền @AccountId = NULL để gỡ)
    -- Trả về mã kết quả (result set 1 hàng 1 cột):
    --   -1 member không tồn tại, -2 account không tồn tại,
    --   -3 account không phải Member, -4 account đã link member khác, 0 thành công.
    IF OBJECT_ID('sp_MemberAccount_Link', 'P') IS NOT NULL DROP PROCEDURE sp_MemberAccount_Link;
    GO
    CREATE PROCEDURE sp_MemberAccount_Link
        @MemberId INT,
        @AccountId INT = NULL
    AS
    BEGIN
        SET NOCOUNT ON;

        IF NOT EXISTS (SELECT 1 FROM Members WHERE MemberId = @MemberId)
        BEGIN SELECT -1; RETURN; END

        IF @AccountId IS NOT NULL
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountId = @AccountId)
            BEGIN SELECT -2; RETURN; END

            IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountId = @AccountId AND LOWER(Role) = 'member')
            BEGIN SELECT -3; RETURN; END

            IF EXISTS (SELECT 1 FROM Members WHERE AccountId = @AccountId AND MemberId <> @MemberId)
            BEGIN SELECT -4; RETURN; END
        END

        UPDATE Members SET AccountId = @AccountId WHERE MemberId = @MemberId;
        SELECT 0;
    END
    GO

    -- Danh sách account role Member kèm memberId đã link (cho dropdown của Admin)
    IF OBJECT_ID('sp_MemberAccount_MemberOptions', 'P') IS NOT NULL DROP PROCEDURE sp_MemberAccount_MemberOptions;
    GO
    CREATE PROCEDURE sp_MemberAccount_MemberOptions
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT a.AccountId, a.Username, a.FullName, a.PhoneNumber AS Phone, a.Email, m.MemberId
        FROM Accounts a
        LEFT JOIN Members m ON m.AccountId = a.AccountId
        WHERE LOWER(a.Role) = 'member'
        ORDER BY a.FullName;
    END
    GO

    -- Gói đã đăng ký của member cho Member Portal ("My packages")
    IF OBJECT_ID('sp_MemberPortal_GetMyRegistrations', 'P') IS NOT NULL DROP PROCEDURE sp_MemberPortal_GetMyRegistrations;
    GO
    CREATE PROCEDURE sp_MemberPortal_GetMyRegistrations
        @MemberId INT
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT r.RegistrationId,
               p.PackageId,
               p.PackageName,
               p.PackageType,
               p.DurationInMonths,
               p.UnitPrice,
               r.DiscountAmount,
               r.StartDate,
               r.EndDate,
               r.Status
        FROM PackageRegistrations r
        INNER JOIN MembershipPackages p ON p.PackageId = r.PackageId
        WHERE r.MemberId = @MemberId
        ORDER BY r.StartDate DESC, r.RegistrationId DESC;
    END
    GO
