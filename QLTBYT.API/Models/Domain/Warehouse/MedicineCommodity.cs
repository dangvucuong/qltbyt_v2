using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLTBYT.API.Models.Domain.Warehouse;

[Table("DMthuocvattu")]
public class MedicineCommodity
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Hanghoa")]
    public string? Name { get; set; }

    [Column("Mahanghoa")]
    public string? Code { get; set; }

    [Column("Hoatchat")]
    public string? ActiveIngredient { get; set; }

    [Column("Hamluong")]
    public string? Concentration { get; set; }

    [Column("IDloai")]
    public int? CategoryId { get; set; }

    [Column("Ngaygio")]
    public DateTime? CreatedAt { get; set; }

    [Column("UserID")]
    public int? CreatedByUserId { get; set; }
}