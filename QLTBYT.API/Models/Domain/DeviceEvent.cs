using System.ComponentModel.DataAnnotations;
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
