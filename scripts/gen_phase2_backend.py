# Generates Phase 2 backend files (UTF-8). Run: python scripts/gen_phase2_backend.py
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
API = ROOT / "QLTBYT.API"

def w(rel: str, content: str) -> None:
    p = API / rel.replace("/", "\\") if "/" in rel else API / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(content, encoding="utf-8", newline="\n")

# --- Domain ---
w("Models/Domain/DeviceEvent.cs", """using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("LogThietbi")]
public class DeviceEvent
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("MaTB")]
    public int? DeviceId { get; set; }

    [Column("Loai")]
    public int? TypeId { get; set; }

    [Column("Noidung")]
    public string? Content { get; set; }

    [Column("Ngaygio")]
    public DateTime? EventDateTime { get; set; }

    [Column("NguoiQL")]
    public int? ManagerUserId { get; set; }

    [Column("Taikhoan")]
    public string? AccountUsername { get; set; }

    [Column("Trangthai")]
    public int? StatusId { get; set; }

    [Column("Chiphi")]
    public double? Cost { get; set; }

    [Column("Keyword")]
    public string? Keyword { get; set; }

    [Column("Idbatdau")]
    public int? StartEventId { get; set; }

    public virtual Device? Device { get; set; }
}
""")

w("Models/Domain/SystemAuditLog.cs", """using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("LogHethong")]
public class SystemAuditLog
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Taikhoan")]
    public int? UserId { get; set; }

    [Column("Noidung")]
    public string? Content { get; set; }

    [Column("Thoigian")]
    public DateTime? Timestamp { get; set; }

    [Column("MaTB")]
    public int? DeviceId { get; set; }
}
""")

w("Models/Domain/Notification.cs", """using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Thongbao")]
public class Notification
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Noidung")]
    public string? Content { get; set; }

    [Column("Ngaygio")]
    public DateTime? CreatedAt { get; set; }

    [Column("Nguoitao")]
    public string? CreatedByUserId { get; set; }

    [Column("Trangthai")]
    public int? Status { get; set; }

    [Column("Tieude")]
    public string? Title { get; set; }
}
""")

w("Models/Domain/NotificationReadStatus.cs", """using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Thongbaostatus")]
public class NotificationReadStatus
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDthongbao")]
    public int? NotificationId { get; set; }

    [Column("IDuser")]
    public int? UserId { get; set; }

    [Column("read_status")]
    public int? ReadStatus { get; set; }

    public virtual Notification? Notification { get; set; }
}
""")

w("Models/Domain/Personnel.cs", """using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("QuanlyNhansu")]
public class Personnel
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Hoten")]
    public string? FullName { get; set; }

    [Column("SoCMND")]
    public string? IdentityNumber { get; set; }

    [Column("Dienthoai")]
    public string? Phone { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("MaDV")]
    public int? OrganizationId { get; set; }

    [Column("LoaiNhansu")]
    public int? PersonnelTypeId { get; set; }

    [Column("IDTaikhoan")]
    public int? UserAccountId { get; set; }
}
""")

w("Models/Domain/UsageLog.cs", """using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("InputByDate")]
public class UsageLog
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("MaTB")]
    public string DeviceCode { get; set; } = string.Empty;

    [Column("Sogio")]
    public int? Hours { get; set; }

    [Column("Soca")]
    public int? Sessions { get; set; }

    [Column("InputDate")]
    public DateTime InputDate { get; set; }

    [Column("Taikhoan")]
    public int? UserId { get; set; }
}
""")

print("domains ok")
