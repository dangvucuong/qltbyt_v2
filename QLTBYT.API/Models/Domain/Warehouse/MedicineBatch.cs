using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain.Warehouse;

[Table("Solothuocvattu")]
public class MedicineBatch
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Solo")]
    public string? BatchCode { get; set; }

    [Column("IDnhacungcap")]
    public int? SupplierId { get; set; }

    [Column("Ngaygio")]
    public DateTime? CreatedAt { get; set; }

    [Column("IDnguoinhap")]
    public int? CreatedByUserId { get; set; }
}