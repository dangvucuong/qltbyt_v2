using System.ComponentModel.DataAnnotations;
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
