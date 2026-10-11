#Requires -RunAsAdministrator
<#
  Tạo site IIS cho QLTBYT API trên cổng 8016.
  Chạy trên server (PowerShell Admin):
    .\setup-iis-api.ps1 -SitePath "C:\QLTBYT\api"
#>
param(
    [string]$SiteName = "QLTBYT-API",
    [string]$AppPoolName = "QLTBYT-API-Pool",
    [string]$SitePath = "C:\QLTBYT\api",
    [int]$Port = 8016
)

Import-Module WebAdministration -ErrorAction Stop

if (-not (Test-Path $SitePath)) {
    throw "Không thấy thư mục publish: $SitePath. Hãy copy D:\deploy\qltbyt-api vào đây trước."
}

# Thư mục ghi file / log
@(
    (Join-Path $SitePath "logs"),
    (Join-Path $SitePath "FILELUU"),
    (Join-Path $SitePath "Captures"),
    (Join-Path $SitePath "HDBaotri")
) | ForEach-Object {
    New-Item -ItemType Directory -Force -Path $_ | Out-Null
}

# App Pool: No Managed Code + AlwaysRunning
if (Test-Path "IIS:\AppPools\$AppPoolName") {
    Write-Host "App Pool đã tồn tại: $AppPoolName"
} else {
    New-WebAppPool -Name $AppPoolName | Out-Null
}

Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ""
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name startMode -Value "AlwaysRunning"
Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name processModel.idleTimeout -Value ([TimeSpan]::FromMinutes(0))

# Site
if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    Write-Host "Site đã tồn tại, cập nhật physicalPath / binding..."
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $SitePath
    Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
} else {
    New-Website -Name $SiteName `
        -Port $Port `
        -PhysicalPath $SitePath `
        -ApplicationPool $AppPoolName `
        -Force | Out-Null
}

# Binding http://*:8016
$bindings = Get-WebBinding -Name $SiteName
$hasPort = $false
foreach ($b in $bindings) {
    if ($b.bindingInformation -like "*:${Port}:*") { $hasPort = $true }
}
if (-not $hasPort) {
    New-WebBinding -Name $SiteName -Protocol http -Port $Port -IPAddress "*"
}

# Quyền ghi cho IIS AppPool identity
$aclUser = "IIS AppPool\$AppPoolName"
icacls $SitePath /grant "${aclUser}:(OI)(CI)M" /T | Out-Null

# Firewall
$ruleName = "QLTBYT API $Port"
if (-not (Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue)) {
    New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Protocol TCP -LocalPort $Port -Action Allow | Out-Null
}

Start-WebAppPool -Name $AppPoolName
Start-Website -Name $SiteName

Write-Host ""
Write-Host "OK. API IIS: http://$((Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.IPAddress -notlike '127.*' } | Select-Object -First 1).IPAddress):$Port"
Write-Host "Kiểm tra: http://127.0.0.1:$Port/api/health"
Write-Host "Nếu 502.5: cài ASP.NET Core 7 Hosting Bundle, xem logs\stdout*.log"
