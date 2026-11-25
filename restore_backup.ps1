param(
    [string]$BakPath = "C:\Users\dat70\Downloads\QL_GymFitness_Pro.bak",
    [string]$SaPassword = "GymFitness@2026!"
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$containerName = "gym-fitness-sqlserver"

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "  Gym_Fitness - Restore QL_GymFitness_Pro.bak" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan

# 1. Kiem tra file .bak
if (-not (Test-Path $BakPath)) {
    throw "Khong tim thay file backup tai: $BakPath"
}

# 2. Tim docker.exe
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    $dockerSearchPaths = New-Object System.Collections.Generic.List[string]
    $dockerProcs = Get-Process -Name "Docker Desktop", "com.docker.backend" -ErrorAction SilentlyContinue
    foreach ($proc in $dockerProcs) {
        try {
            $exeDir = Split-Path -Parent $proc.Path
            $dockerSearchPaths.Add($exeDir)
            $dockerSearchPaths.Add((Join-Path $exeDir "resources\bin"))
            $parentDir = Split-Path -Parent $exeDir
            if ($parentDir) { $dockerSearchPaths.Add((Join-Path $parentDir "resources\bin")) }
        } catch {}
    }
    foreach ($p in @(
        "C:\DockerDesktop\resources\bin",
        "C:\Program Files\Docker\Docker\resources\bin",
        "D:\Program Files\Docker\Docker\resources\bin"
    )) { $dockerSearchPaths.Add($p) }

    foreach ($p in ($dockerSearchPaths | Select-Object -Unique)) {
        if ($p -and (Test-Path (Join-Path $p "docker.exe"))) {
            $env:PATH = "$p;$env:PATH"
            break
        }
    }
}
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Khong tim thay docker.exe."
}

# Dam bao container dang chay
docker compose up -d | Out-Null

$ErrorActionPreference = "Continue"
$sqlcmdPath = "/opt/mssql-tools18/bin/sqlcmd"
$null = docker exec $containerName test -x $sqlcmdPath 2>&1
if ($LASTEXITCODE -ne 0) { $sqlcmdPath = "/opt/mssql-tools/bin/sqlcmd" }
$ErrorActionPreference = "Stop"

# 3. Copy file .bak vao container
Write-Host "`n[1/4] Copy file backup vao container Docker..." -ForegroundColor Yellow
docker exec $containerName mkdir -p /var/opt/mssql/backup
docker cp $BakPath "${containerName}:/var/opt/mssql/backup/QL_GymFitness_Pro.bak"
if ($LASTEXITCODE -ne 0) { throw "Loi khi copy file .bak vao container." }
Write-Host "  Copy thanh cong!" -ForegroundColor Green

# 4. Doc ten logical file ben trong .bak va Restore
Write-Host "`n[2/4] Dang doc cau truc file .bak va Restore QL_GymFitness_Pro..." -ForegroundColor Yellow

$restoreSql = @"
SET NOCOUNT ON;
DECLARE @FileList TABLE (
    LogicalName NVARCHAR(128),
    PhysicalName NVARCHAR(260),
    [Type] CHAR(1),
    FileGroupName NVARCHAR(128),
    Size NUMERIC(20,0),
    MaxSize NUMERIC(20,0),
    FileId BIGINT,
    CreateLSN NUMERIC(25,0),
    DropLSN NUMERIC(25,0),
    UniqueId UNIQUEIDENTIFIER,
    ReadOnlyLSN NUMERIC(25,0),
    ReadWriteLSN NUMERIC(25,0),
    BackupSizeInBytes BIGINT,
    SourceBlockSize INT,
    FileGroupId INT,
    LogGroupGUID UNIQUEIDENTIFIER,
    DifferentialBaseLSN NUMERIC(25,0),
    DifferentialBaseGUID UNIQUEIDENTIFIER,
    IsReadOnly BIT,
    IsPresent BIT,
    TDEThumbprint VARBINARY(32),
    SnapshotUrl NVARCHAR(360)
);

INSERT INTO @FileList
EXEC('RESTORE FILELISTONLY FROM DISK = N''/var/opt/mssql/backup/QL_GymFitness_Pro.bak''');

DECLARE @DataLogical NVARCHAR(128) = (SELECT TOP 1 LogicalName FROM @FileList WHERE [Type] = 'D');
DECLARE @LogLogical  NVARCHAR(128) = (SELECT TOP 1 LogicalName FROM @FileList WHERE [Type] = 'L');

PRINT 'Data logical name: ' + ISNULL(@DataLogical, 'NULL');
PRINT 'Log logical name:  ' + ISNULL(@LogLogical, 'NULL');

IF DB_ID(N'QL_GymFitness_Pro') IS NOT NULL
BEGIN
    ALTER DATABASE QL_GymFitness_Pro SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
END

DECLARE @Cmd NVARCHAR(MAX) = N'RESTORE DATABASE QL_GymFitness_Pro '
    + N'FROM DISK = N''/var/opt/mssql/backup/QL_GymFitness_Pro.bak'' '
    + N'WITH REPLACE, '
    + N'MOVE N''' + @DataLogical + N''' TO N''/var/opt/mssql/data/QL_GymFitness_Pro.mdf'', '
    + N'MOVE N''' + @LogLogical  + N''' TO N''/var/opt/mssql/data/QL_GymFitness_Pro_log.ldf'';';

EXEC(@Cmd);

ALTER DATABASE QL_GymFitness_Pro SET MULTI_USER;
PRINT 'RESTORE THANH CONG!';
"@

docker exec $containerName $sqlcmdPath -S localhost -U sa -P $SaPassword -C -b -d master -Q $restoreSql
if ($LASTEXITCODE -ne 0) { throw "Restore database that bai." }
Write-Host "  Restore QL_GymFitness_Pro thanh cong!" -ForegroundColor Green

# 5. Chay 8 script Stored Procedures / schema bo sung tren DB vua restore
Write-Host "`n[3/4] Cap nhat Stored Procedures & schema bo sung (Database/*.sql)..." -ForegroundColor Yellow

$dbScripts = @(
    "UpgradeLegacyPasswords.sql",
    "procedures_member_accounts.sql",
    "procedures_crud.sql",
    "procedures_class_schedule.sql",
    "procedures_paging.sql",
    "procedures_member_optional_fields.sql",
    "procedures_cancellations.sql",
    "procedures_login_security.sql"
)

foreach ($script in $dbScripts) {
    Write-Host "  -> /scripts/$script" -ForegroundColor Gray
    docker exec $containerName $sqlcmdPath -S localhost -U sa -P $SaPassword -C -I -b -d QL_GymFitness_Pro -i "/scripts/$script"
    if ($LASTEXITCODE -ne 0) {
        throw "Loi khi chay script SQL: $script"
    }
}

# Dam bao tai khoan admin co hash hop le cho Admin@123 (tranh loi hash 59 ky tu cua UpgradeLegacyPasswords.sql)
$fixAdminSql = @"
IF EXISTS (SELECT 1 FROM dbo.Accounts WHERE Username = 'admin' AND LEN(Password) = 59)
BEGIN
    UPDATE dbo.Accounts
    SET Password = 'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7',
        FailedLoginCount = 0,
        LockoutEnd = NULL
    WHERE Username = 'admin';
    PRINT 'Da sua hash mat khau admin -> Admin@123 (tu nang cap BCrypt khi login).';
END
ELSE
BEGIN
    UPDATE dbo.Accounts SET FailedLoginCount = 0, LockoutEnd = NULL WHERE Username = 'admin';
END
"@
docker exec $containerName $sqlcmdPath -S localhost -U sa -P $SaPassword -C -b -d QL_GymFitness_Pro -Q $fixAdminSql

# 6. Thong ke du lieu vua import
Write-Host "`n[4/4] Thong ke du lieu da import vao QL_GymFitness_Pro:" -ForegroundColor Yellow
$summarySql = @"
SET NOCOUNT ON;
SELECT t.name AS TableName, SUM(p.rows) AS [RowCount]
FROM sys.tables t
INNER JOIN sys.partitions p ON t.object_id = p.object_id AND p.index_id IN (0, 1)
GROUP BY t.name
ORDER BY t.name;

SELECT AccountId, Username, FullName, Role, Status, LEN(Password) AS HashLen
FROM dbo.Accounts
ORDER BY AccountId;
"@
docker exec $containerName $sqlcmdPath -S localhost -U sa -P $SaPassword -C -d QL_GymFitness_Pro -Q $summarySql

Write-Host "`nHoan tat import du lieu tu $BakPath!" -ForegroundColor Green
