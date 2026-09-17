param(
  [string]$ConfigPath = (Join-Path $PSScriptRoot "..\QLTBYT.API\appsettings.Development.json"),
  [string]$SourceDb = "db_QLTBYT",
  [string]$TargetDb = "db_QLTBYT_NEW"
)
$ErrorActionPreference = "Continue"
$json = Get-Content $ConfigPath -Raw | ConvertFrom-Json
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $json.ConnectionStrings.DefaultConnection
$builder.InitialCatalog = $TargetDb
$cs = $builder.ConnectionString
$deleteOrder = @(
  "Xuatthuocvattu","Nhapthuocvattu","Thongbaostatus","Thongbao","LogThietbi","LogHethong","InputByDate",
  "Quanlyfile","QuanlyMa","Quanlyvattu","Quanlythietbi","QuanlyNhansu","Taikhoan","Goithau","ThongtinHS",
  "Solothuocvattu","DMthuocvattu","Nhacungcap","DMloaithuocvattu","DMkho","DMdonvitinh","EventKeyword",
  "DMTacvu","DMLoaiTB","DMTrangthai","DanhmucLoaiNS","DanhmucHoso","DanhmucDV","Country","QuanlyDV"
)
$copyOrder = @(
  "Country","DanhmucDV","DanhmucHoso","DanhmucLoaiNS","DMTrangthai","DMLoaiTB","DMTacvu","EventKeyword",
  "DMdonvitinh","DMkho","DMloaithuocvattu","Nhacungcap","DMthuocvattu","Solothuocvattu","QuanlyDV",
  "ThongtinHS","Goithau","Taikhoan","QuanlyNhansu","Quanlythietbi","Quanlyvattu","QuanlyMa","Quanlyfile",
  "InputByDate","LogHethong","LogThietbi","Thongbao","Thongbaostatus","Nhapthuocvattu","Xuatthuocvattu"
)
$conn = New-Object System.Data.SqlClient.SqlConnection($cs)
$conn.Open()
function Run-Sql($sql) {
  $cmd = $conn.CreateCommand()
  $cmd.CommandText = $sql
  $cmd.CommandTimeout = 600
  $cmd.ExecuteNonQuery()
}
function Has-Identity($table) {
  $cmd = $conn.CreateCommand()
  $cmd.CommandText = "SELECT COUNT(1) FROM sys.identity_columns ic INNER JOIN sys.tables t ON ic.object_id = t.object_id WHERE t.name = @t AND SCHEMA_NAME(t.schema_id) = 'dbo'"
  $cmd.Parameters.AddWithValue("@t", $table) | Out-Null
  return ([int]$cmd.ExecuteScalar()) -gt 0
}
Write-Host "Deleting on $TargetDb..."
foreach ($t in $deleteOrder) { try { Run-Sql "DELETE FROM dbo.[$t]" } catch { Write-Warning $_.Exception.Message } }
Write-Host "Copying from $SourceDb..."
foreach ($t in $copyOrder) {
  $insert = "INSERT INTO dbo.[$t] SELECT * FROM [$SourceDb].dbo.[$t]"
  $hasId = Has-Identity $t
  try {
    if ($hasId) {
      Run-Sql "SET IDENTITY_INSERT dbo.[$t] ON"
      Run-Sql $insert
      Run-Sql "SET IDENTITY_INSERT dbo.[$t] OFF"
    } else {
      Run-Sql $insert
    }
    Write-Host "OK $t"
  } catch {
    Write-Warning "Copy $t : $($_.Exception.Message)"
    if ($hasId) { try { Run-Sql "SET IDENTITY_INSERT dbo.[$t] OFF" } catch {} }
  }
}
try { Run-Sql "IF COL_LENGTH('Taikhoan', 'Matkhau') < 200 ALTER TABLE dbo.Taikhoan ALTER COLUMN Matkhau NVARCHAR(200) NULL" } catch {}
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COUNT(*) FROM dbo.Quanlythietbi"
$devices = $cmd.ExecuteScalar()
$cmd.CommandText = "SELECT COUNT(*) FROM dbo.Taikhoan"
$accounts = $cmd.ExecuteScalar()
$conn.Close()
Write-Host "Sync done. Devices=$devices Accounts=$accounts"