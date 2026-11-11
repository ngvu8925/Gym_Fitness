-- ============================================================
-- Bước 1: Hỗ trợ chuyển đổi password hashing sang BCrypt
-- Chạy thủ công trên DB QL_GymFitness_Pro (KHÔNG tự chạy)
--
-- BCrypt không thể so sánh hash trong SQL. Ứng dụng mới:
--   - Tài khoản legacy (hash SHA-256 64 ký tự hex): khi đăng nhập
--     đúng, ứng dụng TỰ ĐỘNG nâng cấp hash sang BCrypt.
--   - Tài khoản legacy KHÔNG đăng nhập lại sẽ giữ hash cũ cho đến khi
--     ai đó đăng nhập lần đầu.
--
-- Script này chỉ làm 2 việc:
--   1. Kiểm kê tài khoản còn hash legacy.
--   2. Seed tài khoản admin dự phòng với hash BCrypt của mật khẩu
--      tạm "Admin@123" (BẮT BUỘC đổi mật khẩu sau khi đăng nhập).
-- ============================================================

-- 1) Kiểm kê tài khoản legacy (SELECT-only, an toàn)
SELECT AccountId, Username, FullName, Role, Status,
       CASE WHEN LEN(Password) = 64 THEN 'Legacy SHA-256' ELSE 'BCrypt (mới)' END AS HashType
FROM Accounts;

-- 2) (Tuỳ chọn) Seed tài khoản admin dự phòng.
-- Hash dưới đây là BCrypt(work factor 11) của chuỗi: Admin@123
-- Câu lệnh tạo hash: PasswordHelper.Hash("Admin@123") trong C#.
IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Username = 'admin')
BEGIN
    INSERT INTO Accounts (Username, Password, FullName, Role, Status, CreatedAt)
    VALUES (
        'admin',
        '$2a$11$ltjSunSlYCIT0hZpB6v4D.AY0PRiw7eZN6FsY1G.I7kOi2Ac2uBPW', -- BCrypt của "Admin@123"
        'System Administrator',
        'Admin',
        1,
        GETDATE()
    );
    PRINT 'Đã tạo tài khoản admin dự phòng. Mật khẩu tạm: Admin@123';
END
ELSE
    PRINT 'Tài khoản admin đã tồn tại, bỏ qua.';
