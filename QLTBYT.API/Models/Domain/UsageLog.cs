using System.ComponentModel.DataAnnotations;
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
