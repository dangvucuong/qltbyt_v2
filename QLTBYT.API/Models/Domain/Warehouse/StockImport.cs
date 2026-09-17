using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain.Warehouse;

[Table("Nhapthuocvattu")]
public class StockImport
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDhanghoa")]
    public int CommodityId { get; set; }

    [Column("IDsolo")]
    public int? BatchId { get; set; }

    [Column("IDkho")]
    public int? WarehouseId { get; set; }

    [Column("Hansudung")]
    public DateTime? ExpiryDate { get; set; }

    [Column("SLnhap")]
    public int? Quantity { get; set; }

    [Column("Dongia")]
    public int? UnitPrice { get; set; }

    [Column("SLtonkho")]
    public int? StockBalance { get; set; }

    [Column("IDnguoinhap")]
    public int? CreatedByUserId { get; set; }

    [Column("Ngaygio")]
    public DateTime? CreatedAt { get; set; }
}