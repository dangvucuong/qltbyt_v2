using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain;

[Table("Quanlyvattu")]
public class DeviceSupply
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Tenvattu")]
    public string? Name { get; set; }

    [Column("Donvitinh")]
    public string? Unit { get; set; }

    [Column("Soluong")]
    public int? Quantity { get; set; }

    [Column("MaTB")]
    public int? DeviceId { get; set; }
}
