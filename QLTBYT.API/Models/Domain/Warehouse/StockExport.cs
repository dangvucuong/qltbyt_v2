using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain.Warehouse;

[Table("Xuatthuocvattu")]
public class StockExport
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDnhap")]
    public int ImportId { get; set; }

    [Column("SLxuat")]
    public int? Quantity { get; set; }

    [Column("Ghichu")]
    public string? Note { get; set; }

    [Column("Dongia")]
    public int? UnitPrice { get; set; }

    [Column("IDnguoinhap")]
    public int? CreatedByUserId { get; set; }

    [Column("Ngaygio")]
    public DateTime? CreatedAt { get; set; }
}