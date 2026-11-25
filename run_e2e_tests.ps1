param(
    [string]$BaseUrl = "http://localhost:5268",
    [string]$AdminUser = "admin",
    [string]$AdminPassword = "Admin@123"
)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$results = New-Object System.Collections.Generic.List[object]

function Add-Result([string]$Name, [bool]$Passed, [string]$Detail = "") {
    $results.Add([pscustomobject]@{
        Test   = $Name
        Status = if ($Passed) { "PASS" } else { "FAIL" }
        Detail = $Detail
    })
    if ($Passed) {
        Write-Host ("  [PASS] {0} {1}" -f $Name, $(if ($Detail) { "- $Detail" } else { "" })) -ForegroundColor Green
    } else {
        Write-Host ("  [FAIL] {0} - {1}" -f $Name, $Detail) -ForegroundColor Red
    }
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [string]$Token = "",
        [object]$Body = $null
    )
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }

    $params = @{
        Uri             = "$BaseUrl$Path"
        Method          = $Method
        Headers         = $headers
        UseBasicParsing = $true
    }
    if ($null -ne $Body) {
        $json = ($Body | ConvertTo-Json -Depth 5 -Compress)
        $params["Body"] = [System.Text.Encoding]::UTF8.GetBytes($json)
        $params["ContentType"] = "application/json; charset=utf-8"
    }

    try {
        $resp = Invoke-WebRequest @params
        $parsed = $null
        if ($resp.Content) {
            try { $parsed = $resp.Content | ConvertFrom-Json } catch { $parsed = $resp.Content }
        }
        return [pscustomobject]@{ StatusCode = [int]$resp.StatusCode; Data = $parsed; Raw = $resp.Content }
    } catch [System.Net.WebException] {
        $httpResp = $_.Exception.Response
        if ($null -eq $httpResp) { throw }
        $code = [int]$httpResp.StatusCode
        $reader = New-Object System.IO.StreamReader($httpResp.GetResponseStream())
        $raw = $reader.ReadToEnd()
        $parsed = $null
        try { $parsed = $raw | ConvertFrom-Json } catch { $parsed = $raw }
        return [pscustomobject]@{ StatusCode = $code; Data = $parsed; Raw = $raw }
    }
}

Write-Host "=====================================================" -ForegroundColor Cyan
Write-Host "  Gym_Fitness - End-to-End Feature Test Suite" -ForegroundColor Cyan
Write-Host "  Target: $BaseUrl" -ForegroundColor Cyan
Write-Host "=====================================================" -ForegroundColor Cyan

# ---------------- 1. STATIC UI & SWAGGER ----------------
Write-Host "`n[1] Kiem tra giao dien tinh & Swagger..." -ForegroundColor Yellow
foreach ($p in @("/", "/admin.html", "/user.html", "/swagger/index.html")) {
    try {
        $r = Invoke-WebRequest -Uri "$BaseUrl$p" -UseBasicParsing
        Add-Result "GET $p" ($r.StatusCode -eq 200) "HTTP $($r.StatusCode)"
    } catch {
        Add-Result "GET $p" $false $_.Exception.Message
    }
}

# ---------------- 2. ADMIN LOGIN ----------------
Write-Host "`n[2] Kiem tra dang nhap Admin..." -ForegroundColor Yellow
$loginResp = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{ username = $AdminUser; password = $AdminPassword }
$adminToken = if ($loginResp.StatusCode -eq 200) { $loginResp.Data.token } else { "" }
Add-Result "Admin Login ($AdminUser)" ($loginResp.StatusCode -eq 200 -and -not [string]::IsNullOrEmpty($adminToken)) "HTTP $($loginResp.StatusCode)"

if ([string]::IsNullOrEmpty($adminToken)) {
    Write-Host "`nKhong lay duoc Admin token -> dung test cac endpoint yeu cau Admin." -ForegroundColor Red
    Write-Host "Response: $($loginResp.Raw)" -ForegroundColor DarkGray
    exit 1
}

# ---------------- 3. REPORTS & DASHBOARD ----------------
Write-Host "`n[3] Kiem tra Dashboard & Bao cao (Reports)..." -ForegroundColor Yellow
$r1 = Invoke-Api -Method GET -Path "/api/reports/all-monthly-revenue" -Token $adminToken
Add-Result "GET /api/reports/all-monthly-revenue" ($r1.StatusCode -eq 200) "rows=$(@($r1.Data).Count)"

$now = Get-Date
$r2 = Invoke-Api -Method GET -Path "/api/reports/monthly-revenue?year=$($now.Year)&month=$($now.Month)" -Token $adminToken
Add-Result "GET /api/reports/monthly-revenue" ($r2.StatusCode -eq 200) "HTTP $($r2.StatusCode)"

$r3 = Invoke-Api -Method GET -Path "/api/reports/best-selling-packages" -Token $adminToken
Add-Result "GET /api/reports/best-selling-packages" ($r3.StatusCode -eq 200) "rows=$(@($r3.Data).Count)"

$r4 = Invoke-Api -Method GET -Path "/api/reports/branch-revenue" -Token $adminToken
Add-Result "GET /api/reports/branch-revenue" ($r4.StatusCode -eq 200) "rows=$(@($r4.Data).Count)"

$r5 = Invoke-Api -Method GET -Path "/api/package-registrations/active-count" -Token $adminToken
Add-Result "GET /api/package-registrations/active-count" ($r5.StatusCode -eq 200) "active=$($r5.Data.count)"

$r6 = Invoke-Api -Method GET -Path "/api/packages/expiring-soon?days=7" -Token $adminToken
Add-Result "GET /api/packages/expiring-soon" ($r6.StatusCode -eq 200) "rows=$(@($r6.Data).Count)"

# ---------------- 4. MEMBERS CRUD + PAGING ----------------
Write-Host "`n[4] Kiem tra Quan ly Hoi vien (Members CRUD + Paging + Search)..." -ForegroundColor Yellow
$ts = Get-Date -Format "yyyyMMddHHmmss"
$mList = Invoke-Api -Method GET -Path "/api/members?page=1&pageSize=5" -Token $adminToken
Add-Result "GET /api/members (paging)" ($mList.StatusCode -eq 200) "totalCount=$($mList.Data.totalCount)"

$mCreate = Invoke-Api -Method POST -Path "/api/members" -Token $adminToken -Body @{
    branchId = 1
    fullName = "E2E Test Member $ts"
    gender   = "Nam"
    phone    = "099$($ts.Substring($ts.Length - 7))"
    email    = "e2e_$ts@test.local"
    address  = "123 Test Street"
}
$createdMemberId = if ($mCreate.StatusCode -in 200, 201) { $mCreate.Data.memberId } else { 0 }
Add-Result "POST /api/members" ($createdMemberId -gt 0) "memberId=$createdMemberId (HTTP $($mCreate.StatusCode))"

if ($createdMemberId -gt 0) {
    $mGet = Invoke-Api -Method GET -Path "/api/members/$createdMemberId" -Token $adminToken
    Add-Result "GET /api/members/$createdMemberId" ($mGet.StatusCode -eq 200) "fullName=$($mGet.Data.fullName)"

    $mUpd = Invoke-Api -Method PUT -Path "/api/members/$createdMemberId" -Token $adminToken -Body @{
        branchId = 1
        fullName = "E2E Test Member Updated $ts"
        gender   = "Nam"
        phone    = "099$($ts.Substring($ts.Length - 7))"
        email    = "e2e_$ts@test.local"
        address  = "456 Updated Street"
    }
    Add-Result "PUT /api/members/$createdMemberId" ($mUpd.StatusCode -eq 200) "HTTP $($mUpd.StatusCode)"

    $mSearch = Invoke-Api -Method GET -Path "/api/members?keyword=$ts&page=1&pageSize=5" -Token $adminToken
    Add-Result "GET /api/members?keyword=... (search)" ($mSearch.StatusCode -eq 200 -and $mSearch.Data.totalCount -ge 1) "found=$($mSearch.Data.totalCount)"

    $mDel = Invoke-Api -Method DELETE -Path "/api/members/$createdMemberId" -Token $adminToken
    Add-Result "DELETE /api/members/$createdMemberId" ($mDel.StatusCode -eq 200) "HTTP $($mDel.StatusCode)"
}

# ---------------- 5. PACKAGES CRUD + PAGING ----------------
Write-Host "`n[5] Kiem tra Quan ly Goi tap (Packages CRUD + Paging)..." -ForegroundColor Yellow
$pList = Invoke-Api -Method GET -Path "/api/packages?page=1&pageSize=5" -Token $adminToken
Add-Result "GET /api/packages (paging)" ($pList.StatusCode -eq 200) "totalCount=$($pList.Data.totalCount)"
$existingPackageId = if (@($pList.Data.items).Count -gt 0) { $pList.Data.items[0].packageId } else { 1 }

$pCreate = Invoke-Api -Method POST -Path "/api/packages" -Token $adminToken -Body @{
    name        = "E2E Package $ts"
    type        = "ThuongGoi"
    months      = 1
    price       = 300000
    description = "Goi test tu dong"
}
Add-Result "POST /api/packages" ($pCreate.StatusCode -in 200, 201) "HTTP $($pCreate.StatusCode)"

$pFind = Invoke-Api -Method GET -Path "/api/packages?keyword=$ts&page=1&pageSize=5" -Token $adminToken
$createdPkgId = if (@($pFind.Data.items).Count -gt 0) { $pFind.Data.items[0].packageId } else { 0 }
if ($createdPkgId -gt 0) {
    $pUpd = Invoke-Api -Method PUT -Path "/api/packages/$createdPkgId" -Token $adminToken -Body @{
        name        = "E2E Package Updated $ts"
        type        = "VIP"
        months      = 2
        price       = 550000
        description = "Updated"
    }
    Add-Result "PUT /api/packages/$createdPkgId" ($pUpd.StatusCode -eq 200) "HTTP $($pUpd.StatusCode)"

    $pDel = Invoke-Api -Method DELETE -Path "/api/packages/$createdPkgId" -Token $adminToken
    Add-Result "DELETE /api/packages/$createdPkgId" ($pDel.StatusCode -eq 200) "HTTP $($pDel.StatusCode)"
}

# ---------------- 6. TRAINERS CRUD + PAGING ----------------
Write-Host "`n[6] Kiem tra Quan ly Huan luyen vien (Trainers CRUD + Paging)..." -ForegroundColor Yellow
$tList = Invoke-Api -Method GET -Path "/api/trainers?page=1&pageSize=5" -Token $adminToken
Add-Result "GET /api/trainers (paging)" ($tList.StatusCode -eq 200) "totalCount=$($tList.Data.totalCount)"

$tCreate = Invoke-Api -Method POST -Path "/api/trainers" -Token $adminToken -Body @{
    branchId  = 1
    fullName  = "E2E Trainer $ts"
    specialty = "Cardio"
    phone     = "098$($ts.Substring($ts.Length - 7))"
    hireDate  = (Get-Date -Format "yyyy-MM-dd")
}
$createdTrainerId = if ($tCreate.StatusCode -in 200, 201) { $tCreate.Data.trainerId } else { 0 }
Add-Result "POST /api/trainers" ($createdTrainerId -gt 0) "trainerId=$createdTrainerId (HTTP $($tCreate.StatusCode))"

if ($createdTrainerId -gt 0) {
    $tUpd = Invoke-Api -Method PUT -Path "/api/trainers/$createdTrainerId" -Token $adminToken -Body @{
        branchId  = 1
        fullName  = "E2E Trainer Updated $ts"
        specialty = "Yoga"
        phone     = "098$($ts.Substring($ts.Length - 7))"
        hireDate  = (Get-Date -Format "yyyy-MM-dd")
    }
    Add-Result "PUT /api/trainers/$createdTrainerId" ($tUpd.StatusCode -eq 200) "HTTP $($tUpd.StatusCode)"

    $tDel = Invoke-Api -Method DELETE -Path "/api/trainers/$createdTrainerId" -Token $adminToken
    Add-Result "DELETE /api/trainers/$createdTrainerId" ($tDel.StatusCode -eq 200) "HTTP $($tDel.StatusCode)"
}

# ---------------- 7. CLASSES CRUD + SCHEDULE ----------------
Write-Host "`n[7] Kiem tra Quan ly Lop hoc (Classes CRUD + Lich hoc)..." -ForegroundColor Yellow
$cList = Invoke-Api -Method GET -Path "/api/classes?page=1&pageSize=5" -Token $adminToken
Add-Result "GET /api/classes (paging)" ($cList.StatusCode -eq 200) "totalCount=$($cList.Data.totalCount)"

$cCreate = Invoke-Api -Method POST -Path "/api/classes" -Token $adminToken -Body @{
    branchId   = 1
    name       = "E2E Yoga $ts"
    capacity   = 15
    daysOfWeek = "2,4,6"
    startTime  = "18:00:00"
    endTime    = "19:30:00"
}
$createdClassId = if ($cCreate.StatusCode -in 200, 201) { $cCreate.Data.classId } else { 0 }
Add-Result "POST /api/classes" ($createdClassId -gt 0) "classId=$createdClassId (HTTP $($cCreate.StatusCode))"

if ($createdClassId -gt 0) {
    $cUpd = Invoke-Api -Method PUT -Path "/api/classes/$createdClassId" -Token $adminToken -Body @{
        branchId   = 1
        name       = "E2E Yoga Updated $ts"
        capacity   = 20
        daysOfWeek = "3,5,7"
        startTime  = "19:00:00"
        endTime    = "20:00:00"
    }
    Add-Result "PUT /api/classes/$createdClassId" ($cUpd.StatusCode -eq 200) "HTTP $($cUpd.StatusCode)"
}

# ---------------- 8. MEMBER ACCOUNTS OPTIONS ----------------
Write-Host "`n[8] Kiem tra Lien ket Tai khoan Hoi vien..." -ForegroundColor Yellow
$maOpt = Invoke-Api -Method GET -Path "/api/accounts/member-options" -Token $adminToken
Add-Result "GET /api/accounts/member-options" ($maOpt.StatusCode -eq 200) "rows=$(@($maOpt.Data).Count)"

# ---------------- 9. MEMBER REGISTER + PORTAL + CHANGE PASSWORD ----------------
Write-Host "`n[9] Kiem tra Dang ky Hoi vien + Member Portal + Doi mat khau..." -ForegroundColor Yellow
$testUser = "e2e_user_$ts"
$testPass = "Test@1234"
$newPass  = "Test@5678"

$regResp = Invoke-Api -Method POST -Path "/api/auth/register" -Body @{
    username = $testUser
    password = $testPass
    fullName = "E2E Portal User $ts"
    phone    = "097$($ts.Substring($ts.Length - 7))"
    email    = "${testUser}@test.local"
}
Add-Result "POST /api/auth/register ($testUser)" ($regResp.StatusCode -in 200, 201) "HTTP $($regResp.StatusCode)"

$uLogin = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{ username = $testUser; password = $testPass }
$userToken = if ($uLogin.StatusCode -eq 200) { $uLogin.Data.token } else { "" }
Add-Result "Member Login ($testUser)" (-not [string]::IsNullOrEmpty($userToken)) "HTTP $($uLogin.StatusCode)"

$portalMemberId = 0
if ($userToken) {
    $me = Invoke-Api -Method GET -Path "/api/member-portal/me" -Token $userToken
    $portalMemberId = if ($me.StatusCode -eq 200 -and $me.Data.member) { $me.Data.member.memberId } else { 0 }
    Add-Result "GET /api/member-portal/me" ($me.StatusCode -eq 200 -and $portalMemberId -gt 0) "memberId=$portalMemberId"

    $meUpd = Invoke-Api -Method PUT -Path "/api/member-portal/me" -Token $userToken -Body @{
        fullName = "E2E Portal User Updated"
        gender   = "Nam"
        phone    = "097$($ts.Substring($ts.Length - 7))"
        email    = "${testUser}@test.local"
        address  = "TP.HCM"
    }
    Add-Result "PUT /api/member-portal/me" ($meUpd.StatusCode -eq 200) "HTTP $($meUpd.StatusCode)"

    $chgPw = Invoke-Api -Method POST -Path "/api/auth/change-password" -Token $userToken -Body @{
        currentPassword    = $testPass
        newPassword        = $newPass
        confirmNewPassword = $newPass
    }
    Add-Result "POST /api/auth/change-password" ($chgPw.StatusCode -eq 200) "HTTP $($chgPw.StatusCode)"

    $reLogin = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{ username = $testUser; password = $newPass }
    Add-Result "Re-login voi mat khau moi" ($reLogin.StatusCode -eq 200) "HTTP $($reLogin.StatusCode)"
    if ($reLogin.StatusCode -eq 200) { $userToken = $reLogin.Data.token }
}

# ---------------- 10. PACKAGE REGISTRATION + ATTENDANCE + CANCEL ----------------
Write-Host "`n[10] Kiem tra Dang ky goi tap + Check-in/Check-out + Huy goi..." -ForegroundColor Yellow
if ($portalMemberId -gt 0) {
    $pkgReg = Invoke-Api -Method POST -Path "/api/package-registrations" -Token $adminToken -Body @{
        memberId      = $portalMemberId
        packageId     = $existingPackageId
        branchId      = 1
        startDate     = (Get-Date -Format "yyyy-MM-dd")
        accountId     = 1
        paymentMethod = "Cash"
    }
    $regId = if ($pkgReg.StatusCode -in 200, 201) { $pkgReg.Data.registrationId } else { 0 }
    Add-Result "POST /api/package-registrations" ($regId -gt 0) "registrationId=$regId (HTTP $($pkgReg.StatusCode))"

    $memPkgs = Invoke-Api -Method GET -Path "/api/package-registrations/member/$portalMemberId" -Token $adminToken
    Add-Result "GET /api/package-registrations/member/$portalMemberId" ($memPkgs.StatusCode -eq 200) "rows=$(@($memPkgs.Data).Count)"

    $chkIn = Invoke-Api -Method POST -Path "/api/attendance/checkin" -Token $adminToken -Body @{ memberId = $portalMemberId }
    Add-Result "POST /api/attendance/checkin" ($chkIn.StatusCode -eq 200) "HTTP $($chkIn.StatusCode)"

    if ($userToken) {
        $meAfter = Invoke-Api -Method GET -Path "/api/member-portal/me" -Token $userToken
        $attId = if ($meAfter.StatusCode -eq 200 -and @($meAfter.Data.attendance).Count -gt 0) { $meAfter.Data.attendance[0].attendanceId } else { 0 }
        if ($attId -gt 0) {
            $chkOut = Invoke-Api -Method POST -Path "/api/attendance/checkout/$attId" -Token $adminToken
            Add-Result "POST /api/attendance/checkout/$attId" ($chkOut.StatusCode -eq 200) "HTTP $($chkOut.StatusCode)"
        }
    }

    if ($regId -gt 0) {
        $cancelPkg = Invoke-Api -Method POST -Path "/api/package-registrations/cancel" -Token $adminToken -Body @{
            registrationId = $regId
            reason         = "E2E automated test cancel"
        }
        Add-Result "POST /api/package-registrations/cancel" ($cancelPkg.StatusCode -eq 200) "HTTP $($cancelPkg.StatusCode)"
    }
}

# ---------------- 11. CLASS ENROLL + UNENROLL ----------------
Write-Host "`n[11] Kiem tra Dang ky & Huy lop hoc (Enroll / Unenroll)..." -ForegroundColor Yellow
if ($portalMemberId -gt 0 -and $createdClassId -gt 0 -and $userToken) {
    $enroll = Invoke-Api -Method POST -Path "/api/classes/enroll" -Token $userToken -Body @{
        memberId = $portalMemberId
        classId  = $createdClassId
    }
    $enrollId = if ($enroll.StatusCode -in 200, 201) { $enroll.Data.enrollmentId } else { 0 }
    Add-Result "POST /api/classes/enroll" ($enrollId -gt 0) "enrollmentId=$enrollId (HTTP $($enroll.StatusCode))"

    if ($enrollId -gt 0) {
        $unenroll = Invoke-Api -Method POST -Path "/api/classes/unenroll/$enrollId" -Token $userToken
        Add-Result "POST /api/classes/unenroll/$enrollId" ($unenroll.StatusCode -eq 200) "HTTP $($unenroll.StatusCode)"
    }
}

# Don dep lop test
if ($createdClassId -gt 0) {
    $cDel = Invoke-Api -Method DELETE -Path "/api/classes/$createdClassId" -Token $adminToken
    Add-Result "DELETE /api/classes/$createdClassId (cleanup)" ($cDel.StatusCode -eq 200) "HTTP $($cDel.StatusCode)"
}

# ---------------- 12. LOGIN SECURITY ----------------
Write-Host "`n[12] Kiem tra Bao mat dang nhap (sai mat khau -> 401)..." -ForegroundColor Yellow
$badLogin = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{ username = $testUser; password = "WrongPassword!" }
Add-Result "Login sai mat khau -> HTTP 401" ($badLogin.StatusCode -eq 401) "HTTP $($badLogin.StatusCode)"

# ---------------- TONG KET ----------------
$passedCount = @($results | Where-Object { $_.Status -eq "PASS" }).Count
$failedCount = @($results | Where-Object { $_.Status -eq "FAIL" }).Count
$totalCount  = $results.Count

Write-Host "`n=====================================================" -ForegroundColor Cyan
Write-Host ("  KET QUA KIEM THU: {0}/{1} PASS | {2} FAIL" -f $passedCount, $totalCount, $failedCount) -ForegroundColor $(if ($failedCount -eq 0) { "Green" } else { "Red" })
Write-Host "=====================================================" -ForegroundColor Cyan

if ($failedCount -gt 0) {
    $results | Where-Object { $_.Status -eq "FAIL" } | Format-Table -AutoSize
    exit 1
}
