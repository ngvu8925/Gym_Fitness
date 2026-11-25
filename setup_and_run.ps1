param(
    [string]$SaPassword = "GymFitness@2026!",
    [switch]$SkipTests,
    [switch]$NoRun
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$containerName = "gym-fitness-sqlserver"

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  Gym_Fitness - Docker SQL + API Setup & Run" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

# 1. Kiem tra cong cu (tu dong tim docker.exe tu tien trinh Docker Desktop dang chay hoac thu muc mac dinh)
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    $dockerSearchPaths = New-Object System.Collections.Generic.List[string]

    # Lay duong dan truc tiep tu tien trinh Docker Desktop / com.docker.backend dang chay
    $dockerProcs = Get-Process -Name "Docker Desktop", "com.docker.backend" -ErrorAction SilentlyContinue
    foreach ($proc in $dockerProcs) {
        try {
            $exeDir = Split-Path -Parent $proc.Path
            $dockerSearchPaths.Add($exeDir)
            $dockerSearchPaths.Add((Join-Path $exeDir "resources\bin"))
            $dockerSearchPaths.Add((Join-Path $exeDir "bin"))
            $parentDir = Split-Path -Parent $exeDir
            if ($parentDir) {
                $dockerSearchPaths.Add((Join-Path $parentDir "resources\bin"))
                $dockerSearchPaths.Add((Join-Path $parentDir "bin"))
            }
        } catch {}
    }

    # Them cac thu muc cai dat pho bien tren Windows
    foreach ($p in @(
        "C:\Program Files\Docker\Docker\resources\bin",
        "D:\Program Files\Docker\Docker\resources\bin",
        "E:\Program Files\Docker\Docker\resources\bin",
        "$env:ProgramFiles\Docker\Docker\resources\bin",
        "$env:LOCALAPPDATA\Programs\Docker\Docker\resources\bin"
    )) {
        $dockerSearchPaths.Add($p)
    }

    foreach ($p in ($dockerSearchPaths | Select-Object -Unique)) {
        if ($p -and (Test-Path (Join-Path $p "docker.exe"))) {
            $env:PATH = "$p;$env:PATH"
            Write-Host "  Da tim thay Docker tai: $p" -ForegroundColor Gray
            break
        }
    }
}
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "Khong tim thay docker.exe. Hay chay thu: (Get-Process 'Docker Desktop').Path de xem thu muc cai Docker."
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Khong tim thay .NET SDK (dotnet). Vui long cai dat .NET 10 SDK truoc."
}

# 2. Khoi dong SQL Server container bang Docker Compose
Write-Host "`n[1/5] Khoi dong SQL Server 2022 trong Docker..." -ForegroundColor Yellow
$env:MSSQL_SA_PASSWORD = $SaPassword
docker compose up -d
if ($LASTEXITCODE -ne 0) {
    throw "docker compose up that bai. Hay dam bao Docker Desktop dang chay."
}

# Tim duong dan sqlcmd trong container (mssql-tools18 hoac mssql-tools)
$ErrorActionPreference = "Continue"
$sqlcmdPath = "/opt/mssql-tools18/bin/sqlcmd"
$null = docker exec $containerName test -x $sqlcmdPath 2>&1
if ($LASTEXITCODE -ne 0) {
    $sqlcmdPath = "/opt/mssql-tools/bin/sqlcmd"
}

Write-Host "  Dang cho SQL Server san sang..." -NoNewline -ForegroundColor Gray
$ready = $false
for ($i = 0; $i -lt 45; $i++) {
    $null = docker exec $containerName $sqlcmdPath -S localhost -U sa -P $SaPassword -C -Q "SELECT 1" -b 2>&1
    if ($LASTEXITCODE -eq 0) {
        $ready = $true
        break
    }
    Write-Host "." -NoNewline -ForegroundColor DarkGray
    Start-Sleep -Seconds 2
}
$ErrorActionPreference = "Stop"

if (-not $ready) {
    Write-Host ""
    throw "SQL Server trong Docker chua san sang sau 90 giay. Kiem tra bang lenh: docker logs $containerName"
}
Write-Host " SAN SANG!" -ForegroundColor Green

# 3. Khoi tao Database QL_GymFitness_Pro & chay toan bo script SQL trong container
Write-Host "`n[2/5] Khoi tao database QL_GymFitness_Pro va Stored Procedures..." -ForegroundColor Yellow

function Invoke-DockerSqlFile([string]$Database, [string]$ScriptName) {
    Write-Host "  -> /scripts/$ScriptName" -ForegroundColor Gray
    docker exec $containerName $sqlcmdPath -S localhost -U sa -P $SaPassword -C -I -b -d $Database -i "/scripts/$ScriptName"
    if ($LASTEXITCODE -ne 0) {
        throw "Loi khi chay script SQL: $ScriptName"
    }
}

# Script 00 tao DB QL_GymFitness_Pro tu master
Invoke-DockerSqlFile -Database "master" -ScriptName "00_init_schema_and_base_procs.sql"

# Cac script Stored Procedures / schema bo sung theo dung thu tu
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
    Invoke-DockerSqlFile -Database "QL_GymFitness_Pro" -ScriptName $script
}
Write-Host "  Khoi tao Database thanh cong!" -ForegroundColor Green

# 4. Cau hinh dotnet user-secrets
Write-Host "`n[3/5] Cau hinh ConnectionString & JwtSettings (dotnet user-secrets)..." -ForegroundColor Yellow

$connStr = "Server=localhost,1433;Database=QL_GymFitness_Pro;User Id=sa;Password=$SaPassword;TrustServerCertificate=True;"

dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connStr | Out-Null
dotnet user-secrets set "JwtSettings:SecretKey" "GymFitness_SuperSecretKey_ChangeMe_2026_AtLeast32Chars!" | Out-Null
dotnet user-secrets set "JwtSettings:Issuer" "GymManagement.Api" | Out-Null
dotnet user-secrets set "JwtSettings:Audience" "GymManagement.Client" | Out-Null
dotnet user-secrets set "JwtSettings:ExpiryMinutes" "60" | Out-Null
Write-Host "  Da luu User Secrets thanh cong!" -ForegroundColor Green

# 5. Restore, Build, Test
Write-Host "`n[4/5] Restore va Build du an..." -ForegroundColor Yellow
dotnet restore GymManagement.Api.slnx
if ($LASTEXITCODE -ne 0) { throw "dotnet restore that bai." }

dotnet build GymManagement.Api.slnx --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet build that bai." }

if (-not $SkipTests) {
    Write-Host "`n  Dang chay Unit Tests..." -ForegroundColor Yellow
    dotnet test GymManagement.Api.Tests\GymManagement.Api.Tests.csproj --no-build
    if ($LASTEXITCODE -ne 0) { throw "dotnet test co test that bai." }
}

# 6. Chay ung dung
if ($NoRun) {
    Write-Host "`n[5/5] Hoan tat cai dat! Chay ung dung bang: dotnet run --launch-profile http" -ForegroundColor Green
    exit 0
}

Write-Host "`n[5/5] Khoi dong GymManagement.Api..." -ForegroundColor Green
Write-Host "-----------------------------------------------" -ForegroundColor Cyan
Write-Host "  SQL Server Docker       : localhost,1433 (sa / $SaPassword)" -ForegroundColor Gray
Write-Host "  Trang chu / Dang nhap   : http://localhost:5268" -ForegroundColor White
Write-Host "  Admin Portal            : http://localhost:5268/admin.html" -ForegroundColor White
Write-Host "  Member Portal           : http://localhost:5268/user.html" -ForegroundColor White
Write-Host "  Swagger API Docs        : http://localhost:5268/swagger" -ForegroundColor White
Write-Host "  Tai khoan Admin mac dinh: admin / Admin@123" -ForegroundColor Yellow
Write-Host "-----------------------------------------------`n" -ForegroundColor Cyan

dotnet run --launch-profile http --no-build
