using System.ComponentModel.DataAnnotations;
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
